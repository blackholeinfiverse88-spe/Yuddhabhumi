using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class VRPauseMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenuUI;
    public GameObject settingsMenuUI;

    [Header("VR Setup")]
    public Transform playerCamera;       // Drag your Main Camera here
    public float spawnDistance = 1.0f;   // How far away it spawns
    public InputActionProperty menuButton; // The controller button to press

    private bool isPaused = false;

    // --- Enable Input ---
    private void OnEnable() => menuButton.action.Enable();
    private void OnDisable() => menuButton.action.Disable();

    void Start()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(false);
    }

    void Update()
    {
        // Listen for the controller button press
        if (menuButton.action.WasPressedThisFrame())
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
        Debug.Log("Pause Button Pressed");
    }

    public void PauseGame()
    {
        // 1. Teleport the menu exactly in front of the player's face
        if (playerCamera != null)
        {
            Vector3 targetPosition = playerCamera.position + (playerCamera.forward * spawnDistance);
            // Keep it at eye level (don't spawn it in the floor if they are looking down)
            targetPosition.y = playerCamera.position.y; 
            
            transform.position = targetPosition;
            
            // Make it face the player
            transform.LookAt(playerCamera);
            transform.Rotate(0, 180, 0); // Flip it so text isn't backwards
        }

        // 2. Show UI and Freeze Time
        pauseMenuUI.SetActive(true);
        settingsMenuUI.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OpenSettings()
    {
        settingsMenuUI.SetActive(true);
        pauseMenuUI.SetActive(false);
    }

    public void BackToPauseMenu()
    {
        settingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f; // Must unfreeze time before loading!
        
        // Use your global manager to load the Home scene safely
        if (GameFlowManager.Instance != null)
        {
            // Assuming your Home scene is called "Home" or "MainMenu"
            GameFlowManager.Instance.LoadScene("Home"); 
        }
        else
        {
            SceneManager.LoadScene("Home"); // Fallback
        }
    }
}   