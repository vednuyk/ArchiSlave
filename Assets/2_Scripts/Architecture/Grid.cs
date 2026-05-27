using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class Grid : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public static event Action<Grid> OnGridRightClicked;

    [SerializeField] private AnimationCurve _bounceCurve;
    [SerializeField] private float _hoverScaleMultiplier = 1.35f;
    [SerializeField] private float _scaleDuration = 0.42f;
    [SerializeField] private float _alphaDuration  = 0.15f;
    [SerializeField] private float _baseAlpha = 0.2f;

    public int Row { get; private set; }
    public int Col { get; private set; }

    private Vector3 _baseScale;
    private Coroutine _scaleCoroutine;
    private Coroutine _alphaCoroutine;
    private SpriteRenderer _sr;
    private int _baseSortingOrder;
    private bool _isActivated;

    private static Grid _activeGrid;

    public void Initialize(int row, int col, Sprite sprite, Material sharedMat)
    {
        Row = row;
        Col = col;

        _sr = GetComponent<SpriteRenderer>();
        _sr.sprite = sprite;
        if (sharedMat != null) _sr.sharedMaterial = sharedMat;
        _baseSortingOrder = _sr.sortingOrder;
        _sr.color = new Color(1f, 1f, 1f, _baseAlpha);

        if (sprite != null)
        {
            float ppu = sprite.pixelsPerUnit;
            var col2D = GetComponent<BoxCollider2D>();
            col2D.size = new Vector2(sprite.rect.width / ppu, sprite.rect.height / ppu);
        }

        _baseScale = transform.localScale;

        if (_bounceCurve == null || _bounceCurve.length == 0)
            _bounceCurve = BuildDefaultCurve();
    }

    public void OnPointerEnter(PointerEventData _) => AnimateAlpha(1f);
    public void OnPointerExit(PointerEventData _)  => AnimateAlpha(_baseAlpha);

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            Activate();
        else if (eventData.button == PointerEventData.InputButton.Right && _isActivated)
            OnGridRightClicked?.Invoke(this);
    }

    public void Deactivate()
    {
        _isActivated = false;
        if (_activeGrid == this) _activeGrid = null;
        AnimateScale(_baseScale, false);
    }

    private void Activate()
    {
        if (_activeGrid != null && _activeGrid != this)
            _activeGrid.Deactivate();

        _activeGrid  = this;
        _isActivated = true;
        AnimateScale(_baseScale * _hoverScaleMultiplier, true);
    }

    private void AnimateAlpha(float targetAlpha)
    {
        if (_alphaCoroutine != null) StopCoroutine(_alphaCoroutine);
        _alphaCoroutine = StartCoroutine(AlphaAnim(_sr.color.a, targetAlpha));
    }

    private void AnimateScale(Vector3 targetScale, bool isEnter)
    {
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(ScaleAnim(transform.localScale, targetScale, isEnter));
    }

    private IEnumerator AlphaAnim(float fromAlpha, float toAlpha)
    {
        float elapsed = 0f;
        Color color = _sr.color;
        while (elapsed < _alphaDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(fromAlpha, toAlpha, Mathf.Clamp01(elapsed / _alphaDuration));
            _sr.color = color;
            yield return null;
        }
        color.a = toAlpha;
        _sr.color = color;
        _alphaCoroutine = null;
    }

    private IEnumerator ScaleAnim(Vector3 fromScale, Vector3 toScale, bool isEnter)
    {
        _sr.sortingOrder = _baseSortingOrder + (isEnter ? 2 : 1);

        float elapsed = 0f;
        while (elapsed < _scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _scaleDuration);
            transform.localScale = Vector3.LerpUnclamped(fromScale, toScale, _bounceCurve.Evaluate(t));
            yield return null;
        }

        transform.localScale = toScale;
        _sr.sortingOrder = _baseSortingOrder + (isEnter ? 3 : 0);
        _scaleCoroutine = null;
    }

    private static AnimationCurve BuildDefaultCurve()
    {
        var curve = new AnimationCurve();
        curve.AddKey(new Keyframe(0f,     0f,     0f,  2f));
        curve.AddKey(new Keyframe(0.20f, -0.06f,  0f,  0f));
        curve.AddKey(new Keyframe(0.52f,  1.25f,  2f, -2f));
        curve.AddKey(new Keyframe(0.78f,  0.96f, -1f,  1f));
        curve.AddKey(new Keyframe(1f,     1f,     1f,  0f));
        return curve;
    }
}
