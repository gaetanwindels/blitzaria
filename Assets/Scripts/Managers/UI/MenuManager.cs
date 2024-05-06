using System;
using UnityEngine;

namespace Managers.UI
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] CanvasGroup mainMenuScreen;
        [SerializeField] CanvasGroup settingsScreen;

        private void Start()
        {
            CloseSettings();
        }

        public void OpenSettings()
        {
            //mainMenuScreen.gameObject.SetActive(false);
            settingsScreen.gameObject.SetActive(true);
            //mainMenuScreen.interactable = false;
            settingsScreen.interactable = true;
        }
        
        public void CloseSettings()
        {
            mainMenuScreen.gameObject.SetActive(true);
            mainMenuScreen.interactable = true;
            settingsScreen.interactable = false;
            settingsScreen.gameObject.SetActive(false);
        }
    }
}