using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    // Config parameters
    [Header("Players")]
    [SerializeField] Player[] players;


    [SerializeField] int gameDuration = 120;
    [SerializeField] int countdownDuration = 5;


    [Header("GUI FIELDS")]
    [SerializeField] GameObject pauseCanvas;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI scoreTeam1Text;
    [SerializeField] TextMeshProUGUI scoreTeam2Text;
    [SerializeField] TextMeshProUGUI countDownText;
    [SerializeField] TextMeshProUGUI winText;
    [SerializeField] GameObject restartButton;
    [SerializeField] Slider sliderEnergyBar1;
    [SerializeField] Slider sliderEnergyBar2;
    [SerializeField] Slider sliderEnergyBar3;
    [SerializeField] Slider sliderEnergyBar4;

    // State variable
    public static GameManager instance = null;
    public float timer;
    public float countDownTimer;
    private int scoreTeam1;
    private int scoreTeam2;
    private bool isCountDown = true;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log("OnSceneLoaded:" + countDownTimer);
        players = FindObjectsOfType<Player>();

        if (sliderEnergyBar1 != null)
        {
            sliderEnergyBar1.gameObject.SetActive(false);
        }
        if (sliderEnergyBar2 != null)
        {
            sliderEnergyBar2.gameObject.SetActive(false);
        }
        if (sliderEnergyBar3 != null)
        {
            sliderEnergyBar3.gameObject.SetActive(false);
        }
        if (sliderEnergyBar4 != null)
        {
            sliderEnergyBar4.gameObject.SetActive(false);
        }

        foreach (Player player in players)
        {
            if (player.playerNumber == 0 && sliderEnergyBar1 != null)
            {
                sliderEnergyBar1.gameObject.SetActive(true);
                sliderEnergyBar1.value = 1;
            } else if (player.playerNumber == 1 && sliderEnergyBar1 != null)
            {
                sliderEnergyBar2.gameObject.SetActive(true);
                sliderEnergyBar2.value = 1;
            } else if (player.playerNumber == 2 && sliderEnergyBar1 != null)
            {
                sliderEnergyBar3.gameObject.SetActive(true);
                sliderEnergyBar3.value = 1;
            } else if (player.playerNumber == 3 && sliderEnergyBar1 != null)
            {
                sliderEnergyBar4.gameObject.SetActive(true);
                sliderEnergyBar4.value = 1;
            }

            player.DisableInputs();
        }

        isCountDown = true;
        winText.text = "";
        countDownTimer = countdownDuration;
        Time.timeScale = 1;
    }

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
        timer = gameDuration;
        countDownTimer = countdownDuration;
        scoreTeam1 = 0;
        scoreTeam2 = 0;
        winText.text = "";
        restartButton.SetActive(false);
        players = FindObjectsOfType<Player>();
        foreach (Player player in players)
        {
            player.isActive = false;
        }

        Time.timeScale = 1;

    }

    void StartGame()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ManageTimer();
        ManageCountDown();
        UpdateEnergy();

        restartButton.SetActive(IsGameOver());

        if (scoreTeam1Text != null)
        {
            scoreTeam1Text.text = scoreTeam1.ToString();
        }

        if (scoreTeam2Text != null)
        {
            scoreTeam2Text.text = scoreTeam2.ToString();
        }

        if (IsGameOver())
        {
            var score1 = int.Parse(scoreTeam1Text.text);
            var score2 = int.Parse(scoreTeam2Text.text);

            if (score1 == score2)
            {
                winText.text = "Draw";
            } else
            {
                winText.text = int.Parse(scoreTeam1Text.text) > int.Parse(scoreTeam2Text.text) ? "Team 1" : "Team 2";
                winText.text += " win";
            }
            
            Time.timeScale = 0;
        }
    }

    public void ManagePause()
    {
        if (!this.pauseCanvas.activeSelf)
        {
            Pause();
        } else
        {
            Resume();
        }
    }

    private void ManageTimer()
    {
        if (!isCountDown)
        {
            timer -= Time.deltaTime;
            timer = Mathf.Max(0, timer);
        }
        
        if (timerText != null)
        {
            var minutes = Mathf.Floor(timer / 60);
            var seconds = Mathf.Max(0, Mathf.Round(timer % 60));
            var secondsText = seconds.ToString();
            if (seconds < 10)
            {
                secondsText = "0" + seconds;
            }

            if (seconds == 60)
            {
                secondsText = "00";
                minutes++;
            }

            timerText.text = minutes + ":" + secondsText;
        }
    }

    private void ManageCountDown()
    {
        if (!isCountDown)
        {
            return;
        }

        countDownTimer -= Time.deltaTime;
        countDownText.text = Mathf.Ceil(countDownTimer).ToString();

        if (countDownTimer < 0)
        {
            isCountDown = false;
            countDownText.text = "";
            var positionLockers = FindObjectsOfType<PositionLocker>();
            var playersFound = FindObjectsOfType<Player>();

            foreach (PositionLocker positionLocker in positionLockers)
            {
                positionLocker.UnlockObject();
            }

            foreach (Player player in players)
            {
                player.EnableInputs();
            }
        }

    }

    public void AddScore(TeamEnum team)
    {
        if (team == TeamEnum.Team1)
        {
            winText.text = "RED TEAM SCORE";
            winText.color = new Color32(255, 55, 55, 255);
            scoreTeam2++;
        } else
        {
            scoreTeam1++;
            winText.text = "BLUE TEAM SCORE";
            winText.color = new Color32(57, 105, 255, 255);
        }
    }

    public void UpdateEnergy() 
    {
        foreach (Player player in players)
        {
            if (player.playerNumber == 0)
            {
                sliderEnergyBar1.value = (player.currentEnergy / GameSettings.energyAmount);
            } else if (player.playerNumber == 1)
            {
                sliderEnergyBar2.value = (player.currentEnergy / GameSettings.energyAmount);
            }
            else if (player.playerNumber == 2)
            {
                sliderEnergyBar3.value = (player.currentEnergy / GameSettings.energyAmount);
            }
            else if (player.playerNumber == 3)
            {
                sliderEnergyBar4.value = (player.currentEnergy / GameSettings.energyAmount);
            }
        }  
    }

    public bool IsGameOver()
    {
        return timer <= 0;
    }

    private void Pause()
    {
        if (pauseCanvas !=  null && !pauseCanvas.activeSelf)
        {
            pauseCanvas.SetActive(true);
        }
        
        Time.timeScale = 0;
    }

    private void Resume()
    {
        if (pauseCanvas != null && pauseCanvas.activeSelf)
        {
            pauseCanvas.SetActive(false);
        }

        Time.timeScale = 1;
    }
}
