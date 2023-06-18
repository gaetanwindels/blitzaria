using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using Managers;
using ScriptableObjects.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{

    // Config parameters
    [Header("Events")] 
    [SerializeField] private EventChannel _eventChannel;
    
    [Header("Players")]
    [SerializeField] List<PlayerSelectConfiguration> playerConfigurations = new List<PlayerSelectConfiguration>();
    [SerializeField] Player[] players;

    [Header("Default player")]
    [SerializeField] GameObject defaultPlayer;

    [Header("Time")]
    [SerializeField] int gameDuration = 120;
    [SerializeField] int countdownDuration = 5;

    [Header("Managers")]
    [SerializeField] TimerManager _timerManager;
    [SerializeField] MiddleTextManager _middleTextManager;

    [Header("GUI FIELDS")]
    [SerializeField] GameObject pauseCanvas;
    [SerializeField] TextMeshProUGUI countDownText;
    [SerializeField] TextMeshProUGUI winText;
    [SerializeField] GameObject restartButton;
    [SerializeField] GameObject mainMenuButton;
    [SerializeField] Slider sliderEnergyBar1;
    [SerializeField] Slider sliderEnergyBar2;
    [SerializeField] Slider sliderEnergyBar3;
    [SerializeField] Slider sliderEnergyBar4;

    // State variable
    public static GameManager instance = null;
    public float timer;
    public bool isTimerStopped;
    public float countDownTimer;
    private int scoreTeam1;
    private int scoreTeam2;
    private bool isCountDown = true;
    private GameSessionConfiguration gameSession;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        _eventChannel.TimeoutEvent += HandleGameOver;
        _eventChannel.GoalScoredEvent += HandleGoalScored;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        _eventChannel.TimeoutEvent -= HandleGameOver;
        _eventChannel.GoalScoredEvent -= HandleGoalScored;
    }

    IEnumerator ResetScene()
    {
        yield return new WaitForSeconds(2f);
        StartingPositionsManager positionManager = FindObjectOfType<StartingPositionsManager>();
        positionManager.PositionPlayers();
        positionManager.PositionBall();
        FindObjectOfType<Ball>(true).gameObject.SetActive(true);
        isCountDown = true;
        countDownTimer = countdownDuration;
    }

    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        gameSession = FindObjectOfType<GameSessionConfiguration>();
        playerConfigurations = gameSession.players;

        InitPlayers();

        players = FindObjectsOfType<Player>();

        foreach (Player player in players)
        {
            player.DisableInputs();
        }

        isTimerStopped = false;
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

    private void HandleGameOver()
    {
        Time.timeScale = 0;
    }
    
    private void HandleGoalScored(TeamEnum scored)
    {
        FindObjectOfType<Ball>().gameObject.SetActive(false);
        _timerManager.StopTimer();
        _middleTextManager.DisplayText("GOAL!");
        StartCoroutine(ResetScene());
    }
    
    private void InitPlayers()
    {
        StartingPositionsManager positionManager = FindObjectOfType<StartingPositionsManager>();

        if (gameSession == null)
        {
            Debug.Log(gameSession.players.Count);
            Instantiate(defaultPlayer, transform.position, Quaternion.identity);
            var player2 = Instantiate(defaultPlayer, transform.position, Quaternion.identity).GetComponent<Player>();
            player2.team = TeamEnum.Team2;
            player2.playerNumber = 1;
            var clothes = player2.GetComponentsInChildren<SpriteRenderer>();
            clothes[clothes.Length - 1].color = new Color(1, 0, 0, 1);
        } else
        {
            Debug.Log("heyo" + gameSession.players.Count);
            playerConfigurations = gameSession.players;
            var positionLocker = FindObjectOfType<PositionLocker>();
            
            foreach (PlayerSelectConfiguration playerConf in playerConfigurations)
            {
                var go = Instantiate(playerConf.player, positionLocker.transform.position, Quaternion.identity);
                var player = go.GetComponent<Player>();
                player.playerNumber = playerConf.playerNumber;
                player.team = playerConf.team;
                var clothes = go.GetComponentsInChildren<SpriteRenderer>();
                //clothes[clothes.Length - 1].color = playerConf.color;
            }
        }

        positionManager.PositionPlayers();
    }

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    void Init()
    {
        timer = gameDuration;
        countDownTimer = countdownDuration;
        scoreTeam1 = 0;
        scoreTeam2 = 0;
        winText.text = "";
        //restartButton.SetActive(false);
        //mainMenuButton.SetActive(false);
        gameSession = FindObjectOfType<GameSessionConfiguration>();

        if (gameSession != null)
        {
            playerConfigurations = gameSession.players;
        }

        if (_timerManager != null)
        {
            _timerManager.Init(180);
        }
        
        players = FindObjectsOfType<Player>();
        pauseCanvas.SetActive(false);
        Time.timeScale = 1;
    }
    
    IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Update is called once per frame
    void Update()
    {
        ManageCountDown();
        UpdateEnergy();

        //restartButton.SetActive(IsGameOver());
        //mainMenuButton.SetActive(IsGameOver());

        //if (IsGameOver())
       // {
            // if (score1 == score2)
            // {
            //     winText.text = "Draw";
            // } else
            // {
            //     winText.text = int.Parse(scoreTeam1Text.text) > int.Parse(scoreTeam2Text.text) ? "Team 1" : "Team 2";
            //     winText.text += " win";
            // }

            EventSystem.current.SetSelectedGameObject(mainMenuButton);
        //}
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Init();
    }

    public void ManagePause()
    {
        if (!pauseCanvas.activeSelf)
        {
            Pause();
        } else
        {
            Resume();
        }
    }

    private void ManageCountDown()
    {
        if (!isCountDown)
        {
            return;
        }
        
        var isFirst = Mathf.FloorToInt(countDownTimer) == 3;
        var previousCountDown = Mathf.CeilToInt(countDownTimer);
        countDownTimer -= Time.deltaTime;
        var roundedCountDown = Mathf.CeilToInt(countDownTimer);

        if (previousCountDown != roundedCountDown || isFirst)
        {
            Debug.Log("YO");
            _middleTextManager.DisplayText(roundedCountDown != 0 ? roundedCountDown.ToString() : "Go");
        }

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
                Debug.Log("enabled");
                player.EnableInputs();
            }
            
            if (_timerManager != null)
            {
                _timerManager.StartTimer();
            }
            
        }

    }

    public void AddScore(TeamEnum team)
    {
        if (team == TeamEnum.Team1)
        {
            winText.text = "RED TEAM SCORE";
            winText.color = new Color32(255, 55, 55, 255);
            scoreTeam1++;
        } else
        {
            scoreTeam2++;
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

    public bool IsPaused()
    {
        return pauseCanvas.activeSelf;
    }

    private void Pause()
    {
        Debug.Log("pausing" + pauseCanvas);
        if (pauseCanvas != null && !pauseCanvas.activeSelf)
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
