using UnityEngine;

public class SmartCanvasRotator : MonoBehaviour
{
    [Header("Targeting")]
    [Tooltip("CRITICAL: Drag the 'Main Camera' (your eyes), NOT the XR Origin (your feet)!")]
    public Transform playerCamera;

    [Header("Settings")]
    public float activationDistance = 6.0f;
    public float rotationSpeed = 4.0f;

    [Header("Rotation Style")]
    [Tooltip("If TRUE, it tilts up/down. If FALSE, it stays perfectly level.")]
    public bool allowTilting = true; 

    void LateUpdate()
    {
        if (playerCamera == null) return;

        // Are we close enough?
        if (Vector3.Distance(transform.position, playerCamera.position) <= activationDistance)
        {
            // 1. Pinpoint exactly where the player's eyes are
            Vector3 targetPoint = playerCamera.position;

            // 2. If tilting is off, we trick the canvas into thinking your eyes 
            // are at the exact same height as the canvas itself.
            if (!allowTilting)
            {
                targetPoint.y = transform.position.y;
            }

            // 3. Find the line pointing from the canvas to the target
            // (We subtract target from position because UI Canvases face backwards by default!)
            Vector3 directionToFace = transform.position - targetPoint;

            if (directionToFace.sqrMagnitude > 0.001f)
            {
                // 4. Calculate the perfect rotation, using Vector3.up to ensure it doesn't roll or twist
                Quaternion targetRot = Quaternion.LookRotation(directionToFace, Vector3.up);

                // 5. Apply the rotation smoothly
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
            }
        }
    }
}