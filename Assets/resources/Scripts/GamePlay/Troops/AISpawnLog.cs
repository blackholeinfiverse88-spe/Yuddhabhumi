using UnityEngine;

[System.Serializable]
public struct AISpawnLog
{
    public SpawnReason reason;
    public TroopType troopType;
    public float gameTime;

    public AISpawnLog(SpawnReason r, TroopType t)
    {
        reason = r;
        troopType = t;
        gameTime = Time.time;
    }
}
