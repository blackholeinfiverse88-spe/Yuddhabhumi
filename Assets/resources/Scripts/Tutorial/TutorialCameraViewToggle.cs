using UnityEngine;
using System.Collections;

public class TutorialCameraViewToggle : MonoBehaviour
{
    [Header("XR Origin")]
    public Transform xrOrigin;   // XR Origin root

    [Header("Anchors (Above Surfaces)")]
    public Transform firstPersonAnchor;
    public Transform strategicViewAnchor;

    [Header("XR Systems")]
    public CharacterController characterController;
    public float groundSnapDelay = 0.05f;

    [Header("Tutorial Settings")]
    [Tooltip("Uncheck this so the player cannot use the POV button until we allow it.")]
    public bool isToggleAllowed = false;

    [Header("UI Elements")]
    [Tooltip("Drag the Canvas with your Strategic View instructions here.")]
    public GameObject strategicTextCanvas; // --- NEW: The floating text ---

    bool isStrategicView;
    bool isTransitioning;

    private void Start()
    {
        // Safety check: Ensure the text is hidden when the game starts
        if (strategicTextCanvas != null)
        {
            strategicTextCanvas.SetActive(false);
        }
    }

    public void UnlockPOVButton()
    {
        isToggleAllowed = true;
        Debug.Log("Tutorial: POV Button is now unlocked!");
    }

    // 🔘 CALL FROM UI BUTTON
    // 🔘 CALL FROM UI BUTTON
    public void ToggleCameraView()
    {
        if (!isToggleAllowed)
        {
            Debug.Log("Tutorial: POV switch blocked. Player isn't ready yet.");
            return;
        }

        Transform target = isStrategicView ? firstPersonAnchor : strategicViewAnchor;

        // 1. Disable, Move, and Re-enable in the EXACT SAME FRAME
        characterController.enabled = false;
        xrOrigin.position = target.position;
        xrOrigin.rotation = target.rotation;
        characterController.enabled = true;

        // 2. Toggle the state
        isStrategicView = !isStrategicView;

        // 3. Turn the floating text ON if we are in Strategic View, OFF if in First Person
        if (strategicTextCanvas != null)
        {
            strategicTextCanvas.SetActive(isStrategicView);
        }
    }

    
}