using System.Threading.Tasks;
using UnityEngine;


[CreateAssetMenu(menuName = "Data/Player Data")]
public class LocalPlayerData : ScriptableObject
{
    public string defaultPlayerName = "Player";

    [ReadOnly] public string PlayerName;
    [ReadOnly] public int Replays;
    [ReadOnly] public float TotalGameTime;

    [ReadOnly] public int Score, BestScore;
    [ReadOnly] public int LevelsCompletedThisRun, BestLevelsCompleted;

    [ReadOnly] public bool IsNewHighScore;
    [ReadOnly] public bool IsNewBestLevel;

    public void ResetGameSessionData()
    {
        IsNewHighScore = IsNewBestLevel = false;
        Score = LevelsCompletedThisRun = 0;
    }

    public async Task ResetAllToDefaults()
    {
        ResetGameSessionData();

        PlayerName = defaultPlayerName;
        BestScore = BestLevelsCompleted = Replays = 0;
        TotalGameTime = 0;

        await PlayerData.Instance.SaveAllAsync();
    }

}
