using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;


/// <summary>
/// Singleton.  Saves and loads PlayerData to/from the Cloud using Unity Cloud Save.
/// </summary>
public class PlayerData : MonoBehaviour
{
    #region Singleton
    public static PlayerData Instance;
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

    public LocalPlayerData Data;
    private List<ItemKey> itemKeys;

    // Keys    
    string PlayerName = nameof(PlayerName);
    string Score = nameof(Score);
    string BestScore = nameof(BestScore);
    string LevelsCompletedThisRun = nameof(LevelsCompletedThisRun);
    string BestLevelsCompleted = nameof(BestLevelsCompleted);
    string Replays = nameof(Replays);
    string TotalGameTime = nameof(TotalGameTime);


    public async Task Init()
    {
        itemKeys = await CloudSaveService.Instance.Data.Player.ListAllKeysAsync();

        if (itemKeys.Count > 0)
        {
            var loadedData = await LoadAllAsync();
            SetLocalPlayerData(loadedData);
        }
        else
        {
            print($"No existing keys for PlayerData found in Cloud.  Creating new PlayerData keys and setting to Defaults.");
            await Data.ResetAllToDefaults();
            await SaveAllAsync();
        }
    }

    //public Sprite GetPlayerAvatar()
    //{
    //    return playerAvatars[Data.PlayerAvatarIndex];
    //}

    void SetLocalPlayerData(Dictionary<string, Item> data)
    {
        Data.PlayerName = GetStringFromData(PlayerName, data);
        Data.Replays = GetIntFromData(Replays, data);
        Data.TotalGameTime = GetFloatFromData(Replays, data);

        Data.Score = GetIntFromData(Score, data);
        Data.BestScore = GetIntFromData(BestScore, data);

        Data.LevelsCompletedThisRun = GetIntFromData(LevelsCompletedThisRun, data);
        Data.BestLevelsCompleted = GetIntFromData(BestLevelsCompleted, data);

        if (GameManager.Instance.cloudLogging) print("PlayerData: Local PlayerData has been set");
    }

    // Load all items from Cloud for the logged-in Player
    public async Task<Dictionary<string, Item>> LoadAllAsync()
    {
        try
        {
            return await CloudSaveService.Instance.Data.Player.LoadAllAsync();
        }
        catch (Exception ex)
        {
            Debug.Log($"PlayerData: Error loading all Player Data from Cloud: {ex}");
            return null;
        }

    }

    // Delete an item in Cloud by Key
    public async Task DeleteAsync(string key)
    {
        await CloudSaveService.Instance.Data.Player.DeleteAsync(key);
    }

    public async Task DeleteAllAsync()
    {
        itemKeys = await CloudSaveService.Instance.Data.Player.ListAllKeysAsync();
        foreach (var key in itemKeys)
        { 
            await DeleteAsync(key.Key);
        }
    }

    public async Task SaveAllAsync()
    {
        try
        {
            var saveData = new Dictionary<string, object>
            {
                { PlayerName, Data.PlayerName },
                { Replays, Data.Replays },
                { TotalGameTime, Data.TotalGameTime },

                { Score, Data.Score },
                { BestScore, Data.BestScore },

                { LevelsCompletedThisRun, Data.LevelsCompletedThisRun },
                { BestLevelsCompleted, Data.BestLevelsCompleted },
            };
            await CloudSaveService.Instance.Data.Player.SaveAsync(saveData);
            //SavePlayerNameToPlayerPrefs(Data.PlayerName);

            if (GameManager.Instance.cloudLogging) print("PlayerData: All Player Data saved to Cloud.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"An exception occurred while saving Player Data: {ex}");
        }
    }

    public async Task ClearAllPlayerData()
    {
        await Data.ResetAllToDefaults();
    }


    #region Shortcut Methods

    //public async Task SaveLevelAndXPAsync()
    //{
    //    try
    //    {
    //        var saveData = new Dictionary<string, object> 
    //        { 
    //            //{ XP, Data.XP }, 
    //            //{ TotalXP, Data.TotalXP },
    //            //{ PlayerLevel, Data.PlayerLevel },

    //            { BestRound, Data.BestRound },
    //            { BestTime, Data.BestTime }
    //        };
    //        await CloudSaveService.Instance.Data.Player.SaveAsync(saveData);
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError($"An exception occurred while saving PlayerLevel and XP: {ex}");
    //    }
    //}

    //public async Task SavePlayerName()
    //{
    //    try
    //    {
    //        var saveData = new Dictionary<string, object> { { PlayerName, Data.PlayerName } };
    //        await CloudSaveService.Instance.Data.Player.SaveAsync(saveData);
    //        SavePlayerNameToPlayerPrefs(Data.PlayerName);
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError($"An exception occurred while saving PlayerName: {ex}");
    //    }
    //}

    //public void SavePlayerNameToPlayerPrefs(string playerName)
    //{
    //    PlayerPrefs.SetString(PlayerName, playerName);
    //    PlayerPrefs.Save();
    //}

    //public string LoadPlayerNameFromPlayerPrefs()
    //{
    //    if (PlayerPrefs.HasKey(PlayerName))
    //    {
    //        return PlayerPrefs.GetString(PlayerName);
    //    }

    //    return string.Empty;
    //}


    #endregion

    #region Helper Methods
    float GetFloatFromData(string key, Dictionary<string, Item> data)
    {
        return data[key].Value.GetAs<float>();
    }

    string GetStringFromData(string key, Dictionary<string, Item> data)
    {
        return data[key].Value.GetAsString();
    }

    int GetIntFromData(string key, Dictionary<string, Item> data)
    {
        return data[key].Value.GetAs<int>();
    }
    #endregion
}
