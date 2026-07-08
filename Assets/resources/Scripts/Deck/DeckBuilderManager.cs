using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class DeckBuilderManager : MonoBehaviour
{
    [Header("Configuration")]
    public List<UnitData> allAvailableUnits;
    public int maxDeckSize = 3;

    [Header("UI References")]
    public Transform libraryGrid;
    public Transform currentDeckGrid;
    public GameObject cardPrefab;
    public Button startBattleButton;
    public TextMeshProUGUI statusText;

    // --- NEW: Add a reference to your Showcase Manager ---
    [Header("Showcase Reference")]
    public DeckShowcaseManager showcaseManager; 

    private List<UnitData> currentDeck = new List<UnitData>();

    private void Start()
    {
        // Safety
        if (startBattleButton == null || statusText == null)
        {
            Debug.LogError("DeckBuilderManager: UI references missing!");
            enabled = false;
            return;
        }

        SelectedDeck.ClearDeck();
        PopulateLibrary();
        UpdateUI();
    }

    void PopulateLibrary()
    {
        if (libraryGrid == null || cardPrefab == null)
        {
            Debug.LogError("DeckBuilderManager: Library setup missing!");
            return;
        }

        foreach (var unit in allAvailableUnits)
        {
            GameObject newCard = Instantiate(cardPrefab, libraryGrid);
            CardUI cardScript = newCard.GetComponent<CardUI>();
            cardScript.Setup(unit, this);
        }
    }

    public void OnCardClicked(UnitData unit)
    {
        if (unit == null) return;

        // --- NEW: Tell the Showcase Manager to display this specific unit! ---
        if (showcaseManager != null)
        {
            showcaseManager.DisplayCharacter(unit);
        }

        if (currentDeck.Contains(unit))
        {
            currentDeck.Remove(unit);
        }
        else if (currentDeck.Count < maxDeckSize)
        {
            currentDeck.Add(unit);
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        // Clear deck visuals
        foreach (Transform child in currentDeckGrid)
            Destroy(child.gameObject);

        // Rebuild deck visuals
        foreach (var unit in currentDeck)
        {
            GameObject cardObj = Instantiate(cardPrefab, currentDeckGrid);
            cardObj.GetComponent<CardUI>().Setup(unit, this);
        }

        statusText.text = $"Deck: {currentDeck.Count} / {maxDeckSize}";
        startBattleButton.interactable = (currentDeck.Count == maxDeckSize);
    }

    public void OnClickStartBattle()
    {
        // HARD SAFETY CHECKS
        if (currentDeck.Count != maxDeckSize)
        {
            Debug.LogWarning("Deck incomplete. Cannot start battle.");
            return;
        }

        if (GameFlowManager.Instance == null)
        {
            Debug.LogError("GameFlowManager is missing in scene!");
            return;
        }

        // Save deck
        SelectedDeck.deck = new List<UnitData>(currentDeck);

        // Load battle
        GameFlowManager.Instance.LoadScene("Level2");
    }
}
