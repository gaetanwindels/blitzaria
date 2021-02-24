using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingPositionsManager : MonoBehaviour
{

    private PositionLocker[] positionLockers;

    // Start is called before the first frame update
    void Start()
    {
        positionLockers = GetComponentsInChildren<PositionLocker>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PositionPlayers()
    {
        var players = FindObjectsOfType<Player>();

        foreach (Player player in players)
        {
            PositionPlayer(player);
        }
    }

    public void PositionPlayer(Player player)
    {
        Player playerToPosition = player.GetComponent<Player>();
        var positionLocker = GetFreePositionLocker(playerToPosition.team);

        if (positionLocker != null)
        {
            positionLocker.SetObjectToLock(player.gameObject);
        }
    }

    public PositionLocker GetFreePositionLocker(TeamEnum team)
    {
        positionLockers = GetComponentsInChildren<PositionLocker>();

        foreach (PositionLocker positionLocker in positionLockers)
        {
            if (positionLocker.team == team && positionLocker.GetObjectTolock() == null)
            {
                return positionLocker;
            }
        }

        return null;
    }
}
