using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    private void Start()
    {
        // Grab the button component on this object
        Button btn = GetComponent<Button>();
        
        // Add a listener via code so it never loses the reference
        if (btn != null)
        {
            btn.onClick.AddListener(PlaySound);
        }
    }

    private void PlaySound()
    {
        // Find the persistent singleton and tell it to play the click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClick();
        }
        else
        {
            Debug.LogWarning("[AUDIO] Button clicked, but AudioManager.Instance is missing!");
        }
    }
}