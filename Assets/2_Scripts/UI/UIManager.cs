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

    void OnEnable()  => Grid.OnGridRightClicked += OpenBuildingPanel;
    void OnDisable() => Grid.OnGridRightClicked -= OpenBuildingPanel;

    void Update()
    {
        if (!_panel.IsOpen) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        if (!RectTransformUtility.RectangleContainsScreenPoint(_panelRect, Mouse.current.position.ReadValue()))
            CloseBuildingPanel();
    }

    public void OpenBuildingPanel(Grid targetGrid)
    {
        if (targetGrid == _currentGrid && _panel.gameObject.activeSelf) return;

        _constructManager.SetTargetGrid(targetGrid);
        _currentGrid = targetGrid;

        if (!_panel.gameObject.activeSelf)
            _panel.Open(targetGrid.transform.position);
        else
            _panel.SwitchTo(targetGrid.transform.position);
    }

    public void CloseBuildingPanel()
    {
        _currentGrid?.Deactivate();
        _currentGrid = null;
        _panel.Close();
    }
}
