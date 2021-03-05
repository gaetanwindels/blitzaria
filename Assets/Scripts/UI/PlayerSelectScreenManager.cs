using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using TMPro;
using UnityEngine.UI;

public class PlayerSelectScreenManager : MonoBehaviour
{
    private IList<Rewired.Player> playersToPoll = new List<Rewired.Player>();
    private PlayerSelect[] playerSelects;
    
    [SerializeField] TextMeshProUGUI readyText;
    [SerializeField] float countDownDuration = 3f;

    private float currentCountDown = -1f;
    private CustomSceneManager sceneManager;
    private bool isStarting = false;
    private GameSessionConfiguration gameSession;

    // Start is called before the first frame update
    void Start()
    {
        playersToPoll = ReInput.players.GetPlayers();
        playerSelects = FindObjectsOfType<PlayerSelect>();
        currentCountDown = countDownDuration;
        sceneManager = FindObjectOfType<CustomSceneManager>();
        gameSession = FindObjectOfType<GameSessionConfiguration>();
    }

    // Update is called once per frame
    void Update()
    {
        Poll();
        Debug.Log("Match starting in " + Mathf.Ceil(currentCountDown).ToString());
        readyText.text = EveryOneIsReady()  ? "Match starting in " + Mathf.Ceil(currentCountDown).ToString() : "";

        if (EveryOneIsReady())
        {
            currentCountDown -= Time.deltaTime;
        } else
        {
            ResetCountDown();
        }

        if (currentCountDown < 0)
        {
            readyText.text = "Starting...";
            SetAllPlayers();
            isStarting = true;
            sceneManager.GoToPlay();
        }

    }

    private void SetAllPlayers()
    {
        foreach (PlayerSelect playerSelect in playerSelects)
        {
            if (playerSelect.playerNumber != -1)
            {
                gameSession.players.Add(playerSelect.GetPlayer());
            }
            
        }        
    }

    void ResetCountDown()
    {
        currentCountDown = countDownDuration;
    }

    private bool EveryOneIsReady()
    {
        var atLeastOnePlayer = false;
        foreach (PlayerSelect playerSelect in playerSelects)
        {
            if (playerSelect.playerNumber != -1 && !playerSelect.IsReady())
            {
                return false;
            } else if (playerSelect.IsReady())
            {
                atLeastOnePlayer = true;
            }
        }

        return atLeastOnePlayer;
    }

    private void Poll()
    {
        if (isStarting)
        {
            return;
        }

        foreach (Rewired.Player playerToPoll in playersToPoll)
        {
            if (playerToPoll.GetButtonDown("UISubmit") && !HasPlayerAlreadyJoined(playerToPoll.id))
            {
                PlayerSelect playerSelect = GetFirstPlayerSelectFree();

                if (playerSelect != null)
                {
                    playerSelect.WakeUp(playerToPoll.id);
                }
            }
        }
    }

    private bool HasPlayerAlreadyJoined(int playerNumber)
    {
        foreach (PlayerSelect playerSelect in playerSelects)
        {
            if (playerSelect.playerNumber == playerNumber)
            {
                return true;
            }
        }

        return false;
    }

    private PlayerSelect GetFirstPlayerSelectFree()
    {
        int countTeam1 = 0;
        int countTeam2 = 0;

        foreach (PlayerSelect playerSelect in playerSelects)
        {
            if (playerSelect.playerNumber != -1)
            {
                if (playerSelect.team == TeamEnum.Team1)
                {
                    countTeam1++;
                } else
                {
                    countTeam2++;
                }
            }
        }

        var teamToChoose = countTeam1 > countTeam2 ? TeamEnum.Team2 : TeamEnum.Team1;
        foreach (PlayerSelect playerSelect in playerSelects)
        {
            if (playerSelect.playerNumber == -1 && playerSelect.team == teamToChoose)
            {
                return playerSelect;
            }
        }

        return null;
    }
}