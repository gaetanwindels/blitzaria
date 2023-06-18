using enums;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class MiddleTextManager : MonoBehaviour
    {
        // Cached variables
        private Animator _animator;
        private TextMeshProUGUI _textMeshProUGUI;

        // Start is called before the first frame update
        void Start()
        {
            _animator = GetComponent<Animator>();
            _textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        }

        public void DisplayText(string text)
        {
            _textMeshProUGUI.text = text;
            _animator.SetTrigger(AnimatorParameters.TriggerText);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
