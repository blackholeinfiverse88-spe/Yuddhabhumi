using UnityEngine;

public class SceneMusicTrigger : MonoBehaviour
{
    [Header("Scene Music")]
    [Tooltip("Drag the specific background music for THIS scene here.")]
    public AudioClip sceneMusicTrack;

    void Start()
    {
        // 1. Safety Check: Does the immortal Audio Manager exist?
        if (AudioManager.Instance != null && sceneMusicTrack != null)
        {
            // 2. The "Skip" Check: If the exact same song is already playing 
            // (like if they restarted the level), don't interrupt it!
            if (AudioManager.Instance.musicSource.clip != sceneMusicTrack)
            {
                // 3. Swap the CD and hit play
                AudioManager.Instance.musicSource.clip = sceneMusicTrack;
                AudioManager.Instance.musicSource.Play();
            }
        }
        else if (AudioManager.Instance == null)
        {
            Debug.LogWarning("SceneMusicTrigger: No Audio Manager found in this scene! Did you start testing from the Home menu?");
        }
    }
}