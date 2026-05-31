using UnityEngine;

public class ConstructManager : MonoBehaviour
{
    [SerializeField] private BuildingDatabase _database;
    [SerializeField] private BuildingAsk _askPrefab;
    [SerializeField] private Transform _canvasRoot;

    private Grid _targetGrid;
    private BuildingAsk _askInstance;
    private UIManager _uiManager;

    void Awake() => _uiManager = GetComponentInParent<UIManager>();

    void OnEnable() => BuildButton.OnBuildRequested += OnBuildSelected;
    void OnDisable() => BuildButton.OnBuildRequested -= OnBuildSelected;

    public void SetTargetGrid(Grid grid) => _targetGrid = grid;

    // BuildingPanel이 닫힐 때 확인창도 함께 닫는다.
    public void CloseAsk()
    {
        if (_askInstance != null && _askInstance.gameObject.activeSelf)
            _askInstance.Close();
    }

    // 건물 버튼 선택 → 즉시 짓지 않고 확인창(BuildingAsk)을 띄운다.
    private void OnBuildSelected(int id)
    {
        if (_targetGrid == null) return;
        if (ArchitectureManager.Instance.HasBuilding(_targetGrid)) return;
        if (_database.GetPrefab(id) == null)
        {
            Debug.LogWarning($"BuildingDatabase: id {id} not found.");
            return;
        }

        // 확인창이 떠 있는 동안 _targetGrid가 바뀌어도 원래 그리드에 짓도록 캡처.
        Grid grid = _targetGrid;
        EnsureAsk().Show(_database.GetDisplayName(id), grid.transform.position, () => Build(id, grid));
    }

    private BuildingAsk EnsureAsk()
    {
        if (_askInstance == null)
        {
            Transform parent = _canvasRoot != null ? _canvasRoot : transform.parent;
            _askInstance = Instantiate(_askPrefab, parent);
            _askInstance.gameObject.SetActive(false); // Show() 안의 Open()이 활성화하도록
        }
        _askInstance.transform.SetAsLastSibling(); // BuildingPanel 위에 겹쳐서
        return _askInstance;
    }

    // Yes 확인 후 실제 건설 — 패널은 즉시 닫고, 그리드/CPU bounce 연출은 그대로 진행한다.
    private void Build(int id, Grid grid)
    {
        if (grid == null) return;
        if (ArchitectureManager.Instance.HasBuilding(grid)) return;

        var prefab = _database.GetPrefab(id);
        if (prefab == null) return;

        // 패널 즉시 닫기 (그리드 비활성화는 SyncNewBuilding의 bounce 후에 일어남)
        _uiManager.CloseBuildingPanel();

        var go = Instantiate(
            prefab,
            grid.transform.position,
            Quaternion.identity,
            ArchitectureManager.Instance.transform
        );

        var node = go.GetComponent<ArchitectureNode>();
        if (node != null)
        {
            ArchitectureManager.Instance.Register(grid, node);
            grid.SyncNewBuilding(node); // 활성 그리드 비율로 바운싱 설치
        }
    }
}
