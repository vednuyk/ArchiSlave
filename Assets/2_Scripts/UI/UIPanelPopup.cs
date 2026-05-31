using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIPanelPopup : MonoBehaviour
{
    [SerializeField] private float _panelRightOffsetPx = 450f;
    [SerializeField] private float _panelVerticalOffsetPx = 0f;
    [SerializeField] private float _popupDuration      = 0.25f;
    [SerializeField] private float _popdownDuration    = 0.18f;
    [SerializeField] private AnimationCurve _popupCurve;
    [SerializeField] private AnimationCurve _popdownCurve;

    public bool IsOpen { get; private set; }

    private CanvasGroup _group;
    private RectTransform _rect;
    private Coroutine _coroutine;

    private void EnsureInit()
    {
        if (_rect != null) return;
        _rect  = GetComponent<RectTransform>();
        _group = GetComponent<CanvasGroup>();
        if (_popupCurve   == null || _popupCurve.length   == 0) _popupCurve   = BuildPopupCurve();
        if (_popdownCurve == null || _popdownCurve.length == 0) _popdownCurve = BuildPopdownCurve();
    }

    void Awake() => EnsureInit();

    // ── 공개 API ────────────────────────────────────────────

    public void Open(Vector3 gridWorldPos)
    {
        if (gameObject.activeSelf) return;
        EnsureInit();

        SetScreenPosition(gridWorldPos);
        gameObject.SetActive(true);
        transform.localScale  = Vector3.one * 0.05f;
        _group.interactable   = false;
        _group.blocksRaycasts = false;

        Restart(OpenSequence());
    }

    public void Close()
    {
        if (!gameObject.activeSelf) return;
        IsOpen = false;
        _group.interactable   = false;
        _group.blocksRaycasts = false;
        Restart(CloseSequence(transform.localScale.x));
    }

    public void SwitchTo(Vector3 newWorldPos)
    {
        IsOpen = false;
        _group.interactable   = false;
        _group.blocksRaycasts = false;
        Restart(SwitchSequence(newWorldPos));
    }

    // ── 시퀀스 코루틴 ───────────────────────────────────────

    private IEnumerator OpenSequence()
    {
        yield return RunPopup();
        _group.interactable   = true;
        _group.blocksRaycasts = true;
        IsOpen = true;
        _coroutine = null;
    }

    private IEnumerator CloseSequence(float fromScale)
    {
        yield return RunPopdown(fromScale);
        gameObject.SetActive(false);
        _coroutine = null;
    }

    private IEnumerator SwitchSequence(Vector3 newWorldPos)
    {
        yield return RunPopdown(transform.localScale.x);
        SetScreenPosition(newWorldPos);
        transform.localScale = Vector3.one * 0.05f;
        yield return RunPopup();
        _group.interactable   = true;
        _group.blocksRaycasts = true;
        IsOpen = true;
        _coroutine = null;
    }

    // ── 내부 애니 프리미티브 ─────────────────────────────────

    private IEnumerator RunPopup()
    {
        float elapsed = 0f;
        while (elapsed < _popupDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _popupDuration);
            transform.localScale = Vector3.one * Mathf.LerpUnclamped(0.05f, 1f, _popupCurve.Evaluate(t));
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    private IEnumerator RunPopdown(float fromScale)
    {
        float elapsed = 0f;
        while (elapsed < _popdownDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _popdownDuration);
            transform.localScale = Vector3.one * Mathf.LerpUnclamped(fromScale, 0.05f, _popdownCurve.Evaluate(t));
            yield return null;
        }
        transform.localScale = Vector3.one * 0.05f;
    }

    // ── 헬퍼 ────────────────────────────────────────────────

    private void SetScreenPosition(Vector3 worldPos)
    {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        screenPos.x += _panelRightOffsetPx;
        screenPos.y += _panelVerticalOffsetPx; // 음수 = 아래로
        _rect.position = screenPos;
    }

    private void Restart(IEnumerator routine)
    {
        if (_coroutine != null) StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(routine);
    }

    // ── 기본 커브 ────────────────────────────────────────────

    private static AnimationCurve BuildPopupCurve()
    {
        var c = new AnimationCurve();
        c.AddKey(new Keyframe(0f,   0f,   0f,  3f));
        c.AddKey(new Keyframe(0.6f, 1.1f, 2f, -2f));
        c.AddKey(new Keyframe(1f,   1f,   0f,  0f));
        return c;
    }

    private static AnimationCurve BuildPopdownCurve()
    {
        var c = new AnimationCurve();
        c.AddKey(new Keyframe(0f,    0f,    0f,  0.5f));
        c.AddKey(new Keyframe(0.2f, -0.08f, 0f,  0f));
        c.AddKey(new Keyframe(1f,    1f,    3f,  0f));
        return c;
    }
}
