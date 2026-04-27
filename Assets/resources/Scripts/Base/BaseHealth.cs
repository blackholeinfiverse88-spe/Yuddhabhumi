using UnityEngine;
using UnityEngine.UI;

public class BaseHealth : MonoBehaviour
{
    public float maxHealth = 500f;

    [Header("UI Reference")]
    public Slider healthSlider;  // Drag your slider here in Inspector
    
    public float currentHealth;
    public Team team;

    private void Start()
    {
        currentHealth = maxHealth;

        // --- NEW LOGIC: Setup the Slider directly ---
        if (healthSlider != null)
        {
            // We use normalized value (0 to 1) for sliders
            healthSlider.value = 1f; 
        }

        Debug.Log($"[{team}] Base initialized with HP: {currentHealth}");

        // --------------------------------------------
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // --- NEW LOGIC: Update the Slider directly ---
        if (healthSlider != null) 
        {
            healthSlider.value = currentHealth / maxHealth;
        }
        // ---------------------------------------------

        if (currentHealth <= 0f)
        {
            // Notify the Game Manager that the game is over
            if (GameManagerVR.Instance != null)
            {
                GameManagerVR.Instance.OnBaseDestroyed(team == Team.Player);
            }
            else
            {
                Debug.Log((team == Team.Player ? "Player" : "Enemy") + " Base Destroyed!");
            }
        }
        Debug.Log($"[{team}] Base HP: {currentHealth}/{maxHealth}");

    }
}