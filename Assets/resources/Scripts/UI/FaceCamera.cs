using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [Header("Distance & Height")]
    public float distanceFromFace = 1.2f;     // Distance in front of camera
    public float heightOffset = -0.1f;        // Slightly below eye level
    public float smoothSpeed = 5f;             // Follow smoothness

    [Header("Offsets")]
    public float horizontalOffset = -0.25f;   // Negative = left, Positive = right
    public float yawOffset = 10f;              // Rotate toward right (degrees)

    [Header("Behavior")]
    public bool lockYAxis = true;              // Keep UI upright
    public bool followContinuously = true;     // Follow every frame

    [Header("Rotation Offsets")]
    public float pitchOffset = 0f;   // X axis (up/down)


    private void LateUpdate()
    {
        if (Camera.main == null) return;

        Transform cam = Camera.main.transform;

        // -------- POSITION --------
        Vector3 forward = cam.forward;
        Vector3 right = cam.right;

        if (lockYAxis)
        {
            forward.y = 0;
            right.y = 0;
        }

        forward.Normalize();
        right.Normalize();

        Vector3 targetPos =
            cam.position +
            forward * distanceFromFace +
            right * horizontalOffset;

        targetPos.y += heightOffset;

        if (followContinuously)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPos,
                Time.deltaTime * smoothSpeed
            );
        }
        else
        {
            transform.position = targetPos;
        }

        // -------- ROTATION --------
        Vector3 lookDir = cam.forward;
        if (lockYAxis) lookDir.y = 0;

        Quaternion targetRot = Quaternion.LookRotation(lookDir);
        targetRot *= Quaternion.Euler(pitchOffset, yawOffset, 0f);

        // Rotate slightly toward right
        targetRot *= Quaternion.Euler(0f, yawOffset, 0f);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * smoothSpeed
        );
    }
}
