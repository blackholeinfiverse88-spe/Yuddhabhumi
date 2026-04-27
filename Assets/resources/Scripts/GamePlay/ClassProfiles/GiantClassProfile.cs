using UnityEngine;

[CreateAssetMenu(
    fileName = "GiantClassProfile",
    menuName = "Game/Troop Class/Giant"
)]
public class GiantClassProfile : TroopClassProfile
{
    public override bool ShouldChaseEnemy(float distanceToEnemy)
        => false;

    public override bool ShouldDisengage(float distanceToEnemy)
        => true;

    public override int GetTargetPriority(Troop enemy)
        => 0;
}
