using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class VRScreenFader : MonoBehaviour
{
    public static VRScreenFader Instance;

    [Header("UI References")]
    public Canvas fadeCanvas;
    public Image fadeImage;

    [Header("Settings")]
    public float fadeDuration = 1.0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Default behavior: Fade in when level loads
        FadeIn(); 
    }

    // --- NEW FUNCTION FOR CINEMATICS ---
    // Allows MatchSessionManager to instantly force screen BLACK (1) or CLEAR (0)
    public void SetAlpha(float alpha)
    {
        StopAllCoroutines(); // Stop any auto-fading
        
        if (fadeCanvas) fadeCanvas.enabled = (alpha > 0);
        
        if (fadeImage) 
            fadeImage.color = new Color(0, 0, 0, alpha);
    }
    // -----------------------------------

    public void FadeOut()
    {
        fadeCanvas.enabled = true;
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(0, 1)); 
    }

    public void FadeIn()
    {
        fadeCanvas.enabled = true;
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(1, 0)); 
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha)
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime; 
            
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            if(fadeImage != null) 
                fadeImage.color = new Color(0, 0, 0, alpha);
            
            yield return null;
        }

        // Final Snap
        if(fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, endAlpha);

        if (endAlpha == 0) fadeCanvas.enabled = false;
    }

    void LateUpdate()
    {
        if (!fadeCanvas.enabled) return;

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // Stick fader to face so player can't look around it
            transform.position = mainCam.transform.position + (mainCam.transform.forward * 0.5f);
            transform.rotation = mainCam.transform.rotation;
        }
    }
}