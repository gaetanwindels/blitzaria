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
    [SerializeField] public GameObject playerName;
    [SerializeField] public TeamEnum team = TeamEnum.Team1;
    [SerializeField] private Image clothes;
    [SerializeField] public Color[] team1Palette;
    [SerializeField] public Color[] team2Palette;

    [SerializeField] GameObject player;

    private bool isReady = false;    
    private TextMeshProUGUI teamText;
    private TextMeshProUGUI playerNameText;
    private Image teamImageButton;
    private int colorIndex;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponentInChildren<RawImage>();
        teamText = teamButton.GetComponentInChildren<TextMeshProUGUI>();
        teamImageButton = teamButton.GetComponentInChildren<Image>();
        playerNameText = playerName.GetComponentInChildren<TextMeshProUGUI>();
        colorIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        teamButton.gameObject.SetActive(playerNumber != -1);

        if (teamButton.gameObject.activeSelf)
        {
            teamText.text = team == TeamEnum.Team1 ? "Red Team" : "Blue Team";
            teamImageButton.color = team == TeamEnum.Team1 ? new Color(1, 0, 0, 1) : new Color(0, 0, 1, 1);
        }

        if (playerName != null && playerNameText != null && playerNumber != -1)
        {
            playerNameText.text = "Player " + (playerNumber + 1);
            playerName.SetActive(true);
        } else if (playerName != null)
        {
            playerName.SetActive(false);
        }

        if (rwPlayer == null)
        {
            return;
        }

        if (rwPlayer.GetButtonDown("Start"))
        {
            isReady = true;
            //image.color = new Color(1, 1, 1, 0.5f);
        }

        if (rwPlayer.GetButtonDown("UICancel"))
        {
            if (isReady)
            {
                //image.color = new Color(1, 1, 1, 1);
                isReady = false;
            }
            else
            {
                playerNumber = -1;
                backgroundText.SetActive(true);
                rwPlayer = null;
            }
        }

        var canvasGroup = GetComponent<CanvasGroup>();
        if (isReady)
        {
            canvasGroup.alpha = .7f;
        } else
        {
            canvasGroup.alpha = 1f;
        }
    }

    public void SwitchTeam()
    {
        if (team == TeamEnum.Team1)
        {
            team = TeamEnum.Team2;
        } else
        {
            team = TeamEnum.Team1;
        }

        colorIndex = 0;
    }

    public void PickNextColor()
    {

        var palette = team1Palette;
        if (team == TeamEnum.Team2)
        {
            palette = team2Palette;
        }

        colorIndex = (colorIndex + 1) % palette.Length;

        clothes.color = palette[colorIndex];
    }

    public void WakeUp(int playerNumber)
    {
        this.playerNumber = playerNumber;
        backgroundText.SetActive(false);
        rwPlayer = ReInput.players.GetPlayer(playerNumber);

        var cursors = FindObjectsOfType<CursorController>(true);

        foreach (CursorController cursor in cursors) {
            if (cursor.playerNumber == this.playerNumber)
            {
                cursor.gameObject.SetActive(true);
            }
        }
    }
    public bool IsAwake()
    {
        return playerNumber != -1;
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
        var greenComponent = 0.25f * playerNumber;

        var palette = team1Palette;
        if (team == TeamEnum.Team2)
        {
            palette = team2Palette;
        }
        playerSelect.color = palette[colorIndex];
        playerSelect.team = team;
        playerSelect.playerNumber = this.playerNumber;

        return playerSelect;
    }
}
