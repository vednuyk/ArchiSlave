using System.Collections;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject _buildingPanel;
    [SerializeField] private ConstructManager _constructManager;

    private CanvasGroup _panelGroup;
    private Coroutine _popupCoroutine;

    private static readonly AnimationCurve _popupCurve = BuildPopupCurve();

    void Awake()
    {
        Instance = this;
        _panelGroup = _buildingPanel.GetComponent<CanvasGroup>();
    }

    void Start()
    {
        _buildingPanel.SetActive(false);
    }

    void OnEnable()  => Grid.OnGridRightClicked += OpenBuildingPanel;
    void OnDisable() => Grid.OnGridRightClicked -= OpenBuildingPanel;

    public void OpenBuildingPanel(Grid targetGrid)
    {
        if (_buildingPanel.activeSelf) return;

        _constructManager.SetTargetGrid(targetGrid);
        _buildingPanel.SetActive(true);
        _buildingPanel.transform.localScale = Vector3.one * 0.05f;
        _panelGroup.interactable = false;
        _panelGroup.blocksRaycasts = false;

        if (_popupCoroutine != null) StopCoroutine(_popupCoroutine);
        _popupCoroutine = StartCoroutine(PopupAnim());
    }

    public void CloseBuildingPanel()
    {
        if (_popupCoroutine != null)
        {
            StopCoroutine(_popupCoroutine);
            _popupCoroutine = null;
        }
        _buildingPanel.SetActive(false);
    }

    private IEnumerator PopupAnim()
    {
        const float duration = 0.25f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float scale = Mathf.LerpUnclamped(0.05f, 1f, _popupCurve.Evaluate(t));
            _buildingPanel.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        _buildingPanel.transform.localScale = Vector3.one;
        _panelGroup.interactable = true;
        _panelGroup.blocksRaycasts = true;
        _popupCoroutine = null;
    }

    private static AnimationCurve BuildPopupCurve()
    {
        var curve = new AnimationCurve();
        curve.AddKey(new Keyframe(0f,    0f,   0f,  3f));
        curve.AddKey(new Keyframe(0.6f,  1.1f, 2f, -2f));
        curve.AddKey(new Keyframe(1f,    1f,   0f,  0f));
        return curve;
    }
}
