using System;
using System.Collections;
using System.Collections.Generic;
using DarkNaku.Popup;
using UnityEngine;
using UnityEngine.UI;

public class CPopup : PopupHandler<CPopup>, IPopupParameter<CPopup, int>, IPopupResult<CPopup, int>
{
    [SerializeField] private Button _buttonClose;
    
    public int Param0 { get; private set; }
    public int Result0 { get; private set; }
    
    public CPopup SetParameter(int param0)
    {
        Param0 = param0;

        return this;
    }

    public CPopup SetResult(int result0)
    {
        Result0 = result0;
        
        return this;
    }
    
    protected override void OnWillShow()
    {
        Debug.Log("C 팝업이 열렸습니다.");
        Debug.LogFormat("매개변수 : {0}", Param0);
    }

    private void OnEnable()
    {
        _buttonClose.onClick.AddListener(OnButtonClicked);
    }

    private void OnDisable()
    {
        _buttonClose.onClick.RemoveListener(OnButtonClicked);
    }
    
    private void OnButtonClicked()
    {
        Popup.Hide<CPopup>(this).SetResult(333);
    }
}