using DG.Tweening;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;


public class LeaderboardController : MenuPanel
{
    #region Singleton
    public static LeaderboardController Instance;
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

        Init();
    }

    public GameObject LeaderboardRowPrefab;
    public VerticalLayoutGroup table;
    public Vector3 offset = new(0, 0, 0);

    public float topYpos = 0f;
    public int currentUserIndex;
    public float scrollDuration = 1f;
    public Ease scrollEase = Ease.InOutCubic;

    int rowCount;
    float rowHeight = 40.7153f;
    List<GameObject> rows = new();

    public ScrollRect scrollRect;
    public RectTransform rectTransform;
    public LeaderboardRow rowOfCurrentUser;

    public void Init()
    {
        //rectTransform = table.gameObject.GetComponent<RectTransform>();
    }

    //private void Update()
    //{
    //    //ClampScrollRect();
    //}

    //private void ClampScrollRect()
    //{
    //    float contentHeight = rectTransform.rect.height;
    //    float viewportHeight = scrollRect.viewport.rect.height;

    //    float minY = contentHeight - viewportHeight;
    //    float maxY = rowHeight * (rowCount - 1);

    //    if (scrollRect.verticalNormalizedPosition < 0)
    //    {
    //        scrollRect.verticalNormalizedPosition = 0;
    //    }
    //    else if (scrollRect.verticalNormalizedPosition > 1)
    //    {
    //        scrollRect.verticalNormalizedPosition = 1;
    //    }

    //    float clampedY = Mathf.Clamp(rectTransform.anchoredPosition.y, minY, maxY);
    //    rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, clampedY);
    //}

    //public void FocusOnTop()
    //{
    //    rectTransform.anchoredPosition = new Vector2(0, topYpos);
    //}

    //public void FocusOnRow()
    //{
    //    Canvas.ForceUpdateCanvases();
    //    float posY = rectTransform.position.y - rowOfCurrentUser.transform.position.y;

    //    if (rowOfCurrentUser.transform.position.y <= 0)
    //    {
    //        FocusOnTop();
    //    }
    //    else
    //        rectTransform.anchoredPosition = new Vector2(0, posY + topYpos);
    //}

    public void ScrollToRow(int rowIndex)
    {
        float targetY = Mathf.Clamp(rowHeight * rowIndex, 0, Mathf.Max(0, rectTransform.rect.height - scrollRect.viewport.rect.height));
        float normalizedPosition = 1 - (targetY / (rectTransform.rect.height - scrollRect.viewport.rect.height));
        DOTween.To(() => scrollRect.verticalNormalizedPosition, x => scrollRect.verticalNormalizedPosition = x, normalizedPosition, scrollDuration).SetEase(scrollEase);
    }

    public void DestroyRows()
    {
        var allRows = GetComponentsInChildren<LeaderboardRow>();
        foreach (var row in allRows)
        {
            Destroy(row.gameObject);
        }
    }

    public void CreateRows(bool downloadDataOnRefresh = false)
    {
        DestroyRows();
        StartCoroutine(CreateLeaderboardRows(downloadDataOnRefresh));
    }

    public IEnumerator CreateLeaderboardRows(bool downloadDataOnRefresh = false)
    {
        //if (downloadDataOnRefresh)
        //{
        //    //yield return new WaitForSecondsRealtime(1f);
        //    //DreamloService.Instance.DownloadAll();


        //    yield return new WaitForSecondsRealtime(1f);
        //}

        yield return new WaitForSecondsRealtime(0f);

        var scores = LeaderboardService.Instance.Scores;
        var entries = scores.Results;
        var sortedEntries = entries.OrderBy(e => e.Rank).ToList();

        if (sortedEntries.Count == 0) yield break;

        rows = new();
        for (int i = 0; i < sortedEntries.Count; i++)
        {
            var userData = sortedEntries[i];

            var NewRowObject = Instantiate(LeaderboardRowPrefab, table.transform.position + offset, Quaternion.identity, table.transform);
            var newRow = NewRowObject.GetComponent<LeaderboardRow>();

            // Remove the random '#xxxx' that Unity Gaming Services appends
            var trimmedPlayerName = userData.PlayerName[..^5];
            newRow.userNameText.text = trimmedPlayerName;

            newRow.scoreText.text = Utils.FormatNumberWithCommas((long)sortedEntries[i].Score);
            newRow.rankText.text = (sortedEntries[i].Rank + 1).ToString();

            if (trimmedPlayerName == PlayerData.Instance.Data.PlayerName)
            {
                currentUserIndex = i;
                rowOfCurrentUser = newRow;
                rowOfCurrentUser.blinkingText.blink = true;
            }

            rows.Add(NewRowObject);
        }

        rowCount = rows.Count;
    }

}
