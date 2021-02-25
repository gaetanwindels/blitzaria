using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Rewired;
using TMPro;

public class PlayerSelect : MonoBehaviour
{
    private RawImage image;
    private Texture textureImage;

    private Rewired.Player rwPlayer;


    [SerializeField] public Button teamButton;
    [SerializeField] public GameObject backgroundText;
    [SerializeField] public int playerNumber = -1;
    [SerializeField] public TeamEnum team = TeamEnum.Team1;

    [SerializeField] GameObject player;

    private bool isReady = false;    
    private TextMeshProUGUI teamText;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponentInChildren<RawImage>();
        teamText = teamButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        teamButton.gameObject.SetActive(playerNumber != -1);

        if (teamButton.gameObject.activeSelf)
        {
            teamText.text = team == TeamEnum.Team1 ? "Red Team" : "Blue Team";
            teamText.color = team == TeamEnum.Team1 ? new Color(1, 0, 0, 1) : new Color(0, 0, 1, 1);
        }

        if (rwPlayer == null)
        {
            return;
        }

        if (rwPlayer.GetButtonDown("Start"))
        {
            isReady = true;
            image.color = new Color(1, 1, 1, 0.5f);
        }

        if (rwPlayer.GetButtonDown("UICancel"))
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

    public void ToggleTeam()
    {
        Debug.Log("BUTTON" + rwPlayer.GetButtonDown("UISubmit"));
        Debug.Log("toggling");
        team = team == TeamEnum.Team1 ? TeamEnum.Team2 : TeamEnum.Team1;
    }

    public PlayerSelectConfiguration GetPlayer()
    {
        var playerSelect = new PlayerSelectConfiguration();
        playerSelect.player = this.player;
        playerSelect.color = team == TeamEnum.Team1 ? new Color(1, 0, 0, 1) : new Color(0, 0, 1, 1);
        playerSelect.team = team;
        playerSelect.playerNumber = this.playerNumber;

        return playerSelect;
    }
}
