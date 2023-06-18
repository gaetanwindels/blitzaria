using System;
using System.Collections.Generic;
using enums;
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
        private Animator _animator;

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
            _animator = GetComponent<Animator>();
            UpdateScore();
        }
        
        void OnGoalScored(TeamEnum scored)
        {
            if (scored != team)
            {
                _score++;
                UpdateScore();
                _animator.SetTrigger(AnimatorParameters.TriggerGoalScored);
            }
        }

        void UpdateScore()
        {
            _scoreText.text = _score.ToString();
        }

    }
}
