using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialMenuController : MonoBehaviour
{
    [Header("Settings")]
    public GameObject canvasToToggle;
    
    [Header("Input")]
    public InputActionProperty toggleButton; 

    [Header("Tutorial Safety Lock")]
    [Tooltip("Leave this UNCHECKED so the player can't open the menu early!")]
    public bool isMenuAllowed = false; // <-- Starts completely locked!

    // --- Call this from your Section 2 Doorway Trigger ---
    public void UnlockMenu()
    {
        isMenuAllowed = true;
        Debug.Log("Tutorial: HUD Menu is now unlocked!");
    }

    private void OnEnable()
    {
        toggleButton.action.Enable();
    }

    private void OnDisable()
    {
        toggleButton.action.Disable();
    }

    void Update()
    {
        // --- THE BOUNCER: If the lock is on, ignore the button press entirely! ---
        if (!isMenuAllowed) return; 

        if (toggleButton.action.WasPressedThisFrame())
        {
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