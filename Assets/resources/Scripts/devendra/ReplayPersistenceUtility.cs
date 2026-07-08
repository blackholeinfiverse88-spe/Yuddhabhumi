using System;
using System.IO;
using UnityEngine;

public static class ReplayPersistenceUtility
{
    public static string FindLatestSessionId()
    {
        string root = ReplayPersistence.ReplaysRoot;
        if (!Directory.Exists(root))
            return null;

        string latestId = null;
        DateTime latestTime = DateTime.MinValue;

        foreach (var dir in Directory.GetDirectories(root))
        {
            string sessionId = Path.GetFileName(dir);
            string headerPath = ReplayPersistence.GetHeaderPath(sessionId);
            if (!File.Exists(headerPath))
                continue;

            try
            {
                var header = ReplayPersistence.LoadHeader(sessionId);
                if (DateTime.TryParse(header.created_utc, out var dt))
                {
                    if (dt > latestTime)
                    {
                        latestTime = dt;
                        latestId = sessionId;
                    }
                }
                else
                {
                    // fallback: directory write time
                    var w = Directory.GetLastWriteTimeUtc(dir);
                    if (w > latestTime)
                    {
                        latestTime = w;
                        latestId = sessionId;
                    }
                }
            }
            catch
            {
                // corrupted header -> skip
            }
        }

        return latestId;
    }
}