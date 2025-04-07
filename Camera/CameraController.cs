using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.Rendering;
using Unity.VisualScripting;

public class CameraController : MonoBehaviour
{
    #region Singleton
    public static CameraController Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    #endregion

    public CinemachineVirtualCamera cam;
    public CinemachineFramingTransposer transposer;

    [ReadOnly] public Vector3 cameraPosition;

    public CameraAnchor startingCameraAnchor;
    [SerializeField] private Transform followTargetRef;

    [Header("Movement Settings")]
    public bool moveCameraWithKeyboard;
    public float moveSpeed = 150f;

    private void Start()
    {
        if (!cam) cam = FindObjectOfType<CinemachineVirtualCamera>();
        if (!transposer) transposer = cam.GetCinemachineComponent<CinemachineFramingTransposer>();

        // Set the camera's follow target to the starting anchor, if available.
        if (startingCameraAnchor)
            SetCameraFollow(startingCameraAnchor.transform);
    }

    private void Update()
    {
        if (moveCameraWithKeyboard)
        {
            ProcessMovementInput();
        }

    }

    /// <summary>
    /// Processes keyboard input to move the camera's follow target.
    /// </summary>
    private void ProcessMovementInput()
    {
        if (GameManager.Instance.inputSuspended) return;

        // Determine the input direction based on the keys configured in InputManager.
        Vector2 inputDir = Vector2.zero;
        if (Input.GetKey(InputManager.Instance.upKey) || Input.GetKey(KeyCode.UpArrow))
            inputDir.y += 1;
        if (Input.GetKey(InputManager.Instance.downKey) || Input.GetKey(KeyCode.DownArrow))
            inputDir.y -= 1;
        if (Input.GetKey(InputManager.Instance.rightKey) || Input.GetKey(KeyCode.RightArrow))
            inputDir.x += 1;
        if (Input.GetKey(InputManager.Instance.leftKey) || Input.GetKey(KeyCode.LeftArrow))
            inputDir.x -= 1;

        inputDir = inputDir.normalized;

        // Move the follow target if it exists; otherwise, move the camera directly.
        Vector3 movement = new Vector3(inputDir.x, inputDir.y, 0f) * moveSpeed * Time.deltaTime;
        if (followTargetRef != null)
        {
            followTargetRef.position += movement;
        }
        else if (cam != null)
        {
            cam.transform.position += movement;
        }
    }

    /// <summary>
    /// Sets the camera's follow target.
    /// </summary>
    public void SetCameraFollow(Transform transformToFollow)
    {
        cam.Follow = transformToFollow;
        followTargetRef = transformToFollow;
    }

    /// <summary>
    /// Smoothly moves either the follow target or the camera to the target position.
    /// </summary>
    public void SmoothFollowToPosition(Vector3 position, float duration = 0, Ease ease = Ease.InOutCubic)
    {
        if (cam.Follow != null)
        {
            followTargetRef.transform.DOMove(position, duration).SetEase(ease);
        }
        else
        {
            cam.transform.DOMove(position, duration).SetEase(ease);
        }
    }

    private void LateUpdate()
    {
        if (cam)
            cameraPosition = cam.transform.position;
    }
}
