using System;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    // Config parameters
    [SerializeField] private Button firstSelected;
    [SerializeField] private CanvasGroup parentScreen;
    [SerializeField] private EventSystem eventSystem;
    
    
    [Header("Button Tabs")]
    [SerializeField] private Button audioTab;
    [SerializeField] private Button controlTab;
    
    // State variables
    private Rewired.Player player;
    private GameObject previouslySelectedGameObject;

    private void OnEnable()
    {
        if (parentScreen)
        {
            parentScreen.gameObject.SetActive(false);
            parentScreen.interactable = false;
            previouslySelectedGameObject = eventSystem.currentSelectedGameObject;
        }
        
        firstSelected.Select();
    }
    
    private void OnDisable()
    {
        if (parentScreen)
        {
            parentScreen.interactable = true;
        }
        
    }

    void Start()
    {
        var players = ReInput.players.GetPlayers();

        if (players.Count != 0)
        {
            player = players[0];
        }
        
    }

    void Update()
    {
        if (player == null)
        {
            return;
        }
        
        if (player.GetButtonDown("UICancel"))
        {
            gameObject.SetActive(false);

            if (parentScreen)
            {
                parentScreen.gameObject.SetActive(true);
                parentScreen.interactable = true;
                eventSystem.SetSelectedGameObject(previouslySelectedGameObject);
            }
        }
    }

    public void SwitchTab(string tab)
    {
        switch (tab)
        {
            case "audio":
            {
                controlTab.image.color = Color.white;
                audioTab.image.color = Color.blue;
                break;
            }
            case "controls":
            {
                Debug.Log("HEY");
                audioTab.image.color = Color.white;
                controlTab.image.color = Color.blue;
                break;
            }
        }
    }
}
