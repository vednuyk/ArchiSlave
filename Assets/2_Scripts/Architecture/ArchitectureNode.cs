using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArchitectureNode : MonoBehaviour
{
    public List<ArchitectureNode> ConnectedNodes = new List<ArchitectureNode>();

    public Vector3 BaseScale { get; private set; }

    private Animator _nodeAnimator;
    private Coroutine _scaleCoroutine;

    void Awake()
    {
        BaseScale = transform.localScale;
        _nodeAnimator = GetComponent<Animator>();
    }

    public void AnimateScale(Vector3 target, float duration, AnimationCurve curve = null)
    {
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(ScaleRoutine(transform.localScale, target, duration, curve));
    }

    public void SetSortingOrder(int order)
    {
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.sortingOrder = order;
    }

    private IEnumerator ScaleRoutine(Vector3 from, Vector3 to, float dur, AnimationCurve curve)
    {
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dur);
            float v = curve != null ? curve.Evaluate(t) : Mathf.SmoothStep(0f, 1f, t);
            transform.localScale = Vector3.LerpUnclamped(from, to, v);
            yield return null;
        }
        transform.localScale = to;
        _scaleCoroutine = null;
    }
}
