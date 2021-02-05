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

        var ball = FindObjectOfType<Ball>();

        if (inputManager == null)
        {
            return;
        }

        inputManager.SimulateButton("Grab");

        //ball.transform.position
        var playPos = player.transform.position;
        var vect = Vector2.MoveTowards(playPos, ball.transform.position, Time.deltaTime);
        // inputManager.SimulateAxis("Move Horizontal", 1);
        //inputManager.SimulateAxis("Move Horizontal", 1);
        //inputManager.SimulateAxis("Move Vertital", 1);

    }
}
