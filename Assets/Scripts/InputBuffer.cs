using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class InputBuffer : MonoBehaviour
{
    [SerializeField]
    private float bufferDuration = 0.2f;

    private Dictionary<string, float> _downBuffer = new();
    private Dictionary<string, float> _upBuffer = new();
    private Dictionary<string, float> _buttonBuffer = new();
    
    // Cached variables
    private InputManager _inputManager;

    void Start()
    {
    }

    void Update()
    {
        BufferInputs();
        UpdateBuffer(_upBuffer);
        UpdateBuffer(_downBuffer);
        UpdateBuffer(_buttonBuffer);
    }

    private void BufferInputs()
    {
        if (_inputManager == null)
        {
            _inputManager = GetComponent<Player>().inputManager;
        }

        if (_inputManager == null)
        {
            return;
        }
        
        if (_inputManager.GetButtonDown("tackle"))
        {
            BufferButtonDown("tackle");
        }
        
        if (_inputManager.GetButton("tackle"))
        {
            BufferButton("tackle");
        }
        
        if (_inputManager.GetButtonUp("tackle"))
        {
            BufferButtonUp("tackle");
        }
    }

    private void UpdateBuffer(Dictionary<string, float> buffer)
    {
        List<string> keysToRemove = new();
        List<string> keys = new List<string>(buffer.Keys);
        foreach(var item in keys)
        {
            buffer[item] += Time.deltaTime;
            
            if (buffer[item] > bufferDuration)
            {
                keysToRemove.Add(item);
            }
        }
        
        foreach (var key in keysToRemove)
        {
            buffer.Remove(key);
        }
    }

    public bool GetButton(string actionName)
    {
        return _buttonBuffer.ContainsKey(actionName);
    }
    
    public bool GetButtonUp(string actionName)
    {
        return _upBuffer.ContainsKey(actionName);
    }
    
    public bool GetButtonDown(string actionName)
    {
        return _downBuffer.ContainsKey(actionName);
    }

    public void BufferButtonUp(string actionName)
    {
        _upBuffer[actionName] = 0f;
    }
    
    public void BufferButtonDown(string actionName)
    {
        _downBuffer[actionName] = 0f;  
    }
    
    public void BufferButton(string actionName)
    {
        _buttonBuffer[actionName] = 0f;
    }
}