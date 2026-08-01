using UnityEngine;

public class PlayerPrefsDebugger : MonoBehaviour
{
    void Start()
    {
        Debug.Log("MusicVolume = " + PlayerPrefs.GetFloat("MusicVolume", -1));
        Debug.Log("SFXVolume = " + PlayerPrefs.GetFloat("SFXVolume", -1));
        Debug.Log("MusicEnabled = " + PlayerPrefs.GetInt("MusicEnabled", -1));
        Debug.Log("SFXEnabled = " + PlayerPrefs.GetInt("SFXEnabled", -1));
    }
}