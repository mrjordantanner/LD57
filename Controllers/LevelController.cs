using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Video;


public class LevelController : MonoBehaviour
{
    #region Singleton
    public static LevelController Instance;
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

    [Header("Current State")]
    [ReadOnly] public bool isLevelActive;
    [ReadOnly] public int CurrentLayer = 1;
    [ReadOnly] public int DiveCost;
    [ReadOnly] public float LevelTimerDuration;
    [ReadOnly] public int ChargesToSpawn;
    [ReadOnly] public float timeRemaining;

    [Header("Base Settings")]
    public int StartingDiveCost = 10;
    public float StartingTimePerLayer = 30f;
    public int StartingCharges = 100;
    public float MinTimePerLayer = 10f;

    [Header("Scaling Settings")]
    public int DiveCostIncrement = 5;
    public float TimeReductionPerLayer = 2f;
    public float ChargeSpawnDecayRate = 0.9f;

    [ReadOnly] public int NextDiveCost;
    [ReadOnly] public float NextLevelTimerDuration;

    [Header("Pickups")]
    GameObject[] PickupClusterPrefabs;
    public float pickupSpawnRadius = 50f;
    public int maxPickupClustersPerLayer = 20;
    public GameObject ChargesContainer;
    public float spawnOffsetZ = 5f;

    private void Start()
    {
        PickupClusterPrefabs = Resources.LoadAll<GameObject>("Pickups");
        Debug.Log($"Loaded {PickupClusterPrefabs.Length} Pickup cluster prefabs");

        Init();
    }

    public void Init()
    {
        DestroyAllPickups();
        CurrentLayer = 1;
        CalculateLevelValues(false);
    }

    public void DestroyAllPickups()
    {
        var pickups = FindObjectsOfType<Pickup>();
        if (pickups.Length < 0)
        {
            foreach (var pickup in pickups)
            {
                Destroy(pickup.gameObject);
            }
        }

    }

    private void Update()
    {
        if (GameManager.Instance.gameRunning && !GameManager.Instance.gamePaused && !LayerController.Instance.isShifting)
        {
            HandleLevelTimer();
        }
    }

    public float warningTime = 8f;

    bool hasBeenWarned;
    private void HandleLevelTimer()
    {
        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= warningTime && !hasBeenWarned)
        {
            HUD.Instance.ShowAlertMessage("Press Space/RMB to Dive to next level before time expires!" , 0.15f, 2f, 1f);
            hasBeenWarned = true;
        }

        if (timeRemaining <= 0)
        {
            StartCoroutine(GameManager.Instance.GameOver());
        }
    }

    public void NextLevel(bool wasPenalized)
    {
        hasBeenWarned = false;
        CurrentLayer++;
        CalculateLevelValues(wasPenalized);
        //DebugLogValues();
    }

    public void CalculateLevelValues(bool wasPenalized)
    {
        DiveCost = StartingDiveCost + DiveCostIncrement * (CurrentLayer - 1);
        ChargesToSpawn = Mathf.FloorToInt(StartingCharges * Mathf.Pow(ChargeSpawnDecayRate, CurrentLayer - 1));

        if (wasPenalized)
        {
            // Apply a harsher time penalty
            LevelTimerDuration = Mathf.Max(MinTimePerLayer,
                (StartingTimePerLayer - TimeReductionPerLayer * (CurrentLayer - 1)) * 0.7f);

            // Reduce collectible spawn by 20% (rounded down)
            ChargesToSpawn = Mathf.FloorToInt(ChargesToSpawn * 0.8f);
        }
        else
        {
            LevelTimerDuration = Mathf.Max(MinTimePerLayer,
                StartingTimePerLayer - TimeReductionPerLayer * (CurrentLayer - 1));
        }

        timeRemaining = LevelTimerDuration;

        // Calculate next layer preview
        int nextLayerIndex = CurrentLayer; // next = current + 1, but formula uses (n - 1)
        NextDiveCost = StartingDiveCost + DiveCostIncrement * nextLayerIndex;
        NextLevelTimerDuration = Mathf.Max(MinTimePerLayer,
            StartingTimePerLayer - TimeReductionPerLayer * nextLayerIndex);

        HUD.Instance.UpdateChargesUI();
        DebugLogValues();
    }


    public void SpawnCollectiblesOnLayer(Layer layer)
    {
        layer.PickupClusters = new List<GameObject>();
        for (int i = 0; i < ChargesToSpawn; i++)
        {
            GameObject Prefab = PickupClusterPrefabs[Random.Range(0, PickupClusterPrefabs.Length)];

            Vector2 offset2D = Random.insideUnitCircle * pickupSpawnRadius;
            Vector3 spawnPos = new Vector3(
                 layer.transform.position.x + offset2D.x,
                 layer.transform.position.y + offset2D.y,
                 layer.transform.position.z + spawnOffsetZ
            );

            var newCluster = Instantiate(Prefab, spawnPos, Quaternion.identity, ChargesContainer.transform);
            layer.PickupClusters.Add(newCluster);
        }

        Debug.Log($"Spawned {ChargesToSpawn} Charge clusters on Layer {layer.gameObject.name} at position {layer.transform.position}");
    }


    public void DebugLogValues()
    {
        Debug.Log($"--- Layer {CurrentLayer} ---");
        Debug.Log($"Dive Cost: {DiveCost}");
        Debug.Log($"Time Remaining: {LevelTimerDuration} seconds");
        Debug.Log($"Charges To Spawn: {ChargesToSpawn}");
    }
}
