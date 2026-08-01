using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public Slider volumeSlider;
    public TMP_Dropdown qualityDropdown;
    public Slider sensitivitySlider;

    private void Start()
    {
        // 1. Load saved Volume (Default to 1.0 / 100%)
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);

        if (volumeSlider != null)
        {
            // IMPORTANT: Temporarily remove listener so setting .value doesn't trigger SetVolume automatically
            volumeSlider.onValueChanged.RemoveListener(SetVolume);
            
            // Set slider bounds safety
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.wholeNumbers = false; // Ensures smooth decimal volume

            volumeSlider.value = savedVolume;

            // Re-attach listener after value is set
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        AudioListener.volume = savedVolume;

        // 2. Load saved Quality
        int savedQuality = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        if (qualityDropdown != null)
        {
            qualityDropdown.onValueChanged.RemoveListener(SetQuality);
            qualityDropdown.value = savedQuality;
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }
        QualitySettings.SetQualityLevel(savedQuality);

        // 3. Load saved Sensitivity
        float savedSens = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.RemoveListener(SetSensitivity);
            sensitivitySlider.value = savedSens;
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        }

        Debug.Log("Loaded Master Volume = " + savedVolume);
    }

    // --- AUDIO ---
    public void SetVolume(float volume)
    {
        // Clamp value between 0.0 (0%) and 1.0 (100%)
        float clampedVolume = Mathf.Clamp01(volume);

        Debug.Log($"SetVolume called with value: {clampedVolume}");

        AudioListener.volume = clampedVolume;
        PlayerPrefs.SetFloat("MasterVolume", clampedVolume);
        PlayerPrefs.Save();
    }

    // --- GRAPHICS ---
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
        PlayerPrefs.Save();
    }

    // --- SENSITIVITY ---
    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
        PlayerPrefs.Save();
    }
}