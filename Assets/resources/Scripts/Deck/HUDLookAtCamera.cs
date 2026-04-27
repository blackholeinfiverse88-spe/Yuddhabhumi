using UnityEngine;

public class VRHUDController : MonoBehaviour
{
    public Transform playerCamera;
    public Vector3 targetPosition;
    public bool isDragging;

    void LateUpdate()
    {
        if (!playerCamera) return;

        // Smooth move
        if (isDragging)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.deltaTime * 15f
            );
        }

        // Face camera (no flip)
        Vector3 dir = playerCamera.position - transform.position;
        dir.y = 0f;
        transform.rotation = Quaternion.LookRotation(-dir);
    }
}
