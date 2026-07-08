using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ReplayPersistence
{
    public static string ReplaysRoot => Path.Combine(Application.persistentDataPath, "replays");

    public static string GetSessionDir(string sessionId) => Path.Combine(ReplaysRoot, sessionId);
    public static string GetHeaderPath(string sessionId) => Path.Combine(GetSessionDir(sessionId), "header.json");
    public static string GetEventsPath(string sessionId) => Path.Combine(GetSessionDir(sessionId), "events.jsonl");

    public static void EnsureRoot()
    {
        if (!Directory.Exists(ReplaysRoot))
            Directory.CreateDirectory(ReplaysRoot);
    }

    public static void SaveHeader(ReplaySessionHeader header)
    {
        EnsureRoot();
        string dir = GetSessionDir(header.session_id);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string json = JsonUtility.ToJson(header, true);
        File.WriteAllText(GetHeaderPath(header.session_id), json);
    }

    /// Append-only: writes one JSON object per line
    public static void AppendEvent(string sessionId, ReplayEventRecord ev)
    {
        EnsureRoot();
        string dir = GetSessionDir(sessionId);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string jsonLine = JsonUtility.ToJson(ev, false);
        File.AppendAllText(GetEventsPath(sessionId), jsonLine + "\n");
    }

    public static ReplaySessionHeader LoadHeader(string sessionId)
    {
        string path = GetHeaderPath(sessionId);
        if (!File.Exists(path))
            throw new FileNotFoundException("Replay header not found: " + path);

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<ReplaySessionHeader>(json);
    }

    public static List<ReplayEventRecord> LoadEvents(string sessionId)
    {
        string path = GetEventsPath(sessionId);
        if (!File.Exists(path))
            throw new FileNotFoundException("Replay events not found: " + path);

        var list = new List<ReplayEventRecord>(256);
        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            list.Add(JsonUtility.FromJson<ReplayEventRecord>(line));
        }
        return list;
    }
}