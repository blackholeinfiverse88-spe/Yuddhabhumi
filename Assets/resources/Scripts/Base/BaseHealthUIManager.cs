using UnityEngine;
using UnityEngine.UI;

public class BaseHealthUIManager : MonoBehaviour
{
    public static BaseHealthUIManager Instance;

    [Header("Sliders")]
    public Slider playerSlider;
    public Slider enemySlider;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdatePlayerHealth(float current, float max)
    {
        playerSlider.maxValue = max;
        playerSlider.value = current;
    }

    public void UpdateEnemyHealth(float current, float max)
    {
        enemySlider.maxValue = max;
        enemySlider.value = current;
    }
}
