using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneManager : MonoBehaviour
{

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
        var soundManager = GameObject.Find("SoundManager");
        Destroy(manager);
        Destroy(soundManager);
        SceneManager.LoadScene(0);
    }

    public void GoToPlay()
    {
        Debug.Log("hey");
        SceneManager.LoadScene("Game Scene");
    }

    public void GoToPlay1vs1()
    {
        SceneManager.LoadScene("Game Scene 1vs1");
    }
}
