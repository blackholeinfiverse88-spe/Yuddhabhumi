using UnityEngine;

public class VRUIFollower : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Drag your XR Rig's Main Camera here.")]
    public Transform vrCamera; 

    [Header("Follow Settings")]
    public float distanceFromFace = 2.0f; // How far away it floats
    public float heightOffset = -0.2f;    // Negative number puts it slightly below eye level
    public float followSpeed = 5.0f;      // Lower number = lazier/slower follow

    void LateUpdate()
    {
        if (vrCamera == null) return;

        // 1. Get the direction the camera is facing, but flatten it so the UI doesn't tilt into the floor
        Vector3 flatForward = vrCamera.forward;
        flatForward.y = 0;
        
        // Safety check: if the player looks perfectly straight down, don't glitch out
        if (flatForward.sqrMagnitude > 0.01f)
        {
            flatForward.Normalize();
        }
        else
        {
            flatForward = transform.forward; 
        }

        // 2. Calculate exactly where the UI SHOULD be
        Vector3 targetPosition = vrCamera.position + (flatForward * distanceFromFace);
        targetPosition.y = vrCamera.position.y + heightOffset;

        // 3. Smoothly glide to that position (The "Lazy" part)
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        // 4. Smoothly rotate to face the player
        Quaternion targetRotation = Quaternion.LookRotation(flatForward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * followSpeed);
    }
}