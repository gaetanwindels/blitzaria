using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedInputManager : InputManager
{

    private int playerNumber;

    private Dictionary<string, float> simulatedAxis = new Dictionary<string, float>();
    private Dictionary<string, Buttonpress> simulatedButtons = new Dictionary<string, Buttonpress>();
    private bool registeredInputEvents = true;

    public SimulatedInputManager(int playerNumber)
    {
        init(playerNumber);
    }

    public void init(int playerNumber)
    {
        this.playerNumber = playerNumber;
    }

    public float GetAxis(string actionName)
    {
        return registeredInputEvents && simulatedAxis.ContainsKey(actionName) ? simulatedAxis[actionName] : 0f;
    }

    public bool GetButton(string actionName)
    {
        return registeredInputEvents && simulatedButtons.ContainsKey(actionName) && simulatedButtons[actionName] == Buttonpress.Pressing;
    }

    public bool GetButtonUp(string actionName)
    {
        return registeredInputEvents && simulatedButtons.ContainsKey(actionName) && simulatedButtons[actionName] == Buttonpress.Up;
    }

    public bool GetButtonDown(string actionName)
    {
        return registeredInputEvents && simulatedButtons.ContainsKey(actionName) && simulatedButtons[actionName] == Buttonpress.Down;
    }

    public void SimulateAxis(string actionName, float value)
    {
        simulatedAxis[actionName] = value;
    }

    public void SimulateButtonUp(string actionName)
    {
        simulatedButtons[actionName] = Buttonpress.Up;
    }

    public void SimulateButton(string actionName)
    {
        simulatedButtons[actionName] = Buttonpress.Pressing;
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
