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
    [SerializeField] private float _animDuration = 0.42f;
    [SerializeField] private float _baseAlpha = 0.2f;

    public int Row { get; private set; }
    public int Col { get; private set; }

    private Vector3 _baseScale;
    private Coroutine _animCoroutine;
    private SpriteRenderer _sr;
    private int _baseSortingOrder;
    private bool _isFullyHovered;

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

    public void OnPointerEnter(PointerEventData _) => Animate(_baseScale * _hoverScaleMultiplier, 1f, true);
    public void OnPointerExit(PointerEventData _)
    {
        _isFullyHovered = false;
        Animate(_baseScale, _baseAlpha, false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && _isFullyHovered)
            OnGridRightClicked?.Invoke(this);
    }

    private void Animate(Vector3 targetScale, float targetAlpha, bool isEnter)
    {
        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = StartCoroutine(AnimateScaleAndAlpha(
            transform.localScale, targetScale,
            _sr.color.a, targetAlpha, isEnter));
    }

    private IEnumerator AnimateScaleAndAlpha(
        Vector3 fromScale, Vector3 toScale,
        float fromAlpha, float toAlpha, bool isEnter)
    {
        // Enter 진행 중 → +2 / Exit 진행 중 → +1
        _sr.sortingOrder = _baseSortingOrder + (isEnter ? 2 : 1);

        float elapsed = 0f;
        Color color = _sr.color;
        while (elapsed < _animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _animDuration);

            transform.localScale = Vector3.LerpUnclamped(fromScale, toScale, _bounceCurve.Evaluate(t));

            // Enter: 빠르게 상승 / Exit: 바운싱 타임라인과 함께 서서히 감소
            float alphaT = isEnter ? Mathf.Clamp01(t * 3.5f) : Mathf.SmoothStep(0f, 1f, t);
            color.a = Mathf.Lerp(fromAlpha, toAlpha, alphaT);
            _sr.color = color;
            yield return null;
        }

        // 최종 정착
        transform.localScale = toScale;
        color.a = toAlpha;
        _sr.color = color;
        // Enter 완료 → +3 / Exit 완료 → 0
        _sr.sortingOrder = _baseSortingOrder + (isEnter ? 3 : 0);
        _isFullyHovered = isEnter;
        _animCoroutine = null;
    }

    // 부드러운 수축 → 오버슈트 → 정착
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
