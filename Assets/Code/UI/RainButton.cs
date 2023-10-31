using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RainButton : MonoBehaviour
{
    public static RainButton Instance;
    
    private RainManager _rainManager;
    
    private Action<bool> _onSetRaining;
    private TextMeshProUGUI _textElement;
    private bool _raining;

    private void Awake()
    {
        Instance = this;
        _textElement = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        _rainManager = RainManager.Instance;
        _onSetRaining += _rainManager.SetRaining;
    }

    public void ToggleRain()
    {
        _raining = !_raining;
        _onSetRaining?.Invoke(_raining);
        _textElement.text = _raining ? "Stopp nedbør" : "Start nedbør";
    }

    public void LimitReached()
    {
        _raining = false;
        _textElement.text = "Start nedbør";
    }
}
