using Extensions;
using UnityEngine;

public class Wall : MonoBehaviour
{

    private AudioSource _audioSource;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_audioSource != null)
        {
            _audioSource.PlayWithRandomPitch();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

}
