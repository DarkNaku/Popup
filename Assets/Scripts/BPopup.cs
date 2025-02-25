using System;
using System.Collections;
using System.Collections.Generic;
using DarkNaku.Popup;
using UnityEngine;
using UnityEngine.UI;

public class BPopup : PopupHandler<BPopup>, IPopupParameter<BPopup, int>, IPopupResult<BPopup, int>
{
    [SerializeField] private Button _buttonClose;
    [SerializeField] private Button _buttonShow;
    
    public int Param0 { get; private set; }
    public int Result0 { get; private set; }
    
    public BPopup SetParameter(int param0)
    {
        Param0 = param0;

        return this;
    }

    public BPopup SetResult(int result0)
    {
        Result0 = result0;

        return this;
    }
    
    protected override void OnWillShow()
    {
        Debug.Log("B 팝업이 열렸습니다.");
        Debug.LogFormat("매개변수 : {0}", Param0);
    }

    private void OnEnable()
    {
        _buttonClose.onClick.AddListener(OnButtonClicked);
        _buttonShow.onClick.AddListener(OnButtonShowClicked);
    }

    private void OnDisable()
    {
        _buttonClose.onClick.RemoveListener(OnButtonClicked);
        _buttonShow.onClick.RemoveListener(OnButtonShowClicked);
    }
    
    private void OnButtonClicked()
    {
        Popup.Hide<BPopup>(this).SetResult(222);
    }
    
    private void OnButtonShowClicked()
    {
        Popup.Show<CPopup>("CPopup")
            .SetParameter(789)
            .OnWillHide((p) => Debug.LogFormat("C 팝업이 닫혔습니다. 결과 : {0}", p.Result0));
    }
}