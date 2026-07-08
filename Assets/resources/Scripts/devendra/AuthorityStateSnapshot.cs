using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public sealed class AuthorityStateSnapshot
{
    public string snapshot_id;
    public string created_utc;
    public string hash;

    // per-asset hashes (for diffs)
    public Dictionary<string, string> unitHashes = new Dictionary<string, string>(64);

    public static AuthorityStateSnapshot Capture(string snapshotId, List<UnitData> unitsToWatch)
    {
        var snap = new AuthorityStateSnapshot
        {
            snapshot_id = snapshotId,
            created_utc = DateTime.UtcNow.ToString("o")
        };

        if (unitsToWatch == null) unitsToWatch = new List<UnitData>();

        // Stable ordering by deterministic key
        var keys = new List<string>(unitsToWatch.Count);
        for (int i = 0; i < unitsToWatch.Count; i++)
        {
            var u = unitsToWatch[i];
            if (u == null) continue;
            keys.Add(UnitKey(u));
        }
        keys.Sort(StringComparer.Ordinal);

        // Map key -> unit reference
        // (O(n^2) but n small; avoids allocations from LINQ)
        for (int k = 0; k < keys.Count; k++)
        {
            UnitData u = null;
            for (int i = 0; i < unitsToWatch.Count; i++)
            {
                var cand = unitsToWatch[i];
                if (cand == null) continue;
                if (UnitKey(cand) == keys[k]) { u = cand; break; }
            }
            if (u == null) continue;

            snap.unitHashes[keys[k]] = HashUnitData(u);
        }

        snap.hash = HashSnapshot(snap.unitHashes);
        return snap;
    }

    public static string UnitKey(UnitData u)
    {
        // Deterministic identity string (avoid instance IDs)
        string name = Safe(u.unitName);
        string prefabName = (u.prefab != null) ? Safe(u.prefab.name) : "null_prefab";
        return $"{name}|cost={Quant(u.cost)}|prefab={prefabName}";
    }

    private static string HashUnitData(UnitData u)
    {
        // Hash public+private instance fields that are stable (serialized-like), but encode UnityEngine.Object as name only.
        // This is a detector: if any field changes at runtime, hash changes.
        var t = u.GetType();
        var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // deterministic field order
        Array.Sort(fields, (a, b) => string.CompareOrdinal(a.Name, b.Name));

        var sb = new StringBuilder(512);
        sb.Append("UnitData|").Append(UnitKey(u)).Append("|");

        for (int i = 0; i < fields.Length; i++)
        {
            var f = fields[i];

            // Skip obvious Unity internals/noise
            if (f.IsStatic) continue;
            if (f.Name == "m_Script") continue;

            object val = f.GetValue(u);
            sb.Append(f.Name).Append("=");

            if (val == null)
            {
                sb.Append("null");
            }
            else
            {
                var ft = f.FieldType;

                if (ft == typeof(int))
                    sb.Append((int)val);
                else if (ft == typeof(float))
                    sb.Append(Quant((float)val));
                else if (ft == typeof(bool))
                    sb.Append(((bool)val) ? "true" : "false");
                else if (ft == typeof(string))
                    sb.Append(Safe((string)val));
                else if (typeof(UnityEngine.Object).IsAssignableFrom(ft))
                    sb.Append(Safe(((UnityEngine.Object)val).name));
                else if (ft.IsEnum)
                    sb.Append(val.ToString());
                else
                {
                    // fallback: stable string
                    sb.Append(Safe(val.ToString()));
                }
            }

            sb.Append("|");
        }

        return Sha256(sb.ToString());
    }

    private static string HashSnapshot(Dictionary<string, string> unitHashes)
    {
        var keys = new List<string>(unitHashes.Keys);
        keys.Sort(StringComparer.Ordinal);

        var sb = new StringBuilder(1024);
        for (int i = 0; i < keys.Count; i++)
        {
            string k = keys[i];
            sb.Append(k).Append("=>").Append(unitHashes[k]).Append("\n");
        }
        return Sha256(sb.ToString());
    }

    private static string Sha256(string s)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(s);
        var hash = sha.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private static string Safe(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\n", " ").Replace("\r", " ").Trim();
    }

    private static string Quant(float v)
    {
        // deterministic float formatting
        v = Mathf.Round(v * 1000f) / 1000f;
        return v.ToString("F3", CultureInfo.InvariantCulture);
    }
}