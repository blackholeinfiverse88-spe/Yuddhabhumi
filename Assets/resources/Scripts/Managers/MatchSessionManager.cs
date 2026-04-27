using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections;

public class MatchSessionManager : MonoBehaviour
{
    [Header("Cinematic Elements")]
    public AudioSource cinematicAudio;  // Drag the "War Horn" Audio Source here
    public float startDelay = 0.5f;     // How long to sit in pitch blackness

    [Header("UI Groups")]
    public CanvasGroup gameplayUI;      // Drag 'Gameplay_Group' here
    
    [Header("Phase UI")]
    public GameObject introPanel;       // "BATTLE START" Text
    public GameObject dangerVignette;   // Red pulsing screen
    // REMOVED: timerText (GameManagerVR handles this now)

    [Header("Game References")]
    public BaseHealth playerBase;       
    public PlayerTroopSpawnerVR spawner; 

    [Header("Settings")]
    public float tensionThreshold = 0.3f; 

    private bool isTensionActive = false;

    void Start()
    {
        // 1. Reset Report Card Stats (Important for Day 4)
        MatchStats.Reset();

        // 2. Start the Cinematic Intro
        StartCoroutine(CinematicIntro());
    }

    void Update()
    {
        // REMOVED: Timer Logic. GameManagerVR counts down, we don't need to count up.
        
        // Only watch for Tension
        MonitorTension();
    }

    IEnumerator CinematicIntro()
    {
        // A. LOCK IT DOWN (Force Screen Black)
        if (VRScreenFader.Instance != null)
        {
            VRScreenFader.Instance.SetAlpha(1); // Force immediate black
        }
        else
        {
            Debug.LogWarning("VRScreenFader missing! Make sure you started from Boot Scene.");
        }

        // Disable Controls & UI
        if (spawner != null) spawner.enabled = false;
        if (gameplayUI) gameplayUI.alpha = 0;
        if (introPanel) introPanel.SetActive(false);
        if (dangerVignette) dangerVignette.SetActive(false);

        // B. THE AUDIO CUE (War Horn)
        yield return new WaitForSeconds(startDelay);
        if (cinematicAudio != null)
        {
            cinematicAudio.Play();
        }

        // Wait a moment for the sound to hit before revealing
        yield return new WaitForSeconds(1.0f);

        // C. THE REVEAL (Fade In)
        if (VRScreenFader.Instance != null)
        {
            VRScreenFader.Instance.FadeIn(); // Slowly clear the black screen
        }

        // D. SHOW TEXT ("BATTLE START")
        if (introPanel) introPanel.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        if (introPanel) introPanel.SetActive(false);

        // E. UNLOCK GAMEPLAY
        if (spawner != null) spawner.enabled = true; 
        
        // Fade in the Deck UI nicely
        if (gameplayUI)
        {
            float duration = 1.0f;
            float time = 0;
            while (time < duration)
            {
                gameplayUI.alpha = Mathf.Lerp(0, 1, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            gameplayUI.alpha = 1; 
        }

        Debug.Log("Cinematic Start Complete.");
    }

    void MonitorTension()
    {
        if (playerBase == null || isTensionActive) return;

        float hpPercent = playerBase.currentHealth / playerBase.maxHealth;

        if (hpPercent <= tensionThreshold)
        {
            ActivateTensionMode();
        }
    }

    void ActivateTensionMode()
    {
        isTensionActive = true;
        if (dangerVignette) dangerVignette.SetActive(true);
    }
}