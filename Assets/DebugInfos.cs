using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugInfos : MonoBehaviour
{

    public string rootState;
    public string subState;

    private PlayerStateMachine _playerStateMachine;
    
    // Start is called before the first frame update
    void Start()
    {
        _playerStateMachine = FindObjectOfType<PlayerStateMachine>();
    }

    // Update is called once per frame
    void Update()
    {
        rootState = subState = "null";
        
        if (_playerStateMachine.CurrentState != null)
        {
            rootState = _playerStateMachine.CurrentState.StateName;

            if (_playerStateMachine.CurrentState.CurrentSubState != null)
            {
                subState = _playerStateMachine.CurrentState.CurrentSubState.StateName;
            }
        }
        
    }
}
