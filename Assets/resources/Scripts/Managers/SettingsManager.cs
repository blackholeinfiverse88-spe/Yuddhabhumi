using UnityEngine;
using UnityEngine.UI;
using TMPro; // Use this if your Dropdown is TextMeshPro, otherwise use UnityEngine.UI

public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public Slider volumeSlider;
    public TMP_Dropdown qualityDropdown; // Or just "Dropdown" if using legacy UI
    public Slider sensitivitySlider;

    private void Start()
    {
        // 1. Load saved Volume (Default to 100%)
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        if (volumeSlider) volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume;

        // 2. Load saved Quality (Default to Medium/High)
        int savedQuality = PlayerPrefs.GetInt("QualityLevel", 2);
        if (qualityDropdown) qualityDropdown.value = savedQuality;
        QualitySettings.SetQualityLevel(savedQuality);

        // 3. Load saved Sensitivity (Default to 1.0)
        float savedSens = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
        if (sensitivitySlider) sensitivitySlider.value = savedSens;
    }

    // --- AUDIO ---
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    // --- GRAPHICS ---
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
    }

    // --- SENSITIVITY ---
    // (Other scripts like your PlayerController will read this PlayerPref later)
    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
        PlayerPrefs.Save(); // Force save immediately for this one
    }
}