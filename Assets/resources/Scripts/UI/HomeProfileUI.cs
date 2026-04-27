using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HomeProfileUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelText;    // Displays "1" inside the badge
    public Slider xpSlider;              // The purple progress bar
    public TextMeshProUGUI xpValueText;  // Displays "150 / 500 XP"
    public TextMeshProUGUI nameText;     // Optional: "COMMANDER"

    void Start()
    {
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        // 1. Safety Check: Does the Brain exist?
        if (GameFlowManager.Instance == null)
        {
            // Fallback for testing Home Scene directly without Boot
            if (levelText) levelText.text = "1";
            if (xpSlider) xpSlider.value = 0;
            return;
        }

        // 2. Get Data from the persistent Manager
        int lvl = GameFlowManager.Instance.playerLevel;
        float current = GameFlowManager.Instance.currentXP;
        float max = GameFlowManager.Instance.xpToNextLevel;

        // 3. Update Text
        if (levelText) levelText.text = lvl.ToString();
        
        // Format as whole numbers (e.g., 150 / 500)
        if (xpValueText) 
            xpValueText.text = $"{Mathf.FloorToInt(current)} / {Mathf.FloorToInt(max)} XP";

        // 4. Update Slider
        if (xpSlider)
        {
            xpSlider.maxValue = max;
            xpSlider.value = current;
        }
    }
}