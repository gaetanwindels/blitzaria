using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSessionConfiguration : MonoBehaviour
{

    public List<PlayerSelectConfiguration> players = new();

    private static GameSessionConfiguration _instance;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}
