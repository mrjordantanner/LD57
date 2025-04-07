using UnityEngine;
using System.Linq;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class ResultsPanel : MenuPanel
{
    public GameObject ReplayButtonObject;
    public GameObject BackButtonObject;
    public MenuPanel scoreCardPanel, leaderboardPanel;

    [Header("Labels")]
    public TextMeshProUGUI newBestScoreNotification;
    public TextMeshProUGUI
        scoreLabel1, scoreLabel2,
        bestScoreLabel;

    public GameObject LevelResultsRowPrefab;
    public VerticalLayoutGroup LevelDetailsContentContainer;
    public List<GameObject> levelResultsRows = new();

    public override void Show(float fadeDuration = 0.2f, bool setActivePanel = true)
    {
        newBestScoreNotification.enabled = false;
        BackButtonObject.SetActive(false);
        ReplayButtonObject.SetActive(false);

        base.Show(fadeDuration);
    }

    public IEnumerator ShowResults()
    {
        scoreCardPanel.Show(0.4f);

        scoreLabel1.text = scoreLabel2.text = Utils.FormatNumberWithCommas(PlayerData.Instance.Data.Score);
        bestScoreLabel.text = Utils.FormatNumberWithCommas(PlayerData.Instance.Data.BestScore);

        newBestScoreNotification.enabled = PlayerData.Instance.Data.IsNewHighScore;

        yield return new WaitForSecondsRealtime(1f);

        if (levelResultsRows.Count > 0)
        {
            Utils.DestroyListOfItems(levelResultsRows);
        }

        yield return new WaitForSecondsRealtime(1f);

        ShowLeaderboard();

        ReplayButtonObject.SetActive(true);
    }

    public void ShowLeaderboard(bool showBackButton = false)
    {
        LeaderboardController.Instance.CreateRows();
        LeaderboardController.Instance.ScrollToRow(LeaderboardController.Instance.currentUserIndex);
        leaderboardPanel.Show(0.4f);

        if (showBackButton) BackButtonObject.SetActive(true);
    }

    // Callbacks
    public void OnReplayButtonClick()
    {
        GameManager.Instance.ReplayGame();
        Hide();
    }

    public void OnBackButtonClick()
    {
        Menu.Instance.TitleScreenPanel.Show();
        Hide();

    }
    public override void Hide(float fadeDuration = 0.1f)
    {
        base.Hide(fadeDuration);
        ReplayButtonObject.SetActive(false);
        BackButtonObject.SetActive(false);

        scoreCardPanel.Hide();
        leaderboardPanel.Hide();
    }

}
