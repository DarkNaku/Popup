using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DarkNaku.Popup;

public class Main : MonoBehaviour
{
    [SerializeField] private Button _buttonShow;

    private void OnEnable()
    {
        _buttonShow.onClick.AddListener(OnButtonShowClicked);
    }

    private void OnDisable()
    {
        _buttonShow.onClick.RemoveListener(OnButtonShowClicked);
    }

    private void OnButtonShowClicked()
    {
        Popup.Show<APopup>("APopup")
            .SetParameter(123)
            .OnWillHide((result) => Debug.LogFormat("A 팝업이 닫혔습니다. 결과 : {0}", result.Result0))
            .Hello();
    }
}
