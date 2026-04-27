using UnityEngine;
using UnityEngine.InputSystem;

public class VRMenuController : MonoBehaviour
{
    [Header("Settings")]
    public GameObject canvasToToggle;
    
    [Header("Input")]
    public InputActionProperty toggleButton; 

    // --- NEW: TURN ON THE LISTENER ---
    private void OnEnable()
    {
        toggleButton.action.Enable();
    }

    private void OnDisable()
    {
        toggleButton.action.Disable();
    }
    // ---------------------------------

    void Update()
    {
        if (toggleButton.action.WasPressedThisFrame())
        {
            Debug.Log("Button Pressed!"); // Debug to prove it works
            bool isActive = !canvasToToggle.activeSelf;
            canvasToToggle.SetActive(isActive);
            
            // Re-enable FaceCamera if opening
            if (isActive)
            {
                FaceCamera faceCam = canvasToToggle.GetComponent<FaceCamera>();
                if (faceCam) faceCam.enabled = true; 
            }
        }
    }
}