using UnityEngine; // This line fixes the error

public class AttackPoint : MonoBehaviour
{
    public bool isOccupied;
    public Troop currentTroop;

    public void Occupy(Troop troop)
    {
        isOccupied = true;
        currentTroop = troop;
    }

    public void Release()
    {
        isOccupied = false;
        currentTroop = null;
    }
}