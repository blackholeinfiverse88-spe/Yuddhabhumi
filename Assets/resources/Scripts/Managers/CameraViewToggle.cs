using UnityEngine;
using System.Collections;

public class CameraViewToggle : MonoBehaviour
{
    [Header("XR Origin")]
    public Transform xrOrigin;   // XR Origin root (not the Camera)

    [Header("Anchors (Above Surfaces)")]
    public Transform firstPersonAnchor;
    public Transform strategicViewAnchor;

    [Header("XR Systems")]
    public CharacterController characterController;
    public float groundSnapDelay = 0.05f;

    bool isStrategicView;
    bool isTransitioning;

    // 🔘 CALL FROM UI BUTTON
    public void ToggleCameraView()
    {
        if (isTransitioning)
            return;

        Transform target = isStrategicView
            ? firstPersonAnchor
            : strategicViewAnchor;

        StartCoroutine(MoveAndSnap(target));

        isStrategicView = !isStrategicView;
    }

    IEnumerator MoveAndSnap(Transform anchor)
    {
        isTransitioning = true;

        // 1️⃣ Disable CharacterController to allow reposition
        characterController.enabled = false;

        // 2️⃣ Move XR Origin above target surface
        xrOrigin.position = anchor.position;
        xrOrigin.rotation = anchor.rotation;

        // 3️⃣ Small delay so XR systems update
        yield return new WaitForSeconds(groundSnapDelay);

        // 4️⃣ Re-enable controller → XR snaps to surface
        characterController.enabled = true;

        isTransitioning = false;
    }
}
