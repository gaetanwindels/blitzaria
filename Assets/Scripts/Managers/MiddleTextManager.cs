using System;
using System.Collections;
using enums;
using ScriptableObjects.Events;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class MiddleTextManager : MonoBehaviour
    {
        // Config parameters
        [SerializeField] private EventChannel eventChannel;
        
        // Cached variables
        private TextMeshProUGUI _textMeshProUGUI;

        private void OnEnable()
        {
            eventChannel.GoalScoredEvent += HandleGoalScored;
        }

        // Start is called before the first frame update
        void Start()
        {
            _textMeshProUGUI = GetComponent<TextMeshProUGUI>();
            _textMeshProUGUI.text = "";
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        private void HandleGoalScored(TeamEnum teamEnum)
        {
            DisplayText("GOAL!", 1f);
        }
        
        public void DisplayText(string text)
        {
            _textMeshProUGUI.text = text;
        }
        
        public void DisplayText(string text, float duration)
        {
            _textMeshProUGUI.text = text;
            StartCoroutine(ResetSceneRoutine(duration));
        }
        
        IEnumerator ResetSceneRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            _textMeshProUGUI.text = "";
        }
    }
}
