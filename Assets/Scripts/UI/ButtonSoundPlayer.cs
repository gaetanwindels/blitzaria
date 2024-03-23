using Extensions;
using UnityEngine;

public class ButtonSoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip selectSound;
    [SerializeField] private AudioClip clickSound;

    private AudioSource _audioSource;

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlaySelectSound()
    {
        if (_audioSource)
        {
            _audioSource.PlayClipWithRandomPitch(selectSound, false, 0.95f, 1.05f);
        }
    }

    public void PlayClickSound()
    {
        if (_audioSource)
        {
            _audioSource.PlayClipWithRandomPitch(clickSound, false, 0.95f, 1.05f);
        }
    }

}
