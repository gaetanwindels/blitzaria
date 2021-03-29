using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Rewired.Integration.UnityUI;

public class TeamButtonEventHandler : MonoBehaviour, IPointerDownHandler
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData is PlayerPointerEventData)
        {
            PlayerPointerEventData playerEventData = (PlayerPointerEventData) eventData;
            var playerSelect = GetComponentInParent<PlayerSelect>();

            if (playerSelect != null && playerSelect.playerNumber == playerEventData.playerId)
            {
                Debug.Log("CLICK");
                playerSelect.SwitchTeam();
            }
        }
    }
}
