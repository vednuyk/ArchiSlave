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

        var prefab = _database.GetPrefab(id);
        if (prefab == null)
        {
            Debug.LogWarning($"BuildingDatabase: id {id} not found.");
            return;
        }

        Instantiate(prefab, _targetGrid.transform.position, Quaternion.identity);
        _uiManager.CloseBuildingPanel();
    }
}
