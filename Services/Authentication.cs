using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using System.Collections;

public class Authentication : MonoBehaviour, IInitializable
{
    #region Singleton
    public static Authentication Instance;
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

    public string Name { get { return "Authenticating..."; } }

    public string testPlayerName;

    [ReadOnly] public string playerName;
    [ReadOnly] public string playerId;
    [ReadOnly] public string accessToken;

    public IEnumerator Init()
    {
        yield return InitAsync();
    }

    async Task InitAsync()
    {
        await UnityServices.InitializeAsync();
        await SignInAnonymously();

        await PlayerData.Instance.Init();
    }

    public bool IsSignedIn()
    {
        return AuthenticationService.Instance.IsSignedIn;
    }

    private async Task SignInAnonymously()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            playerId = AuthenticationService.Instance.PlayerId;
            accessToken = AuthenticationService.Instance.AccessToken;

            Debug.Log($"Signed in as PlayerId: {playerId}");

            AuthenticationService.Instance.SignInFailed += (err) => {
                Debug.LogError(err);
            };

            AuthenticationService.Instance.SignedOut += () => {
                Debug.Log("Player signed out.");
            };

            AuthenticationService.Instance.Expired += () =>
            {
                Debug.Log("Player session could not be refreshed and expired.");
            };
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
       
    }

    public async Task<string> GetPlayerNameAsync()
    {
        var playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
        print($"AuthenticationService: Got PlayerName: {playerName}");
        PlayerData.Instance.Data.PlayerName = name;
        return playerName;
    }

    public async Task UpdatePlayerNameAsync(string name)
    {
        var success = await AuthenticationService.Instance.UpdatePlayerNameAsync(name);
        print($"updateName: {success}");
        print($"AuthenticationService: Updated PlayerName to {name}");
        PlayerData.Instance.Data.PlayerName = name;
    }

}
