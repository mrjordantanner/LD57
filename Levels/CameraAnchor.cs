using UnityEngine;


public class CameraAnchor : MonoBehaviour
{
    [Header("Gizmos")]
    public float gizmoSize = 0.5f;
    public Color gizmoColor = Color.blue;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        // Draw the diagonals to form an "X"
        Gizmos.DrawLine(transform.position + new Vector3(-1, 1, 0) * gizmoSize,
                        transform.position + new Vector3(1, -1, 0) * gizmoSize);

        Gizmos.DrawLine(transform.position + new Vector3(-1, -1, 0) * gizmoSize,
                        transform.position + new Vector3(1, 1, 0) * gizmoSize);
    }

}