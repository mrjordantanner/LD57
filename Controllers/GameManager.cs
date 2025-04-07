using Cinemachine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;


public class GameManager : MonoBehaviour, IInitializable
{
    #region Singleton
    public static GameManager Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        #endregion

        StartCoroutine(Init());
    }

    #region Declarations

    public string Name { get { return "Game Manager"; } }

    public Dialogue tutorialDialogue;
    public GameObject StaticBackground;

    [Header("Logging")]
    public bool cloudLogging;

    [Header("Game State")]
    [ReadOnly]
    public bool gameRunning;
    [ReadOnly]
    public bool gamePaused;
    [ReadOnly]
    public bool inputSuspended;
    public bool showTutorial = true;

    [Header("Time")]
    [ReadOnly] public float timeScale;
    [ReadOnly] public float gameTimer;
    public bool gameTimerEnabled = true;
    #endregion

    public IEnumerator Init()
    {
        gameRunning = false;
        inputSuspended = true;
        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(0f);
    }

    void Update()
    {
        timeScale = Time.timeScale;
        if (gameTimerEnabled && gameRunning) gameTimer += Time.deltaTime;
        if (PlayerData.Instance) PlayerData.Instance.Data.TotalGameTime += Time.unscaledDeltaTime;

        // Toggle Freeze Frame
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            if (Time.timeScale > 0)
            {
                Time.timeScale = 0;
                gamePaused = true;
            }
            else if (Time.timeScale == 0)
            {
                Time.timeScale = 1;
                gamePaused = false;
            }
        }
    }

    // Button callback
    public void RestartFromPauseMenu()
    {
        PlayerManager.Instance.DespawnPlayer();
        Menu.Instance.BackToGame();
        ReplayGame();
    }

    public void ReplayGame()
    {
        PlayerData.Instance.Data.Replays++;

        showTutorial = false;
        StartCoroutine(InitializeNewRun(true));
    }

    public IEnumerator InitializeNewRun(bool isReplay = false)
    {
        // Handle screen fade if it's player's first playthrough
        if (!isReplay)
        {
            HUD.Instance.screenFader.FadeToWhite(1f);
            yield return new WaitForSecondsRealtime(1f);

            Menu.Instance.FullscreenMenuBackground.SetActive(false);
            Menu.Instance.NameEntryPanel.Hide();
            StaticBackground.SetActive(true);
        }

        gameTimer = 0;
        PlayerData.Instance.Data.ResetGameSessionData();
        PlayerManager.Instance.ResetCurrentCharges();

        StartCoroutine(StartRun());
    }

    public IEnumerator StartRun()
    {
        //print("IF YOU'RE READING THIS, THANKS FOR PLAYING MY GAME");

        LayerController.Instance.GenerateRandomLayers();
        PlayerManager.Instance.SpawnPlayer();

        HUD.Instance.screenFader.FadeIn(1f);
        yield return new WaitForSecondsRealtime(1f);

        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02f;

        //if (showTutorial)
        //{
        //    StartCoroutine(ShowTutorial());
        //}
        //else
        //{
        //    StartCoroutine(OnIntroComplete());
        //}

        StartCoroutine(OnIntroComplete());
    }

    public IEnumerator OnIntroComplete()
    {
        HUD.Instance.Show();
        inputSuspended = false;
        yield return new WaitForSecondsRealtime(2f);

        gameRunning = true;
        LevelController.Instance.Init();
    }

    //public IEnumerator ShowTutorial()
    //{
    //    Debug.Log("TODO SHOW TUTORIAL TEXT");
    //    yield return new WaitForSecondsRealtime(3f);
    //    StartCoroutine(OnIntroComplete());

    //}

    public void EndRunCallback()
    {
        StartCoroutine(EndRun());
    }

    public IEnumerator EndRun()
    {
        //print("GameManager: End Run");

        HUD.Instance.Hide();
        //HUD.Instance.ShowPointerCursor();

        PlayerData.Instance.SaveAllAsync();
        PlayerManager.Instance.DespawnPlayer();

        PauseMenu.Instance.Hide();
        //DevTools.Instance.gameplayDevToolsWindow.Hide();
        Menu.Instance.ActiveMenuPanel.Hide();

        HUD.Instance.screenFader.FadeOut(1);
        StartCoroutine(AudioManager.Instance.FadeMusicOut(1));
        yield return new WaitForSecondsRealtime(1);

        LayerController.Instance.DestroyAllLayers();
        LevelController.Instance.DestroyAllPickups();

        gameRunning = false;
        gameTimerEnabled = false;
        gameTimer = 0;
        inputSuspended = true;
        Time.timeScale = 0;

        StartCoroutine(Menu.Instance.ReturnToTitleScreen());
    }

    public IEnumerator GameOver()
    {
        HUD.Instance.Hide();
        //HUD.Instance.ShowPointerCursor();

        HUD.Instance.ShowMessage("Timer expired", 0.5f, 2f, 1f);
        yield return new WaitForSecondsRealtime(3.5f);

        PlayerManager.Instance.DespawnPlayer();

        gameRunning = false;
        PlayerData.Instance.Data.Score = PlayerManager.Instance.currentCharges * LayerController.Instance.layersVisited;
        CheckForHighScores();

        // Update cloud, push player score up, get all scores back down
        PlayerData.Instance.SaveAllAsync();
        LeaderboardService.Instance.OnPlaySessionEnd();

        // Display results
        Menu.Instance.ResultsPanel.Show();
        StartCoroutine(Menu.Instance.ResultsPanel.ShowResults());
    }

    public void CheckForHighScores()
    {
        // Best score
        if (PlayerData.Instance.Data.Score > PlayerData.Instance.Data.BestScore)
        {
            print("NEW HIGH SCORE - Updated BestScore");
            PlayerData.Instance.Data.IsNewHighScore = true;
            PlayerData.Instance.Data.BestScore = PlayerData.Instance.Data.Score;
        }

        // Best level
        //if (PlayerData.Instance.Data.LevelsCompletedThisRun > PlayerData.Instance.Data.BestLevelsCompleted)
        //{
        //    print("NEW BEST LEVEL - Updated BestLevel");
        //    PlayerData.Instance.Data.IsNewBestLevel = true;
        //    PlayerData.Instance.Data.BestLevelsCompleted = PlayerData.Instance.Data.LevelsCompletedThisRun;
        //}
    }

    public void RestartGame()
    {
        StartCoroutine(Restart());
    }

    IEnumerator Restart()
    {
        yield return new WaitForSecondsRealtime(1f);
        SceneManager.LoadScene(0);
    }

    public void Pause()
    {
        AudioManager.Instance.ReduceMusicVolume();

        inputSuspended = true;
        gamePaused = true;
        Time.timeScale = 0;
        Physics2D.simulationMode = SimulationMode2D.Script;
        HUD.Instance.screenFlash.SetActive(false);
    }

    public void Unpause()
    {
        AudioManager.Instance.RestoreMusicVolume();

        inputSuspended = false;
        gamePaused = false;
        Time.timeScale = 1;
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
        HUD.Instance.screenFlash.SetActive(true);
    }


    public void Quit()
    {
        PlayerData.Instance.SaveAllAsync();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}
