using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class RewiredInputManager : InputManager
{

    private int playerNumber;
    private Rewired.Player rwPlayer;
    private bool registeredInputEvents = true;

    public RewiredInputManager(int playerNumber)
    {
        init(playerNumber);
    }

    public void init(int playerNumber)
    {
        this.playerNumber = playerNumber;
        rwPlayer = ReInput.players.GetPlayer(playerNumber);
    }

    public float GetAxis(string actionName)
    {
        Debug.Log("HOOOO" + registeredInputEvents);
        return registeredInputEvents ? rwPlayer.GetAxis(actionName) : 0f;
    }

    public bool GetButton(string actionName)
    {
        return registeredInputEvents && rwPlayer.GetButton(actionName);
    }

    public bool GetButtonUp(string actionName)
    {
        return registeredInputEvents && rwPlayer.GetButtonUp(actionName);
    }

    public bool GetButtonDown(string actionName)
    {
        return registeredInputEvents && rwPlayer.GetButtonDown(actionName);
    }

    public void UnregisterInputEvents()
    {
        registeredInputEvents = false;
    }

    public void RegisterInputEvents()
    {
        registeredInputEvents = true;
    }
}
