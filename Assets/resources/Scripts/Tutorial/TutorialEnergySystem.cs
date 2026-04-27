using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialEnergySystem : MonoBehaviour
{
    public float maxEnergy = 10f;
    public float regenPerTick = 1f;
    public float regenInterval = 1.8f;

    public float currentEnergy;
    float regenMultiplier = 1f;

    public Slider energySlider;
    public TextMeshProUGUI energyText;

    void Awake()
    {
        // Start at 0 when the level loads
        currentEnergy = 0f;
        UpdateUI();
    }

    void OnEnable()
    {
        // Because we use OnEnable instead of Start, the elixir will wait 
        // until the exact moment the Section 2 Trigger turns this script on!
        InvokeRepeating(nameof(Regenerate), regenInterval, regenInterval);
    }

    void OnDisable()
    {
        // Stop regenerating if the script is ever turned off
        CancelInvoke(nameof(Regenerate));
    }

    void Regenerate()
    {
        currentEnergy = Mathf.Clamp(currentEnergy + regenPerTick * regenMultiplier, 0f, maxEnergy);
        UpdateUI();
    }

    public void SetRegenMultiplier(float m)
    {
        regenMultiplier = m;
    }

    public bool TrySpend(float amount)
    {
        if (currentEnergy < amount) return false;

        currentEnergy -= amount;
        
        // --- CHANGED FOR TUTORIAL ---
        // We removed the GameFlowManager tracking here so 
        // tutorial stats don't permanently alter player profiles.
        // ----------------------------

        UpdateUI();
        return true;
    }

    void UpdateUI()
    {
        if (energySlider)
            energySlider.value = currentEnergy / maxEnergy;

        if (energyText)
            energyText.text = $"{Mathf.FloorToInt(currentEnergy)} / {maxEnergy}";
    }
}