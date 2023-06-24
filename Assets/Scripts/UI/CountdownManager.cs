using enums;
using ScriptableObjects.Events;
using TMPro;
using UnityEngine;

namespace UI
{
    [
        RequireComponent(typeof(TextMeshProUGUI)), 
        RequireComponent(typeof(Animator))
    ]
    public class CountdownManager : MonoBehaviour
    {
        // Config parameters
        [SerializeField] private EventChannel eventChannel;
    
        // Cached variables
        private Animator _animator;
        private TextMeshProUGUI _countdownText;
        
        // State variables
        private float _countDownTimer;
        private bool _isCountDown;
        
        void Start()
        {
            _countdownText = GetComponent<TextMeshProUGUI>();
            _animator = GetComponent<Animator>();
        }

        void Update()
        {
            ManageCountdown();
        }
        
        private void DisplayText(string text)
        {
            _countdownText.text = text;
            _animator.SetTrigger(AnimatorParameters.TriggerText);
        }

        public void StartCountdown()
        {
            _countDownTimer = 3f;
            _isCountDown = true;
        }

        private void ManageCountdown()
        {
            if (!_isCountDown)
            {
                return;
            }

            var isFirst = Mathf.FloorToInt(_countDownTimer) == 3;
            var previousCountDown = Mathf.CeilToInt(_countDownTimer);
            _countDownTimer -= Time.deltaTime;
            var roundedCountDown = Mathf.CeilToInt(_countDownTimer);

            if (previousCountDown != roundedCountDown || isFirst)
            {
                DisplayText(roundedCountDown != 0 ? roundedCountDown.ToString() : "GO!");
            }

            if (_countDownTimer < 0)
            {
                _isCountDown = false;
                eventChannel.RaiseCountdownOver();
            }
        }
    }
    
}
