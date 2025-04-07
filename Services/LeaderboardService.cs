using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using Newtonsoft.Json;


public class LeaderboardService : MonoBehaviour, IInitializable
{
    #region Singleton
    public static LeaderboardService Instance;
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

    public string Name {  get { return "Leaderboards"; } }
    const string LEADERBOARD_ID = "Leaderboard_1";

    public LeaderboardScoresPage Scores;
    public LeaderboardEntry PlayerScore;

    public int testScore;

    public IEnumerator Init()
    {

        yield return new WaitForSecondsRealtime(0);
    }

    public async Task FetchAll()
    {
        Scores = await GetPaginatedScoresAsync();
        PlayerScore = await GetPlayerScoreAsync();

        Debug.Log($"LeaderboardService: Got PlayerScore: {PlayerScore.PlayerName}/{PlayerScore.Score} and {Scores.Total} other scores");

    }

    public void StartLeaderboardFetchRetry()
    {
        InvokeRepeating(nameof(FetchLeaderboardWithRetry), 0, 2f);
    }

    void FetchLeaderboardWithRetry()
    {
        if (Authentication.Instance.IsSignedIn())
        {
            try
            {
                FetchAll();
            }
            finally
            {
                CancelInvoke(nameof(FetchLeaderboardWithRetry));
            }
        }
        else
        {
            Debug.Log("Unable to access Leaderboard bc not yet Authenticated.  Waiting to be Authenticated...");
        }
    }

    public async Task OnPlaySessionEnd()
    {
        await PutPlayerScoreAsync(PlayerData.Instance.Data.Score);
        print($"Finished PutPlayerScoreAsync at {Time.time}");

        await GetPaginatedScoresAsync();
        print($"Finished GetPaginatedScoresAsync at {Time.time}");
    }

    public async Task PutPlayerScoreAsync(int score)
    {
        try
        {
            var playerEntry = await LeaderboardsService.Instance.AddPlayerScoreAsync(LEADERBOARD_ID, score);
            Debug.Log($"LeaderboardService: PUT Player Score: {JsonConvert.SerializeObject(playerEntry)}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"LeaderboardService: Exception caught during PutPlayerScoreAsync: {ex}");
        }
    }

    public async Task<LeaderboardEntry> GetPlayerScoreAsync()
    {
        try
        {
            PlayerScore = await LeaderboardsService.Instance.GetPlayerScoreAsync(LEADERBOARD_ID);
            Debug.Log($"LeaderboardService: GET Player Score: {JsonConvert.SerializeObject(PlayerScore)}");
            return PlayerScore;
        }
        catch (Exception ex)
        {
            Debug.LogError($"LeaderboardService: Exception caught during GetPlayerScoreAsync: {ex}");
            return null;
        }

    }

    public async Task<LeaderboardScoresPage> GetPaginatedScoresAsync(int offset = 0, int limit = 100)
    {
        try
        {
            Scores = await LeaderboardsService.Instance.GetScoresAsync(LEADERBOARD_ID,
                new GetScoresOptions { Offset = offset, Limit = limit });
            Debug.Log($"LeaderboardService: GET {Scores.Results.Count} Paginated Scores");
            return Scores;
        }
        catch (Exception ex)
        {
            Debug.LogError($"LeaderboardService: Exception caught during GetPaginatedScoresAsync: {ex}");
            return null;
        }


    }

    public async Task<LeaderboardScores> GetPlayerRangeAsync(int rangeLimit = 5)
    {
        try
        {
            // Rangelimit = 5 returns a total of 11 entries (the given player plus 5 on either side)
            var scoresResponse = await LeaderboardsService.Instance.GetPlayerRangeAsync(LEADERBOARD_ID,
                new GetPlayerRangeOptions { RangeLimit = rangeLimit });
            Debug.Log($"LeaderboardService: GET {scoresResponse.Results.Count} Scores within +/- {rangeLimit} ranks of Player's rank");

            return scoresResponse;
        }
        catch (Exception ex)
        {
            Debug.LogError($"LeaderboardService: Exception caught during GetPlayerRangeAsync: {ex}");
            return null;
        }

    }

    //public async void GetScoresByPlayerIds(List<string> playerIds)
    //{
    //    var scoresResponse = await LeaderboardsService.Instance.GetScoresByPlayerIdsAsync(LEADERBOARD_ID, playerIds);
    //    Debug.Log($"LeaderboardService: GET {scoresResponse.Results.Count} Scores for {playerIds.Count} Player Id's: {JsonConvert.SerializeObject(scoresResponse)}");
    //}



}
