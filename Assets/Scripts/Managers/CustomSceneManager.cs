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
        var gameSession = GameObject.Find("GameSessionConfiguration");
        var soundManager = GameObject.Find("SoundManager");
        Destroy(manager);
        Destroy(soundManager);
        Destroy(gameSession);
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void GoToSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Select Scene");
    }

    public void GoToPlay()
    {
        SceneManager.LoadScene("Game Scene");
    }
}
