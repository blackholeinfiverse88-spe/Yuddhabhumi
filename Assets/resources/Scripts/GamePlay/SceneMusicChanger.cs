using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SceneMusicChanger : MonoBehaviour
{
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        audioSource.Stop();
        audioSource.time = 0f;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.Play();
        Debug.Log("SceneMusicChanger Start() called");
    }

    void OnDestroy()
    {
        // Stop automatically when leaving the scene
        if (audioSource != null)
            audioSource.Stop();
    }
}