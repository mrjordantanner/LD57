using System.Collections;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.Services.CloudSave.Models.Data.Player;
using UnityEngine.Rendering;
using static Pickup;

public enum PlayerState { Idle, Walking, Jumping, Shooting, Hurt, Dead }

public class PlayerManager : MonoBehaviour, IInitializable
{
    #region Singleton
    public static PlayerManager Instance;
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

    public string Name { get { return "Player Manager"; } }

    [ReadOnly] public PlayerState State;

    #region Declarations
    public bool useDpad, useAnalogStick;

    [Header("Player Game Data")]
    public float currentHealth, MaxHealth = 1;

    public float DamageCooldownDuration = 3;
    public float MoveSpeed = 5;
    public int currentLives, startingLives = 3;

    [Header("Charges")]
    public int totalChargesThisRun;
    public int currentCharges;

    [Header("Clickable References")]
    public GameObject PlayerPrefab;
    public PlayerCharacter player;
    public GameObject PlayerGraphicsRef;
    public Transform playerSpawnPoint;
    public GameObject PlayerDeathVFX;

    [Header("Movement")]
    public float baseMoveSpeed = 200f;
    public float speedBoostAmount = 15f;
    public float bonusTime = 6f;
    public float maxMoveSpeed = 400f;

    private List<Coroutine> activeBoosts = new List<Coroutine>();

    public float damageRadius = 2f;
    public float respawnTime = 3f;

    [Header("Input")]
    [ReadOnly] public Vector2 directionalInput;
    [ReadOnly] public float horiz;
    [ReadOnly] public float vert;

    [Header("States")]
    public bool invulnerable;
    public bool canMove = true;
    [ReadOnly]
    public bool
        isMoving,
        facingRight,
        masterInvulnerability;

    #endregion

    public IEnumerator Init()
    {
        SetInitialState();
        yield return new WaitForSecondsRealtime(0);
    }

    public void UpdatePlayerRef(PlayerCharacter newPlayer)
    {
        player = newPlayer;
        print("PlayerRef updated");
    }

    void Update()
    {
        if (!GameManager.Instance.gameRunning) return;

        // Select Player Object with KeyPad 0
        //if (UnityEditor.EditorApplication.isPlaying && player != null && player.gameObject != null)
        //{
        //    if (Input.GetKeyDown(KeyCode.Keypad0)) UnityEditor.Selection.activeGameObject = player.gameObject;
        //}

        // Handle Input
        if (!GameManager.Instance.inputSuspended && !GameManager.Instance.gamePaused && State != PlayerState.Hurt)
        {
            HandleInput();
            HandleGamepadInput();
        }

    }

    public void AddSpeedBoost()
    {
        if (MoveSpeed < maxMoveSpeed)
        {
            MoveSpeed = Mathf.Min(MoveSpeed + speedBoostAmount, maxMoveSpeed);
            Coroutine boost = StartCoroutine(RemoveSpeedBoostAfterDelay(bonusTime));
            activeBoosts.Add(boost);
        }
    }

    private IEnumerator RemoveSpeedBoostAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        MoveSpeed = Mathf.Max(baseMoveSpeed, MoveSpeed - speedBoostAmount);
    }

    public void SetInitialState()
    {
        //RefillHealth();

        //HUD.Instance.UpdateHealthbar(true, false);

        State = PlayerState.Idle;
        canMove = true;
        facingRight = false;
        invulnerable = false;

        //player.trails.enabled = true;
        //player.trails.on = true;
    }

    public void SpawnPlayer()
    {
        DespawnPlayer();

        //var spawnPosition = LevelController.Instance.CurrentLevel.playerSpawnPoint.transform.position;
        var spawnPosition = LayerController.Instance.activeLayerAnchor.position;
        var PlayerObject = Instantiate(PlayerPrefab, spawnPosition, Quaternion.identity);
        PlayerObject.name = "Player";
        PlayerGraphicsRef = player.PlayerGraphics;
        UpdatePlayerRef(PlayerObject.GetComponent<PlayerCharacter>());

        SetInitialState();
        //StartCoroutine(DamageCooldown());

        CameraController.Instance.SetCameraFollow(PlayerObject.transform);
    }

    public void DespawnPlayer()
    {
        var existingPlayer = FindObjectOfType<PlayerCharacter>();
        if (existingPlayer != null)
        {
            Destroy(existingPlayer.transform.gameObject);
        }

        CameraController.Instance.SetCameraFollow(null);
    }

    public void HandleGamepadInput()
    {
        // Dpad 
        if (useDpad)
        {
            horiz = Input.GetAxisRaw("DpadHoriz");
            vert = Input.GetAxisRaw("DpadVert");
        }
        // Analog Stick
        if (useAnalogStick)
        {
            horiz = Input.GetAxisRaw("Horizontal");
            vert = Input.GetAxisRaw("Vertical");
        }

        directionalInput = new(horiz, vert);
    }

    public void HandleInput()
    {
        if (!player
            || GameManager.Instance.inputSuspended
            || State == PlayerState.Hurt || State == PlayerState.Dead) return;

        // Get keyboard horiz input
        if (Input.GetKey(InputManager.Instance.downKey))  vert = -1;
        else if (Input.GetKey(InputManager.Instance.upKey)) vert = 1;
        else vert = 0;

        // Get keyboard vert input
        if (Input.GetKey(InputManager.Instance.leftKey)) horiz = -1;
        else if (Input.GetKey(InputManager.Instance.rightKey)) horiz = 1;
        else horiz = 0;


        // Dive / Shift Layers with Spacebar / RMB
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse1)) && !LayerController.Instance.isShifting)
        {
            var wasPenalized = SpendCharges();
            StartCoroutine(StartLayerShift(wasPenalized));
            AudioManager.Instance.soundBank.LayerShift.Play();
        }
    }

    IEnumerator StartLayerShift(bool wasPenalized)
    {
        yield return StartCoroutine(LayerController.Instance.ShiftLayers());
        LevelController.Instance.NextLevel(wasPenalized);

    }

    public void CollectPickup(Pickup pickup)
    {
        pickup.OnTargetHit();

        switch (pickup.type)
        {
            case PickupType.Charge:
                GainCharges(1);
                break;


            case PickupType.Powerup:
                break;

            default:
                break;
        }

    }

    public void GainCharges(int amount)
    {
        PlayerManager.Instance.AddSpeedBoost();
        StartCoroutine(HUD.Instance.TextPop(HUD.Instance.currentCoinsLabel));
        AudioManager.Instance.soundBank.CollectPickup.Play();
        currentCharges += amount;
        totalChargesThisRun += amount;
        HUD.Instance.UpdateChargesUI();
    }

    public void ResetCurrentCharges()
    {
        currentCharges = 0;
        HUD.Instance.UpdateChargesUI();
    }

    public bool SpendCharges()
    {
        StartCoroutine(HUD.Instance.TextPop(HUD.Instance.currentCoinsLabel));
        StartCoroutine(HUD.Instance.TextPop(HUD.Instance.diveCostLabel));
        bool wasPenalized = false;
        currentCharges -= LevelController.Instance.DiveCost;
        if (currentCharges < 0)
        {
            currentCharges = 0;
            HUD.Instance.ShowAlertMessage("Not enough Charges to Dive - Timer reduced!");
            wasPenalized = true;
        }
        HUD.Instance.UpdateChargesUI();
        return wasPenalized;
    }
}


