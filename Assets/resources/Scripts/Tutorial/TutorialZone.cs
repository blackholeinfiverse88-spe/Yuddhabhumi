using UnityEngine;
using UnityEngine.Events;

public class TutorialZone : MonoBehaviour
{
    [Header("Images to Show / Hide")]
    [Tooltip("How many Canvases do you want to TURN ON?")]
    public GameObject[] imagesToShow; 
    
    [Tooltip("How many Canvases do you want to TURN OFF?")]
    public GameObject[] imagesToHide;

    [Header("Gameplay Triggers")]
    [Tooltip("Drag the Elixir System here to wake it up")]
    public MonoBehaviour scriptToEnable; 

    [Header("Custom Actions")]
    [Tooltip("Add your Spawner Unlock and POV Unlock here!")]
    public UnityEvent onZoneEnter; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Loop through and turn ON all new Canvases
            foreach (GameObject canvas in imagesToShow)
            {
                if (canvas != null) canvas.SetActive(true);
            }

            // 2. Loop through and turn OFF all old Canvases (if you want to hide Section 1)
            foreach (GameObject canvas in imagesToHide)
            {
                if (canvas != null) canvas.SetActive(false);
            }

            // 3. Start the Elixir
            if (scriptToEnable != null) scriptToEnable.enabled = true;

            // 4. Fire off custom events (like Unlocking the Spawner!)
            if (onZoneEnter != null) onZoneEnter.Invoke();

            // 5. Turn off this invisible trigger so it doesn't fire again
            gameObject.SetActive(false); 
        }
    }
}