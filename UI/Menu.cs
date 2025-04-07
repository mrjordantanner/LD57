using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using System;
using static UnityEngine.Rendering.DebugUI;


public class Menu : MonoBehaviour, IInitializable
{
    #region Singleton
    public static Menu Instance;
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
        }
    #endregion

    #region Declarations

    public string Name { get { return "Menus"; } }

    [ReadOnly] public MenuPanel ActiveMenuPanel;

    [Header("Menu Panels")]
    public TitleScreen TitleScreenPanel;
    public PauseMenu PauseMenuPanel;
    public ResultsPanel ResultsPanel;
    public NameEntryPanel NameEntryPanel;
    public MenuPanel 
        LeaderboardMenuPanel,
        InstructionsMenuPanel;

    [Header("Dialog Box")]
    public GameObject DialogBoxPrefab;
    public GameObject DialogBoxContainer;

    [Header("BGs")]
    public GameObject FullscreenMenuBackground;

    [HideInInspector] public SettingsSlider[] settingsSliders;

    public TextMeshProUGUI versionNumberText;
    public TextMeshProUGUI companyNameText;
    
    [HideInInspector] public bool enteringText;
    #endregion

    public IEnumerator Init()
    {
        FullscreenMenuBackground.SetActive(true);

        versionNumberText.text = $"v{Application.version}";
        companyNameText.text = Application.companyName;

        settingsSliders = FindObjectsOfType<SettingsSlider>();
        RefreshAllSliders();

        //MenuBackgroundsSetActive(true);

        yield return new WaitForSecondsRealtime(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M) && GameManager.Instance.gameRunning && !enteringText)
        {
            if (!GameManager.Instance.gamePaused)
            {
                ShowPauseMenu();
                PlayClickSound();
            }
            else
            {
                if (ActiveMenuPanel == PauseMenu.Instance)
                {
                    BackToGame();
                    PlayClickSound();
                }
                else
                {
                    BackToPauseMenu();
                    PlayClickSound();
                }
            }
        }
    }

    public void RefreshAllSliders()
    {
        foreach (var slider in settingsSliders)
        {
            slider.Refresh();
        }
    }


    public void CreateDialogBox(
    string headerText,
    string messageText,
    string descriptionText,
    Sprite sprite,
    Action onOk,
    Action onCancel)
    {
        var NewDialogBox = Instantiate(DialogBoxPrefab, DialogBoxContainer.transform.position, Quaternion.identity, DialogBoxContainer.transform);
        var dialogBox = NewDialogBox.GetComponent<DialogBox>();
        dialogBox.Initialize(headerText, messageText, descriptionText, sprite, onOk, onCancel);
    }


    #region Panel Methods
    public void ShowPauseMenu()
    {
        GameManager.Instance.Pause();
        //FullscreenMenuBackground.SetActive(true);
        HUD.Instance.Hide();
        //HUD.Instance.ShowPointerCursor();
        PauseMenu.Instance.Show();
    }

    public void ToggleCanvasGroup(
        bool enabled,
        CanvasGroup canvasGroup,
        float targetOpacity,
        float fadeDuration = 0.3f)
    {
        canvasGroup.DOFade(targetOpacity, fadeDuration).SetUpdate(UpdateType.Normal, true);
        canvasGroup.interactable = enabled;
        canvasGroup.blocksRaycasts = enabled;
    }

    public void FadeInCanvasGroup(CanvasGroup canvasGroup, float fadeDuration = 0.2f)
    {
        canvasGroup.DOFade(1f, fadeDuration).SetUpdate(UpdateType.Normal, true);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void FadeOutCanvasGroup(CanvasGroup canvasGroup, float fadeDuration = 0.2f)
    {
        canvasGroup.DOFade(0f, fadeDuration).SetUpdate(UpdateType.Normal, true);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public IEnumerator SwapMenus(MenuPanel newPanel, float transitionTime = 0.2f)
    {
        ActiveMenuPanel.Hide();
        yield return new WaitForSecondsRealtime(transitionTime);
        newPanel.Show();
    }
    #endregion


    #region Button Callbacks
    public void StartButtonOnClick()
    {
        PlayClickSound();
        ShowNameEntryPanel();
    }

    public void RankingsButtonOnClick()
    {
        PlayClickSound();
        StartCoroutine(LoadLeaderboardPanel());
    }

    IEnumerator LoadLeaderboardPanel()
    {
        LeaderboardService.Instance.StartLeaderboardFetchRetry();

        ResultsPanel.Show();
        ResultsPanel.scoreCardPanel.Hide();
        yield return new WaitForSecondsRealtime(1);

        ResultsPanel.ShowLeaderboard(true);
    }


    public void Swap(MenuPanel newPanel)
    {
        StartCoroutine(SwapMenus(newPanel));
    }

    public void BackToGame()
    {
        //FullscreenMenuBackground.SetActive(false);
        HUD.Instance.Show();
        PauseMenu.Instance.Hide();
        GameManager.Instance.Unpause();
    }

    public void BackToPauseMenu()
    {
        StartCoroutine(SwapMenus(PauseMenuPanel));
    }

    public void PlayClickSound()
    {
        //AudioManager.Instance.soundBank.MenuClick.Play();
    }


    public void RestartGameWithFadeOut(bool fadeToWhite = true)
    {
        StartCoroutine(RestartGameWithFadeOutRoutine(fadeToWhite));
    }

    IEnumerator RestartGameWithFadeOutRoutine(bool fadeToWhite = true)
    {
        if (fadeToWhite) HUD.Instance.screenFader.FadeToWhite(0.5f);
        else HUD.Instance.screenFader.FadeOut(0.5f);

        AudioManager.Instance.musicAudioSource.DOFade(0, 0.5f).SetUpdate(UpdateType.Normal, true);
        yield return new WaitForSecondsRealtime(0.5f);

        GameManager.Instance.RestartGame();
    }

    public IEnumerator ReturnToTitleScreen()
    {
        //MenuBackgroundsSetActive(true);
        TitleScreenPanel.Show();

        HUD.Instance.screenFader.FadeIn(1);
        AudioManager.Instance.FadeMusicIn(AudioManager.Instance.titleScreenMusic);
        yield return new WaitForSecondsRealtime(1);
    }

    public void ShowNameEntryPanel()
    {
        enteringText = true;

        NameEntryPanel.Show();
        TitleScreenPanel.Hide();
    }

    public void ConfirmNameEntry()
    {
        PlayClickSound();
        enteringText = false;

        var playerName = NameEntryPanel.nameInputField.text;
        PlayerData.Instance.Data.PlayerName = playerName;
        Authentication.Instance.UpdatePlayerNameAsync(playerName);
        StartCoroutine(GameManager.Instance.InitializeNewRun());
    }

    public void QuitButton()
    {
        GameManager.Instance.Quit();
    }
   
    #endregion

}

