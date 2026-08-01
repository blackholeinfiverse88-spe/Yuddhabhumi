using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("UI Sounds")]
    public AudioClip clickSound;

    [Header("Game Sounds")]
    public AudioClip battleStartSound;
    public AudioClip victorySound;
    public AudioClip defeatSound;

    private void Awake()
    {
        // 1. Check duplicate BEFORE doing any setup
        if (Instance != null && Instance != this)
        {
            // CRITICAL FIX: Disable component first so OnDisable / OnEnable 
            // unsubscribes aren't executed in a way that breaks the singleton
            enabled = false; 
            Destroy(gameObject);
            return;
        }

        // 2. Assign Singleton & Persist
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 3. Ensure sources are forced 2D for VR Headset
        if (musicSource != null) musicSource.spatialBlend = 0f;
        if (sfxSource != null) sfxSource.spatialBlend = 0f;
    }

    private void OnEnable()
    {
        // Only register event if this is the active singleton instance
        if (Instance == null || Instance == this)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnDisable()
    {
        // Only unregister if this instance is the actual active singleton
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Global VR Unpause Fix
        AudioListener.pause = false;
        AudioListener.volume = 1f;

        // Find the AudioSceneData helper (if present in the newly loaded scene)
        AudioSceneData sceneData = FindFirstObjectByType<AudioSceneData>();
        if (sceneData != null && sceneData.sceneMusic != null)
        {
            PlayMusic(sceneData.sceneMusic);
        }
        else
        {
            // If music was paused/stopped by Unity during scene unload, force resume it
            if (musicSource != null && !musicSource.isPlaying && musicSource.clip != null)
            {
                musicSource.UnPause();
                musicSource.Play();
            }
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;

        // Don't restart if the exact same music track is already playing
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f; // Force 2D
        musicSource.Play();
    }

    // ==========================================
    // UI & SFX FUNCTIONS
    // ==========================================

    public void PlayClick()
    {
        if (clickSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }

    public void PlayBattleStart()
    {
        if (battleStartSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(battleStartSound);
        }
    }

    public void PlayVictory()
    {
        if (victorySound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(victorySound);
        }
    }

    public void PlayDefeat()
    {
        if (defeatSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(defeatSound);
        }
    }
}