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

    [SerializeField] public GameObject backgroundText;
    [SerializeField] public int playerNumber = -1;
    [SerializeField] public TeamEnum team = TeamEnum.Team1;

    [SerializeField] GameObject player;

    private bool isReady = false;    

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponentInChildren<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        if (rwPlayer == null)
        {
            return;
        }

        if (rwPlayer.GetButtonDown("Start"))
        {
            isReady = true;
            image.color = new Color(1, 1, 1, 0.5f);
        }

        if (rwPlayer.GetButtonDown("B"))
        {
            if (isReady)
            {
                image.color = new Color(1, 1, 1, 1);
                isReady = false;
            }
            else
            {
                playerNumber = -1;
                backgroundText.SetActive(true);
                rwPlayer = null;
            }
        }
    }

    public void WakeUp(int playerNumber)
    {
        this.playerNumber = playerNumber;
        backgroundText.SetActive(false);
        rwPlayer = ReInput.players.GetPlayer(playerNumber);
    }

    public bool IsReady()
    {
        return playerNumber != -1 && isReady;
    }

    public GameObject GetPlayer()
    {
        Player player = this.player.GetComponent<Player>();
        player.team = team;
        player.playerNumber = this.playerNumber;

        return this.player;
    }
}
