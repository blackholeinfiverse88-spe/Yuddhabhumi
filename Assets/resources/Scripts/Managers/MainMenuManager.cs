using UnityEngine;
using TMPro; // For Text Mesh Pro

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Text (Optional)")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI statsText;
    public GameObject settingsPanel;

    private void Start()
    {
        // If you created the UserProfile script earlier, we update the text here.
        if (UserProfile.Instance != null)
        {
            if(levelText) levelText.text = "Lvl " + UserProfile.Instance.currentLevel;
            if(statsText) statsText.text = $"Wins: {UserProfile.Instance.wins}";
        }
        Application.targetFrameRate=72;
    }

    // Connect this to your PLAY Button
    public void OnClickPlay()
    {
        // Uses your GameFlowManager to fade out and load the next scene.
        // According to the plan, this goes to Deck Builder.
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.LoadScene("DeckBuilder");
        }
        else
        {
            // Fallback if you started directly in this scene without Boot
            UnityEngine.SceneManagement.SceneManager.LoadScene("DeckBuilder");
        }
    }

    // Connect this to your QUIT Button
    public void OnClickQuit()
    {
        Debug.Log("Quitting Game...");

        // 1. If we are running in the Unity Editor, stop playing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 2. If we are in the built game (VR Headset), close the app
        Application.Quit();
#endif
    }

    public void OnClickSettings()
    {
        settingsPanel.SetActive(true); // Show the settings
    }

    public void OnClickCloseSettings()
    {
        settingsPanel.SetActive(false); // Hide the settings
     }

    public void OnClickTutorial()
    {
        // Make sure "Tutorial" matches the exact name of your tutorial scene file
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.LoadScene("Tutorial"); 
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Tutorial");
        }
    }
}