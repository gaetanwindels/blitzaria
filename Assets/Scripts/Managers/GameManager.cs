using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using enums;
using Managers;
using ScriptableObjects.Events;
using UI;
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
    
    [Header("Ball")]
    [SerializeField] GameObject ball;

    [Header("Default player")]
    [SerializeField] GameObject defaultPlayer;

    [Header("Time")]
    [SerializeField] int gameDuration = 120;
    [SerializeField] int countdownDuration = 5;

    [Header("Managers")]
    [SerializeField] TimerManager timerManager;
    [SerializeField] CountdownManager countdownManager;
    [SerializeField] ScoreManager scoreManagerTeam1;
    [SerializeField] ScoreManager scoreManagerTeam2;
    [SerializeField] MiddleTextManager _middleTextManager;

    [Header("GUI FIELDS")]
    [SerializeField] GameObject endgameScreen;
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
    private GameSessionConfiguration gameSession;
    private MatchState _matchState;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        _eventChannel.TimeoutEvent += HandleGameOver;
        _eventChannel.ScoreUpdatedEvent += HandleGoalScored;
        _eventChannel.CountdownOverEvent += HandleCountdownOver;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        _eventChannel.TimeoutEvent -= HandleGameOver;
        _eventChannel.GoalScoredEvent -= HandleGoalScored;
        _eventChannel.CountdownOverEvent -= HandleCountdownOver;
    }

    IEnumerator ResetSceneRoutine()
    {
        yield return new WaitForSeconds(2f);
        ReloadScene();
    }

    private void ReloadScene()
    {
        SpawnBall();
        StartingPositionsManager positionManager = FindObjectOfType<StartingPositionsManager>();
        positionManager.PositionPlayers();
        positionManager.PositionBall();
        countdownManager.StartCountdown();
    }

    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        gameSession = FindObjectOfType<GameSessionConfiguration>();
        playerConfigurations = gameSession.players;

        InitPlayers();
        SpawnBall();

        players = FindObjectsOfType<Player>();

        foreach (Player player in players)
        {
            player.DisableInputs();
        }

        isTimerStopped = false;
        winText.text = "";
        Time.timeScale = 1;
        _matchState = MatchState.CountdownRegular;
        endgameScreen.SetActive(false);
    }

    void Awake()
    {
        // if (instance == null)
        // {
        //     instance = this;
        // }
        // else if (instance != this)
        // {
        //     Destroy(gameObject);
        // }

        //Sets this to not be destroyed when reloading scene
        //DontDestroyOnLoad(gameObject);
    }

    private void HandleGameOver()
    {
        if (scoreManagerTeam1.Score == scoreManagerTeam2.Score)
        {
            _matchState = MatchState.CountdownOvertime;
            if (timerManager != null)
            {
                timerManager.InitOvertime();
            }
            ReloadScene();
        } else if (scoreManagerTeam1.Score > scoreManagerTeam2.Score)
        {
            Time.timeScale = 0;
            //_middleTextManager.DisplayText("TEAM 1 WON!");
            _matchState = MatchState.Finished;
            endgameScreen.SetActive(true);
        } else
        {
            //_middleTextManager.DisplayText("TEAM 2 WON!");
            Time.timeScale = 0;
            _matchState = MatchState.Finished;
            endgameScreen.SetActive(true);
        }
    }
    
    private void HandleGoalScored(TeamEnum scored)
    {
        timerManager.StopTimer();

        if (_matchState == MatchState.RunningOvertime)
        {
            HandleGameOver();
        }
        else
        {
            StartCoroutine(ResetSceneRoutine());  
        }

    }

    private void SpawnBall()
    {
        var existingBall = FindObjectOfType<Ball>();
        if (existingBall != null)
        {
            Destroy(existingBall.gameObject);
        }
        Instantiate(ball, new Vector3(0, 0, 0), Quaternion.identity);
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
        winText.text = "";
        //restartButton.SetActive(false);
        //mainMenuButton.SetActive(false);
        gameSession = FindObjectOfType<GameSessionConfiguration>();

        if (gameSession != null)
        {
            playerConfigurations = gameSession.players;
        }

        if (timerManager != null)
        {
            timerManager.Init(180);
        }
        
        players = FindObjectsOfType<Player>();
        pauseCanvas.SetActive(false);
        Time.timeScale = 1;
        countdownManager.StartCountdown();
    }

    // Update is called once per frame
    void Update()
    {

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

    private void HandleCountdownOver()
    {
       
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
        
        if (timerManager != null)
        {
            timerManager.StartTimer();
        }

        if (_matchState == MatchState.CountdownOvertime)
        {
            _matchState = MatchState.RunningOvertime;
        }
        else
        {
            _matchState = MatchState.RunningRegular;
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
