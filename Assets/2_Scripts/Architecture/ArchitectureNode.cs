using System.Collections.Generic;
using UnityEngine;

public class ArchitectureNode : MonoBehaviour
{
    public List<ArchitectureNode> ConnectedNodes = new List<ArchitectureNode>();
    private Animator _nodeAnimator;

    void Awake()
    {
        _nodeAnimator = GetComponent<Animator>();
    }

    protected virtual void OnMouseEnter()
    {

    }
    protected virtual void OnMouseOver()
    {

    }
    protected virtual void OnMouseDown()
    {

    }
    protected virtual void OnMouseDrag()
    {

    }
    protected virtual void OnMouseExit()
    {

    }
}
