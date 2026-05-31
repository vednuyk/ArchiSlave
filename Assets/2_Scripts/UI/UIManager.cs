using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    private UIPanelPopup _panel;
    private RectTransform _panelRect;
    private ConstructManager _constructManager;
    private Grid _currentGrid;

    void Awake()
    {
        _panel            = GetComponentInChildren<UIPanelPopup>(includeInactive: true);
        _panelRect        = _panel.GetComponent<RectTransform>();
        _constructManager = GetComponentInChildren<ConstructManager>(includeInactive: true);
    }

    void OnEnable()
    {
        Grid.OnGridActivated   += OnAnyGridActivated;
        Grid.OnGridDeactivated += OnAnyGridDeactivated;
    }

    void OnDisable()
    {
        Grid.OnGridActivated   -= OnAnyGridActivated;
        Grid.OnGridDeactivated -= OnAnyGridDeactivated;
    }

    void Update()
    {
        // B키: BuildingPanel 토글
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            if (_panel.gameObject.activeSelf)
                CloseBuildingPanel();
            else if (Grid.ActiveGrid != null)
                OpenBuildingPanel(Grid.ActiveGrid);
            return;
        }

        // 우클릭: 뒤로가기 — 활성화된 그리드 비활성화 (패널은 OnGridDeactivated에서 자동 처리)
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Grid.ActiveGrid?.Deactivate();
            return;
        }

        // 패널 밖 좌클릭: 패널 닫기
        if (!_panel.IsOpen) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (IsMouseOverGrid()) return;
        if (!RectTransformUtility.RectangleContainsScreenPoint(_panelRect, Mouse.current.position.ReadValue()))
            CloseBuildingPanel();
    }

    private void OnAnyGridActivated(Grid targetGrid)
    {
        if (!_panel.gameObject.activeSelf) return;
        _constructManager.SetTargetGrid(targetGrid);
        _currentGrid = targetGrid;
        _panel.SwitchTo(targetGrid.transform.position);
    }

    private void OnAnyGridDeactivated(Grid grid)
    {
        if (_currentGrid != grid) return;
        _currentGrid = null;
        _constructManager.CloseAsk();
        _panel.Close();
    }

    public void OpenBuildingPanel(Grid targetGrid)
    {
        _constructManager.SetTargetGrid(targetGrid);
        _currentGrid = targetGrid;

        if (!_panel.gameObject.activeSelf)
            _panel.Open(targetGrid.transform.position);
        else
            _panel.SwitchTo(targetGrid.transform.position);
    }

    public void CloseBuildingPanel()
    {
        // 패널만 닫는다. 그리드 활성화 해제는 우클릭(뒤로가기) 전담.
        _currentGrid = null;
        _constructManager.CloseAsk();
        _panel.Close();
    }

    private bool IsMouseOverGrid()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        worldPos.z = 0f;
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        return hit != null && hit.GetComponent<Grid>() != null;
    }
}
