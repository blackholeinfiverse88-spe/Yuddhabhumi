using UnityEngine;

public class UserProfile : MonoBehaviour
{
    public static UserProfile Instance;

    public int currentLevel = 1;
    public int currentXP = 0;
    public int wins = 0;
    public int losses = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData(); // Load saved data when game starts
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddWin()
    {
        wins++;
        SaveData();
    }

    public void AddLoss()
    {
        losses++;
        SaveData();
    }

    // Simple saving using PlayerPrefs
    void SaveData()
    {
        PlayerPrefs.SetInt("Level", currentLevel);
        PlayerPrefs.SetInt("XP", currentXP);
        PlayerPrefs.SetInt("Wins", wins);
        PlayerPrefs.SetInt("Losses", losses);
        PlayerPrefs.Save();
    }

    void LoadData()
    {
        currentLevel = PlayerPrefs.GetInt("Level", 1);
        currentXP = PlayerPrefs.GetInt("XP", 0);
        wins = PlayerPrefs.GetInt("Wins", 0);
        losses = PlayerPrefs.GetInt("Losses", 0);
    }
}