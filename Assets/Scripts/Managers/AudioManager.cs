using UnityEngine;
using UnityEngine.Audio;

namespace Managers
{
    public class AudioManager : MonoBehaviour
    {
        // Config parameters
        [SerializeField] private AudioMixer audioMixer;
        
        // Constants
        private const string MixerMaster = "MasterVolume";
        private const string MixerMusic = "MusicVolume";
        private const string MixerSfx = "SfxVolume";
        private const float DefaultValue = 0.5f;

        void Start()
        {
            SetMasterVolume(PlayerPrefs.GetFloat(MixerMaster, DefaultValue));
            SetMusicVolume(PlayerPrefs.GetFloat(MixerMusic, DefaultValue));
            SetSfxVolume(PlayerPrefs.GetFloat(MixerSfx, DefaultValue));
        }
        
        void SetMasterVolume(float value)
        {
            SetVolume(MixerMaster, value);
        }

        void SetMusicVolume(float value)
        {
            SetVolume(MixerMusic, value);
        }
    
        void SetSfxVolume(float value)
        {
            SetVolume(MixerSfx, value);
        }

        void SetVolume(string mixer, float value)
        {
            if (!audioMixer) return;
            
            var volume = Mathf.Log10(value) * 20;
            Debug.Log("Volume!" + volume);
            audioMixer.SetFloat(mixer, volume);
        }
    }
}