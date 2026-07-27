using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Local reference for scripts inside the SAME scene to easily call AudioManager.Instance
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Background Music for THIS Scene")]
    [Tooltip("Drag the specific BGM asset for this level here.")]
    public AudioClip sceneGameplayMusic;

    [Header("UI Sounds")]
    public AudioClip clickSound;

    [Header("Game Sounds")]
    public AudioClip battleStartSound;
    public AudioClip victorySound;
    public AudioClip defeatSound;

    void Awake()
    {
        // ====== ADD THESE TWO LINES FOR TESTING ======
    PlayerPrefs.DeleteAll();
    PlayerPrefs.Save();
    Debug.Log("--- PLAYER PREFS CLEARED FOR TESTING ---");
    // =============================================
 
    // If an Instance already exists and it's not this one, destroy this duplicate
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }

    // Set the persistent instance
    Instance = this;
    
    // Crucial: Keep this GameObject alive when loading new scenes!
    DontDestroyOnLoad(gameObject);

    }

    void Start()
    {
        // 1. Unmute Unity's global master volume
        AudioListener.volume = 1f;

        // 2. Play this scene's specific music immediately if assigned
        if (musicSource != null && sceneGameplayMusic != null)
        {
            musicSource.clip = sceneGameplayMusic;
            musicSource.loop = true;
            musicSource.volume = 1f;
            
            // Crucial VR Fix: Force to 2D so it follows the player headset perfectly
            musicSource.spatialBlend = 0f; 
            
            musicSource.Play();
            Debug.Log("[AUDIO] Playing scene music: " + sceneGameplayMusic.name);
        }
    }

    //======================
    // UI SOUNDS FUNCTIONS
    //======================

    public void PlayClick()
    {
        if (clickSound != null && sfxSource != null)
        {
            sfxSource.spatialBlend = 0f; // Force to 2D for VR
            sfxSource.PlayOneShot(clickSound);
        }
    }

    //======================
    // GAMEPLAY SOUNDS FUNCTIONS
    //======================

    public void PlayBattleStart()
    {
        if (battleStartSound != null && sfxSource != null)
        {
            sfxSource.spatialBlend = 0f;
            sfxSource.PlayOneShot(battleStartSound);
        }
    }

    public void PlayVictory()
    {
        if (victorySound != null && sfxSource != null)
        {
            sfxSource.spatialBlend = 0f;
            sfxSource.PlayOneShot(victorySound);
        }
    }

    public void PlayDefeat()
    {
        if (defeatSound != null && sfxSource != null)
        {
            sfxSource.spatialBlend = 0f;
            sfxSource.PlayOneShot(defeatSound);
        }
    }
}