using UnityEngine;

public class CombatUnitHandle : MonoBehaviour
{
    public bool isPlayerUnit;

    private void OnDestroy()
    {
        if (CombatStateRegistry.Instance == null) return;

        if (isPlayerUnit)
            CombatStateRegistry.Instance.UnregisterPlayerUnit(transform);
    }
}