using System;
using UnityEngine;

public class BuildButton : MonoBehaviour
{
    public static event Action<int> OnBuildRequested;

    [SerializeField] private int _buildingId;

    public void OnClick() => OnBuildRequested?.Invoke(_buildingId);
}
