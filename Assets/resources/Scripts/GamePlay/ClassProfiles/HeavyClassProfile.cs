using UnityEngine;

[CreateAssetMenu(
    fileName = "HeavyClassProfile",
    menuName = "Game/Troop Class/Heavy"
)]
public class HeavyClassProfile : TroopClassProfile
{
    public override bool ShouldChaseEnemy(float distanceToEnemy)
        => distanceToEnemy <= aggroRadius * 0.7f;

    public override bool ShouldDisengage(float distanceToEnemy)
        => distanceToEnemy > aggroRadius;

    public override int GetTargetPriority(Troop enemy)
        => enemy.troopType == TroopType.Tank ? 10 : 6;
}
