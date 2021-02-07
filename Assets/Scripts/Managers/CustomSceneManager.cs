using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneManager : MonoBehaviour
{

    // State variable
    public static CustomSceneManager instance = null;

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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoToMenu()
    {
        var manager = GameObject.Find("GameManager");
        Destroy(manager);
        SceneManager.LoadScene(0);
    }

    public void GoToPlay()
    {
        Debug.Log("hey");
        SceneManager.LoadScene("Game Scene");
    }
}
