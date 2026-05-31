using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ArchiSlave/BuildingDatabase")]
public class BuildingDatabase : ScriptableObject
{
    public List<BuildingEntry> entries = new List<BuildingEntry>();

    public GameObject GetPrefab(int id)
    {
        foreach (var entry in entries)
            if (entry.id == id) return entry.prefab;
        return null;
    }

    public string GetDisplayName(int id)
    {
        foreach (var entry in entries)
            if (entry.id == id) return entry.displayName;
        return "?";
    }
}

[Serializable]
public class BuildingEntry
{
    public int id;
    public string displayName;
    public GameObject prefab;
}
