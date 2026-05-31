using System.Collections.Generic;
using UnityEngine;

public class ArchitectureManager : MonoBehaviour
{
    public static ArchitectureManager Instance { get; private set; }

    private readonly Dictionary<Grid, ArchitectureNode> _buildings = new();

    void Awake() => Instance = this;

    public void Register(Grid grid, ArchitectureNode node) => _buildings[grid] = node;

    public void Unregister(Grid grid) => _buildings.Remove(grid);

    public ArchitectureNode GetBuilding(Grid grid) =>
        _buildings.TryGetValue(grid, out var node) ? node : null;

    public bool HasBuilding(Grid grid) => _buildings.ContainsKey(grid);
}
