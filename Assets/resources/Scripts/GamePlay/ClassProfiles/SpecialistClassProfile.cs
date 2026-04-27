using UnityEngine;

[CreateAssetMenu(menuName = "Game/Troop Class/Specialist")]
public class SpecialistClassProfile : TroopClassProfile
{
    public override bool ShouldChaseEnemy(float distanceToEnemy)
    {
        // Specialists engage selectively
        return distanceToEnemy <= aggroRadius * 0.8f;
    }

    public override bool ShouldDisengage(float distanceToEnemy)
    {
        // Specialists disengage aggressively to survive
        return distanceToEnemy > aggroRadius * 0.6f;
    }

    public override int GetTargetPriority(Troop enemy)
    {
        // Specialists hunt high-value targets
        switch (enemy.troopType)
        {
            case TroopType.Ranged:
                return 12;
            case TroopType.Tank:
                return 8;
            case TroopType.Melee:
                return 6;
            default:
                return 0;
        }
    }
}
