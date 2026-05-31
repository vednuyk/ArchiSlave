using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(UIPanelPopup))]
public class BuildingAsk : MonoBehaviour
{
    [SerializeField] private TMP_Text _infoText;

    private UIPanelPopup _popup;
    private Action _onYes;

    void Awake() => _popup = GetComponent<UIPanelPopup>();

    // 확인창을 BuildingPanel과 같은 위치(동일 offset)에 팝업으로 띄운다.
    public void Show(string displayName, Vector3 gridWorldPos, Action onYes)
    {
        _infoText.text = $"Are you Building {displayName} ??";
        _onYes = onYes;
        _popup.Open(gridWorldPos);
    }

    // Yes 버튼 OnClick에서 호출 — 건설 콜백 실행 후 확인창만 닫힌다.
    public void OnYes()
    {
        var callback = _onYes;
        _onYes = null;
        _popup.Close();
        callback?.Invoke();
    }

    // No 버튼 OnClick에서 호출.
    public void OnNo() => Close();

    // 외부에서 강제로 확인창을 닫는다 (BuildingPanel과 생명주기 동기화).
    public void Close()
    {
        _onYes = null;
        _popup.Close();
    }
}
