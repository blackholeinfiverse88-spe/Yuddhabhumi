using UnityEngine;

public static class MatchStats
{
    // The Data we want to track
    public static int unitsDeployed = 0;
    public static float damageDealt = 0;
    public static int energySpent = 0;
    public static float matchDuration = 0;

    // Call this at the start of every match to wipe the slate clean
    public static void Reset()
    {
        unitsDeployed = 0;
        damageDealt = 0;
        energySpent = 0;
        matchDuration = 0;
    }
}