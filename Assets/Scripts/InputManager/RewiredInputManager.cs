using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class RewiredInputManager : InputManager
{

    private int playerNumber;
    private Rewired.Player rwPlayer;

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
        return rwPlayer.GetAxis(actionName);
    }

    public bool GetButton(string actionName)
    {
        return rwPlayer.GetButton(actionName);
    }

    public bool GetButtonUp(string actionName)
    {
        return rwPlayer.GetButtonUp(actionName);
    }

    public bool GetButtonDown(string actionName)
    {
        return rwPlayer.GetButtonDown(actionName);
    }
}
