using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{

    // Config parameters
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    // Constants
    private const string MixerMaster = "MasterVolume";
    private const string MixerMusic = "MusicVolume";
    private const string MixerSfx = "SfxVolume";
    private const float DefaultValue = 0.5f;

    private void Start()
    {
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSfxVolume);
        
        masterSlider.value = PlayerPrefs.GetFloat(MixerMaster, DefaultValue);
        musicSlider.value = PlayerPrefs.GetFloat(MixerMusic, DefaultValue);
        sfxSlider.value = PlayerPrefs.GetFloat(MixerSfx, DefaultValue);
    }

    void SetMasterVolume(float value)
    {
        SetVolume(MixerMaster, masterSlider.value);
    }

    void SetMusicVolume(float value)
    {
        SetVolume(MixerMusic, musicSlider.value);
    }
    
    void SetSfxVolume(float value)
    {
        SetVolume(MixerSfx, sfxSlider.value);
    }

    void SetVolume(string mixer, float value)
    {
        if (audioMixer)
        {
            var volume = Mathf.Log10(value) * 20;
            audioMixer.SetFloat(mixer, volume);
        }
        
        PlayerPrefs.SetFloat(mixer, value);
    }
}