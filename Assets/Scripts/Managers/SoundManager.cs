using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    [SerializeField] AudioClip music;

    public static SoundManager instance = null;

    private AudioSource source;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("START SOUND MANAGER");
        source = GetComponent<AudioSource>();

        if (!source.isPlaying && music != null)
        {
            source.volume = 0.1f;
            source.clip = music;
            source.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
