using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingPositionsManager : MonoBehaviour
{

    [SerializeField] private PositionLocker[] team1Positions;
    [SerializeField] private PositionLocker[] team2Positions;
    [SerializeField] private PositionLocker ballPosition;

    private bool isEven = false;
    
    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void PositionBall()
    {
        if (!ballPosition)
        {
            return;
        }

        ballPosition.SetObjectToLock(FindObjectOfType<Ball>(true).gameObject);
    }

    public void PositionPlayers()
    {
        var players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        isEven = !isEven;

        if (isEven)
        {
            Array.Reverse(players);
        }
        
        foreach (Player player in players)
        {
            PositionPlayer(player);
        }
    }

    public void PositionPlayer(Player player)
    {
        Player playerToPosition = player.GetComponent<Player>();
        var positionLocker = GetFreePositionLocker(playerToPosition.team);

        if (positionLocker)
        {
            positionLocker.SetObjectToLock(player.gameObject);
        }
    }

    public PositionLocker GetFreePositionLocker(TeamEnum team)
    {
        PositionLocker[] positionLockers = {};
        
        if (team == TeamEnum.Team1)
        {
            positionLockers = team1Positions;
        }

        if (team == TeamEnum.Team2)
        {
            positionLockers = team2Positions;            
        }
        
        foreach (PositionLocker positionLocker in positionLockers)
        {
            if (!positionLocker.GetObjectTolock())
            {
                return positionLocker;
            }
        }

        return null;
    }
}
