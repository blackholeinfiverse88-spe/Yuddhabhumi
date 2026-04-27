using UnityEngine;

[CreateAssetMenu(
    fileName = "RangedClassProfile",
    menuName = "Game/Troop Class/Ranged"
)]
public class RangedClassProfile : TroopClassProfile
{
    public override bool ShouldChaseEnemy(float distanceToEnemy)
        => distanceToEnemy > attackRange && distanceToEnemy <= aggroRadius;

    public override bool ShouldDisengage(float distanceToEnemy)
        => distanceToEnemy < attackRange * 0.6f;

    public override int GetTargetPriority(Troop enemy)
    {
        return enemy.troopType switch
        {
            TroopType.Melee => 10,
            TroopType.Tank  => 6,
            TroopType.Ranged => 4,
            _ => 0
        };
    }
}
