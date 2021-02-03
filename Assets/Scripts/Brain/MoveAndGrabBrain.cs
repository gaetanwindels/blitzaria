using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAndGrabBrain : Brain
{

    private Player player;
    private SimulatedInputManager inputManager = null;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Player>();
        //inputManager = new SimulatedInputManager(player.playerNumber);
        inputManager = (SimulatedInputManager) player.inputManager;
    }

    // Update is called once per frame
    void Update()
    {
        inputManager = (SimulatedInputManager) player.inputManager;

        // var ball = FindObjectOfType<Ball>();

        if (inputManager == null)
        {
            return;
        }

        inputManager.SimulateButton("Grab");
        // inputManager.SimulateAxis("Move Horizontal", 1);

    }
}
