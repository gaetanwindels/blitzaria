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
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI scoreTeam1Text;
    [SerializeField] TextMeshProUGUI scoreTeam2Text;
    [SerializeField] TextMeshProUGUI countDownText;
    [SerializeField] Slider sliderEnergyBar1;
    [SerializeField] Slider sliderEnergyBar2;

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
        Debug.Log(mode);
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
        players = FindObjectsOfType<Player>();

        foreach (Player player in players)
        {
            player.isActive = false;
        }

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

        if (scoreTeam1Text != null)
        {
            scoreTeam1Text.text = scoreTeam1.ToString();
        }

        if (scoreTeam2Text != null)
        {
            scoreTeam2Text.text = scoreTeam2.ToString();
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
            foreach (Player player in players)
            {
                player.isActive = true;
            }
        }

    }

    public void AddScore(TeamEnum team)
    {
        if (team == TeamEnum.Team1)
        {
            scoreTeam2++;
        } else
        {
            scoreTeam1++;
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
        }  
    }
}
