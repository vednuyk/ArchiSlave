using UnityEngine;

public class ConstructManager : MonoBehaviour
{

    private UIManager _uiManager;
    [SerializeField] private BuildingDatabase _database;

    void Awake()
    {
        _uiManager = GetComponentInParent<UIManager>();
    }
    private Grid _targetGrid;

    void OnEnable() => BuildButton.OnBuildRequested += Build;
    void OnDisable() => BuildButton.OnBuildRequested -= Build;

    public void SetTargetGrid(Grid grid) => _targetGrid = grid;

    private void Build(int id)
    {
        if (_targetGrid == null) return;
        if (ArchitectureManager.Instance.HasBuilding(_targetGrid)) return;

        var prefab = _database.GetPrefab(id);
        if (prefab == null)
        {
            Debug.LogWarning($"BuildingDatabase: id {id} not found.");
            return;
        }

        var go = Instantiate(
            prefab,
            _targetGrid.transform.position,
            Quaternion.identity,
            ArchitectureManager.Instance.transform
        );

        var node = go.GetComponent<ArchitectureNode>();
        if (node != null)
            ArchitectureManager.Instance.Register(_targetGrid, node);

        _uiManager.CloseBuildingPanel();
    }
}
