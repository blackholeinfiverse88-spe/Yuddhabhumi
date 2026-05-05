using System;
using System.Collections.Generic;
using UnityEngine;

public enum DecisionType { DeployCard = 1 }
public enum KeshavMockMode { Allow, Reject, Modify }
public enum ValidationStatus { Allow, Reject, Modify }
public enum ExecutionStatus { Executed, Blocked }

[Serializable]
public struct DecisionPacket
{
    public int packet_id;          // deterministic sequence id
    public DecisionType type;
    public int slot_index;

    // Trace-only (not used for gameplay logic)
    public string card_name;
    public float card_cost;        // FIX: float because UnitData.cost is float in your project
    public int trace_frame;

    // Execution payload (existing object reference)
    public UnitData card;
}

public struct ValidationResult
{
    public ValidationStatus status;
    public DecisionPacket packet;  // if modify -> updated packet
    public string reason;
}

public readonly struct ExecutionToken
{
    public readonly int token_id;
    public readonly int packet_id;

    public ExecutionToken(int tokenId, int packetId)
    {
        token_id = tokenId;
        packet_id = packetId;
    }
}

/// <summary>
/// SAARTHI: active execution authorization context.
/// Spawner will block execution unless a valid token context is active.
/// </summary>
public static class SaarthiExecutionContext
{
    private static int _activeTokenId = 0;
    public static bool IsAuthorized => _activeTokenId != 0;
    public static int ActiveTokenId => _activeTokenId;

    public static void Enter(int tokenId) => _activeTokenId = tokenId;
    public static void Exit() => _activeTokenId = 0;
}

/// <summary>
/// Governed execution node:
/// Input -> DecisionPacket -> KESHAV validate -> SAARTHI token -> Execute (token required)
/// </summary>
public class TantraExecutionNode : MonoBehaviour
{
    [Header("Execution Target")]
    [SerializeField] private PlayerTroopSpawnerVR spawner;

    [Header("KESHAV Mock")]
    public KeshavMockMode mode = KeshavMockMode.Allow;

    [Tooltip("Used only when mode = Modify. If null, modify returns same card.")]
    public UnitData forcedCardOnModify;

    private int _nextPacketId = 1;
    private int _nextTokenId = 1;

    private readonly Dictionary<int, int> _tokenToPacket = new Dictionary<int, int>(128);
    private readonly HashSet<int> _consumedTokens = new HashSet<int>();

    public (ExecutionStatus status, string reason) SubmitDeploy(int slotIndex, UnitData card)
    {
        if (card == null)
            return (ExecutionStatus.Blocked, "null_card");

        var packet = new DecisionPacket
        {
            packet_id = _nextPacketId++,
            type = DecisionType.DeployCard,
            slot_index = slotIndex,
            card_name = card.unitName,
            card_cost = card.cost,              // FIX: float -> float
            trace_frame = Time.frameCount,      // trace only
            card = card
        };

        Debug.Log($"[INPUT->PACKET] id={packet.packet_id} type={packet.type} slot={packet.slot_index} card={packet.card_name} cost={packet.card_cost:0.##}");

        var validation = ValidateDecision(packet);
        Debug.Log($"[KESHAV] packet_id={packet.packet_id} status={validation.status} reason={validation.reason}");

        if (validation.status == ValidationStatus.Reject)
        {
            Debug.Log($"[EXEC_BLOCKED] packet_id={packet.packet_id} reason={validation.reason}");
            return (ExecutionStatus.Blocked, validation.reason);
        }

        var token = IssueToken(validation.packet);
        Debug.Log($"[SAARTHI] token_issued token_id={token.token_id} packet_id={token.packet_id}");

        return Execute(validation.packet, token);
    }

    private ValidationResult ValidateDecision(DecisionPacket packet)
    {
        switch (mode)
        {
            case KeshavMockMode.Reject:
                return new ValidationResult
                {
                    status = ValidationStatus.Reject,
                    packet = packet,
                    reason = "mock_reject"
                };

            case KeshavMockMode.Modify:
            {
                var modified = packet;

                if (forcedCardOnModify != null)
                {
                    modified.card = forcedCardOnModify;
                    modified.card_name = forcedCardOnModify.unitName;
                    modified.card_cost = forcedCardOnModify.cost; // FIX: float
                    return new ValidationResult
                    {
                        status = ValidationStatus.Modify,
                        packet = modified,
                        reason = "mock_modify_forced_card"
                    };
                }

                return new ValidationResult
                {
                    status = ValidationStatus.Modify,
                    packet = modified,
                    reason = "mock_modify_no_change"
                };
            }

            default:
                return new ValidationResult
                {
                    status = ValidationStatus.Allow,
                    packet = packet,
                    reason = "mock_allow"
                };
        }
    }

    private ExecutionToken IssueToken(DecisionPacket packet)
    {
        int tokenId = _nextTokenId++;
        _tokenToPacket[tokenId] = packet.packet_id;
        return new ExecutionToken(tokenId, packet.packet_id);
    }

    private bool IsTokenValid(ExecutionToken token, DecisionPacket packet)
    {
        if (_consumedTokens.Contains(token.token_id))
            return false;

        if (!_tokenToPacket.TryGetValue(token.token_id, out int mappedPacketId))
            return false;

        return mappedPacketId == packet.packet_id && token.packet_id == packet.packet_id;
    }

    private void ConsumeToken(ExecutionToken token) => _consumedTokens.Add(token.token_id);

    private (ExecutionStatus status, string reason) Execute(DecisionPacket packet, ExecutionToken token)
    {
        if (!IsTokenValid(token, packet))
        {
            Debug.LogError($"[EXEC_BLOCKED] invalid_or_consumed_token token_id={token.token_id} packet_id={packet.packet_id}");
            return (ExecutionStatus.Blocked, "invalid_or_consumed_token");
        }

        ConsumeToken(token);

        if (spawner == null)
        {
            Debug.LogError("[EXEC_BLOCKED] spawner_missing");
            return (ExecutionStatus.Blocked, "spawner_missing");
        }

        Debug.Log($"[EXECUTE] packet_id={packet.packet_id} token_id={token.token_id} card={packet.card_name}");

        SaarthiExecutionContext.Enter(token.token_id);
        try
        {
            spawner.SpawnUnit(packet.card); // existing gameplay method (now governed)
        }
        finally
        {
            SaarthiExecutionContext.Exit();
        }

        return (ExecutionStatus.Executed, "executed");
    }
}