using UnityEngine;

public class TopDownCharacterController : MonoBehaviour
{
    [ReadOnly] public Vector2 direction;
    [HideInInspector] public Rigidbody rb;
    public bool isMoving;

    [Header("Mouse Movement")]
    public float deadZoneRadius = 0.5f;
    public Camera mainCamera;
    public bool useMouseMovement = true;

    [Header("Smoothing")]
    public float accelerationTime = 0.2f;

    private Vector3 mouseWorldTarget;
    private Vector3 currentVelocity = Vector3.zero;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (PlayerManager.Instance.State == PlayerState.Dead ||
            PlayerManager.Instance.State == PlayerState.Hurt ||
            GameManager.Instance.gamePaused ||
            !PlayerManager.Instance.canMove)
        {
            direction = Vector2.zero;
            isMoving = false;
            return;
        }

        Vector2 keyboardInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (keyboardInput.sqrMagnitude > 0.01f)
        {
            direction = keyboardInput.normalized;
            isMoving = true;
        }
        else if (useMouseMovement && Input.GetMouseButton(0))
        {
            Vector3? mouseTarget = GetMouseWorldPosition();

            if (mouseTarget.HasValue)
            {
                mouseWorldTarget = mouseTarget.Value;

                Vector3 toMouse = mouseWorldTarget - transform.position;
                toMouse.z = 0f; // ? Z is locked, movement is X/Y only

                float distance = toMouse.magnitude;

                if (distance > deadZoneRadius)
                {
                    direction = new Vector2(toMouse.x, toMouse.y).normalized;
                    isMoving = true;
                }
                else
                {
                    direction = Vector2.zero;
                    isMoving = false;
                }
            }
        }
        else
        {
            direction = Vector2.zero;
            isMoving = false;
        }
    }

    private void FixedUpdate()
    {
        SmoothMove();
    }

    private void SmoothMove()
    {
        // Move only on X and Y, keep Z fixed
        Vector3 currentPosition = transform.position;
        Vector3 desiredVelocity = new Vector3(direction.x, direction.y, 0f) * PlayerManager.Instance.MoveSpeed;

        Vector3 smoothVelocity = Vector3.SmoothDamp(rb.velocity, desiredVelocity, ref currentVelocity, accelerationTime);

        rb.velocity = new Vector3(smoothVelocity.x, smoothVelocity.y, 0f); // lock Z
    }

    /// <summary>
    /// Casts mouse ray onto the X/Y plane (Z = transform.position.z)
    /// </summary>
    private Vector3? GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Project onto plane at current Z height (since Z is locked)
        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, transform.position.z));

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return null;
    }
}
