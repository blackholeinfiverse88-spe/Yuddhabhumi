using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SimpleSceneMusic : MonoBehaviour
{
    [Header("Scene Track")]
    public AudioClip backgroundMusic;

    void Start()
    {
        AudioSource audioSource = GetComponent<AudioSource>();

        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.volume = 1f;
            
            // Crucial VR Settings: Force 2D so it stays in the player's headset
            audioSource.spatialBlend = 0f; 
            audioSource.playOnAwake = false;

            audioSource.Play();
        }
    }
}