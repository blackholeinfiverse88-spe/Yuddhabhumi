using UnityEngine;

[CreateAssetMenu(
    fileName = "LightClassProfile",
    menuName = "Game/Troop Class/Light"
)]
public class LightClassProfile : TroopClassProfile
{
    public override bool ShouldChaseEnemy(float distanceToEnemy)
        => distanceToEnemy <= aggroRadius;

    public override bool ShouldDisengage(float distanceToEnemy)
        => distanceToEnemy > aggroRadius * 0.8f;

    public override int GetTargetPriority(Troop enemy)
        => enemy.troopType == TroopType.Ranged ? 10 : 5;
}
