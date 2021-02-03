using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedInputManager : InputManager
{

    private int playerNumber;

    private Dictionary<string, float> simulatedAxis = new Dictionary<string, float>();
    private Dictionary<string, Buttonpress> simulatedButtons = new Dictionary<string, Buttonpress>();

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
        return simulatedAxis.ContainsKey(actionName) ? simulatedAxis[actionName] : 0f;
    }

    public bool GetButton(string actionName)
    {
        if (actionName == "Grab")
        {

            Debug.Log("IT IS CALLED " + actionName);
        }
        if (simulatedButtons.ContainsKey(actionName))
        {
            Debug.Log(simulatedButtons[actionName]);
        }
        return simulatedButtons.ContainsKey(actionName) && simulatedButtons[actionName] == Buttonpress.Pressing;
    }

    public bool GetButtonUp(string actionName)
    {
        return simulatedButtons.ContainsKey(actionName) && simulatedButtons[actionName] == Buttonpress.Up;
    }

    public bool GetButtonDown(string actionName)
    {
        return simulatedButtons.ContainsKey(actionName) && simulatedButtons[actionName] == Buttonpress.Down;
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

}
