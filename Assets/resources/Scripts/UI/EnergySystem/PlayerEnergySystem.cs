using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerEnergySystem : MonoBehaviour
{
    public float maxEnergy = 10f;
    public float regenPerTick = 1f;
    public float regenInterval = 1.8f;

    public float currentEnergy;
    float regenMultiplier = 1f;

    public Slider energySlider;
    public TextMeshProUGUI energyText;

    void Start()
    {
        currentEnergy = 0f;
        InvokeRepeating(nameof(Regenerate), regenInterval, regenInterval);
        UpdateUI();
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
        GameFlowManager.Instance?.AddEnergySpent(amount);
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
