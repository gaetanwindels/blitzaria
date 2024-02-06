using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects.Events;
using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    // Config parameters
    [SerializeField] private EventChannel _eventChannel;
    
    // Cached variables
    private TextMeshProUGUI _timerText;
    
    // State Variable
    private bool _isStarted;
    private float _currentTime;
    private bool _isOvertime;

    private void Awake()
    {
        _timerText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isStarted || _currentTime <= 0)
        {
            return;
        }

        _currentTime -= Time.deltaTime;
        _currentTime = Mathf.Max(0, _currentTime);

        if (_currentTime <= 0)
        {
            _eventChannel.RaiseTimeoutEvent();
        }
        
        UpdateTimer();
    }
    
    private void UpdateTimer()
    {
        if (_timerText == null || _isOvertime)
        {
            return;
        }
        
        var minutes = Mathf.Floor(_currentTime / 60);
        var seconds = Mathf.Max(0, Mathf.Round(_currentTime % 60));
        var secondsText = seconds.ToString();
        if (seconds < 10)
        {
            secondsText = "0" + seconds;
        }

        if (seconds == 60)
        {
            secondsText = "00";
            minutes++;
        }

        _timerText.text = minutes + ":" + secondsText;
    }
    
    public void InitOvertime()
    {
        _isOvertime = true;
        _timerText.text = "OT";
        UpdateTimer();
    }

    public void Init(float seconds)
    {
        _currentTime = seconds;
        UpdateTimer();
    }

    public void StartTimer()
    {
        _isStarted = true;
    }

    public void StopTimer()
    {
        _isStarted = false;
    }

}
