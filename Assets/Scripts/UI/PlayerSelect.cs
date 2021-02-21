using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Rewired;

public class PlayerSelect : MonoBehaviour
{
    private RawImage image;
    private Texture textureImage;

    private Rewired.Player rwPlayer;

    [SerializeField] GameObject backgroundText;
    [SerializeField] int playerNumber = 0;

    [SerializeField] GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        rwPlayer = ReInput.players.GetPlayer(playerNumber);
    }

    // Update is called once per frame
    void Update()
    {
        if (rwPlayer.GetButton("A"))
        {
            Debug.Log("Pressed A");
            ManageNewPlayer();
        }
    }

    private void ManageNewPlayer()
    {
        backgroundText.SetActive(false);
    }
}
