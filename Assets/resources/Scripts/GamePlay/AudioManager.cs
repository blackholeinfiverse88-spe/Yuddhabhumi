using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Clips")]
    public AudioClip clickSound;
    public AudioClip hoverSound;
    public AudioClip battleStartSound;
    public AudioClip victorySound;
    public AudioClip defeatSound;

    private void Awake()
    {
        // 1. Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Survive scene changes
        }
        else
        {
            // 2. Duplicate Check
            Destroy(gameObject); 
        }
    }

    public void PlayClick()
    {
        if (clickSound && sfxSource) sfxSource.PlayOneShot(clickSound);
    }

    public void PlayHover()
    {
        if (hoverSound && sfxSource) sfxSource.PlayOneShot(hoverSound);
    }

    public void PlayBattleStart()
    {
        if (battleStartSound && sfxSource) sfxSource.PlayOneShot(battleStartSound);
    }

    public void PlayVictory()
    {
        // Stop music to emphasize victory
        if (musicSource) musicSource.Stop(); 
        if (victorySound && sfxSource) sfxSource.PlayOneShot(victorySound);
    }

    // --- THIS WAS MISSING ---
    public void PlayDefeat()
    {
        // Stop happy music
        if (musicSource) musicSource.Stop(); 
        
        // Play sad sound
        if (defeatSound && sfxSource) sfxSource.PlayOneShot(defeatSound);
    }
}