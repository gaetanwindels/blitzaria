using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class RewiredInputManager : InputManager
{

    private int playerNumber;
    private Rewired.Player rwPlayer;
    private bool registeredInputEvents = true;
    private bool forceRegisteredInputEvents = true;

    public RewiredInputManager(int playerNumber)
    {
        Init(playerNumber);
    }

    public void Init(int playerNumber)
    {
        this.playerNumber = playerNumber;
        rwPlayer = ReInput.players.GetPlayer(playerNumber);
    }

    public float GetAxis(string actionName)
    {
        return (forceRegisteredInputEvents || registeredInputEvents)  ? rwPlayer.GetAxis(actionName) : 0f;
    }

    public bool GetButton(string actionName)
    {
        return (forceRegisteredInputEvents || registeredInputEvents)  && rwPlayer.GetButton(actionName);
    }

    public bool GetButtonUp(string actionName)
    {
        return (forceRegisteredInputEvents || registeredInputEvents)  && rwPlayer.GetButtonUp(actionName);
    }

    public bool GetButtonDown(string actionName)
    {
        return (forceRegisteredInputEvents || registeredInputEvents) && rwPlayer.GetButtonDown(actionName);
    }

    public void UnregisterInputEvents()
    {
        registeredInputEvents = false;
    }

    public void RegisterInputEvents()
    {
        registeredInputEvents = true;
    }
    
    public void ForceRegisterInputEvents()
    {
        forceRegisteredInputEvents = true;
    }
    
    public void UnForceRegisterInputEvents()
    {
        forceRegisteredInputEvents = false;
    }
}
