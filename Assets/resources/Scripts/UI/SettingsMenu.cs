using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VRSettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown graphicsDropdown;
    public Slider volumeSlider;
    public Slider sensitivitySlider;

    // PlayerPrefs Keys
    private const string GRAPHICS_KEY = "GraphicsQuality";
    private const string VOLUME_KEY = "MasterVolume";
    private const string SENSITIVITY_KEY = "Sensitivity";

    void Start()
    {
        LoadSettings();

        graphicsDropdown.onValueChanged.AddListener(SetGraphics);
        volumeSlider.onValueChanged.AddListener(SetVolume);
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
    }

    void LoadSettings()
    {
        // Graphics
        int savedGraphics = PlayerPrefs.GetInt(GRAPHICS_KEY, QualitySettings.GetQualityLevel());
        graphicsDropdown.value = savedGraphics;
        QualitySettings.SetQualityLevel(savedGraphics);

        // Volume
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);
        volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume;

        // Sensitivity
        float savedSensitivity = PlayerPrefs.GetFloat(SENSITIVITY_KEY, 1f);
        sensitivitySlider.value = savedSensitivity;
    }

    public void SetGraphics(int value)
    {
        QualitySettings.SetQualityLevel(value);
        PlayerPrefs.SetInt(GRAPHICS_KEY, value);
        PlayerPrefs.Save();
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VOLUME_KEY, value);
        PlayerPrefs.Save();
    }

    public void SetSensitivity(float value)
    {
        PlayerPrefs.SetFloat(SENSITIVITY_KEY, value);
        PlayerPrefs.Save();
    }

    public float GetSensitivity()
    {
        return PlayerPrefs.GetFloat(SENSITIVITY_KEY, 1f);
    }
}
