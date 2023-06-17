using System;
using System.Collections.Generic;
using ScriptableObjects.Events;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class ScoreManager : MonoBehaviour
    {
        // Config parameters
        [SerializeField] private TeamEnum team;
        [SerializeField] private EventChannel eventChannel;
        
        // Cached variables
        private TextMeshProUGUI _scoreText;

        // State variables        
        private int _score;

        private void OnEnable()
        {
            eventChannel.GoalScoredEvent += OnGoalScored;
        }
        
        private void OnDisable()
        {
            eventChannel.GoalScoredEvent -= OnGoalScored;
        }

        void Start()
        {
            // var goals = FindObjectsOfType<Goal>();
            // foreach (Goal goal in goals)
            // {
            //     goal.GoalScored += OnGoalScored;
            // }

            _scoreText = GetComponent<TextMeshProUGUI>();
            UpdateScore();
        }
        
        void OnGoalScored(TeamEnum scored)
        {
            if (scored != team)
            {
                Debug.Log("SCORED BY " + scored);
                _score++;
                UpdateScore();
            }
        }

        void UpdateScore()
        {
            _scoreText.text = _score.ToString();
        }

    }
}
