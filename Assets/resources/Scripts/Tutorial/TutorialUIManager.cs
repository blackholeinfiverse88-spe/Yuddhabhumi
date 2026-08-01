using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TutorialUIManager : MonoBehaviour
{
    [Header("Tutorial Systems")]
    public TutorialEnergySystem energySystem;    

    [Header("Fixed Tutorial Deck")]
    public List<UnitData> tutorialCards;       

    [Header("Deck UI Buttons")]
    public Button cardButton1; 
    public Button cardButton2;
    public Button cardButton3;

    [Header("Elixir UI")]
    public Slider elixirSlider;
    public TextMeshProUGUI elixirText;

    private void Start()
    {
        SetupTutorialDeckUI();
        Application.targetFrameRate=72;
    }

    void SetupTutorialDeckUI()
    {
        if (tutorialCards.Count > 0) UpdateButtonVisuals(cardButton1, tutorialCards[0]);
        if (tutorialCards.Count > 1) UpdateButtonVisuals(cardButton2, tutorialCards[1]);
        if (tutorialCards.Count > 2) UpdateButtonVisuals(cardButton3, tutorialCards[2]);
    }

    void UpdateButtonVisuals(Button btn, UnitData data)
    {
        if (btn == null || data == null) return;
        Transform iconTrans = btn.transform.Find("Icon");
        if (iconTrans) iconTrans.GetComponent<Image>().sprite = data.icon;
        Transform costTrans = btn.transform.Find("CostText");
        if (costTrans) costTrans.GetComponent<TextMeshProUGUI>().text = data.cost.ToString();
    }

    void Update()
    {
        if (energySystem != null)
        {
            if (elixirSlider) 
                elixirSlider.value = energySystem.currentEnergy / energySystem.maxEnergy;
            if (elixirText) 
                elixirText.text = $"{Mathf.FloorToInt(energySystem.currentEnergy)} / {energySystem.maxEnergy}";
        }
    }
    
    public void OnClickQuitToMenu()
    {
        Time.timeScale = 1f; 
        UnityEngine.SceneManagement.SceneManager.LoadScene("Home"); 
    }
}