using UnityEngine;

public class ConstructManager : MonoBehaviour
{
    [SerializeField] private BuildingDatabase _database;

    private Grid _targetGrid;

    void OnEnable()  => BuildButton.OnBuildRequested += Build;
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
        UIManager.Instance.CloseBuildingPanel();
    }
}
