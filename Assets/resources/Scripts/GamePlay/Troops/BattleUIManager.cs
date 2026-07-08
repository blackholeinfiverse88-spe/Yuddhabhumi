using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BattleUIManager : MonoBehaviour
{
    [Header("Managers")]
    public PlayerTroopSpawnerVR spawner;
    public PlayerEnergySystem energySystem;

    [Header("Deck UI Buttons (3 Hand Slots)")]
    public Button cardButton1;
    public Button cardButton2;
    public Button cardButton3;

    [Header("Next Card Preview (Full Card Object)")]
    public GameObject nextCardPreview;

    [Header("Elixir UI")]
    public Slider elixirSlider;
    public TextMeshProUGUI elixirText;

    private UnitData[] handSlots = new UnitData[3];
    private UnitData nextCard;
    private Queue<UnitData> cycleQueue = new Queue<UnitData>();
    private Button[] cardButtons;

    private List<UnitData> runtimeDeck = new List<UnitData>();

    private void Start()
    {
        Invoke(nameof(SetupDeckUI), 0.1f);
    }

    void SetupDeckUI()
    {
        cardButtons = new Button[] { cardButton1, cardButton2, cardButton3 };

        List<UnitData> sourceDeck = new List<UnitData>();

        if (SelectedDeck.deck != null && SelectedDeck.deck.Count > 0)
            sourceDeck = new List<UnitData>(SelectedDeck.deck);
        else if (spawner != null && spawner.debugDeck != null && spawner.debugDeck.Count > 0)
            sourceDeck = new List<UnitData>(spawner.debugDeck);

        if (sourceDeck.Count < 4)
        {
            Debug.LogError("Need at least 4 cards");
            return;
        }

        ShuffleDeck(sourceDeck);
        runtimeDeck = new List<UnitData>(sourceDeck);

        cycleQueue.Clear();

        handSlots[0] = runtimeDeck[0];
        handSlots[1] = runtimeDeck[1];
        handSlots[2] = runtimeDeck[2];

        nextCard = runtimeDeck[3];

        for (int i = 4; i < runtimeDeck.Count; i++)
            cycleQueue.Enqueue(runtimeDeck[i]);

        UpdateAllCardVisuals();
        SetupButtonListeners();
    }

    void SetupButtonListeners()
    {
        cardButton1.onClick.RemoveAllListeners();
        cardButton2.onClick.RemoveAllListeners();
        cardButton3.onClick.RemoveAllListeners();

        cardButton1.onClick.AddListener(() => UseCard(0));
        cardButton2.onClick.AddListener(() => UseCard(1));
        cardButton3.onClick.AddListener(() => UseCard(2));
    }

    public void UseCard(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex > 2)
            return;

        UnitData usedCard = handSlots[slotIndex];
        if (usedCard == null)
            return;

        if (spawner == null)
            return;

        // authoritative energy system
        PlayerEnergySystem es = energySystem != null ? energySystem : spawner.energySystem;

        if (es != null && es.currentEnergy < usedCard.cost)
            return;

        float energyBefore = es != null ? es.currentEnergy : 0f;
        float maxEnergy = es != null ? es.maxEnergy : 10f;

        // Gameplay execution unchanged
        spawner.SpawnUnit(usedCard);

        float energyAfter = es != null ? es.currentEnergy : energyBefore;
        bool spent = energyAfter < (energyBefore - 0.0001f);

        if (spent)
        {
            float dmgTotal = (GameFlowManager.Instance != null) ? GameFlowManager.Instance.totalDamageDealt : 0f;

            if (BehaviorTraceRecorder.Instance != null)
                BehaviorTraceRecorder.Instance.RecordSuccessfulDeploy(usedCard, energyBefore, energyAfter, maxEnergy, dmgTotal);

            if (KarmaStateTracker.Instance != null)
                KarmaStateTracker.Instance.TriggerDerivation();
        }

        // Deck rotation + animation unchanged
        cycleQueue.Enqueue(usedCard);

        UnitData cardFromPreview = nextCard;

        if (cycleQueue.Count > 0)
            nextCard = cycleQueue.Dequeue();
        else
            nextCard = null;

        StartCoroutine(FlipAndPop(slotIndex, cardFromPreview));

        UpdateNextCardVisual();
        StartCoroutine(FadePreview());
    }

    void UpdateAllCardVisuals()
    {
        for (int i = 0; i < 3; i++)
            UpdateButtonVisuals(cardButtons[i], handSlots[i]);

        UpdateNextCardVisual();
    }

    void UpdateButtonVisuals(Button btn, UnitData data)
    {
        if (btn == null || data == null)
            return;

        Transform icon = btn.transform.Find("Icon");
        if (icon)
            icon.GetComponent<Image>().sprite = data.icon;

        Transform cost = btn.transform.Find("CostText");
        if (cost)
            cost.GetComponent<TextMeshProUGUI>().text = data.cost.ToString();

        Transform name = btn.transform.Find("NameText");
        if (name)
            name.GetComponent<TextMeshProUGUI>().text = data.unitName;
    }

    void UpdateNextCardVisual()
    {
        if (nextCardPreview == null)
            return;

        if (nextCard == null)
        {
            nextCardPreview.SetActive(false);
            return;
        }

        nextCardPreview.SetActive(true);

        Transform icon = nextCardPreview.transform.Find("Icon");
        if (icon)
            icon.GetComponent<Image>().sprite = nextCard.icon;

        Transform cost = nextCardPreview.transform.Find("CostText");
        if (cost)
            cost.GetComponent<TextMeshProUGUI>().text = nextCard.cost.ToString();

        Transform name = nextCardPreview.transform.Find("NameText");
        if (name)
            name.GetComponent<TextMeshProUGUI>().text = nextCard.unitName;
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

    System.Collections.IEnumerator FadePreview()
    {
        if (nextCardPreview == null)
            yield break;

        CanvasGroup cg = nextCardPreview.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = nextCardPreview.AddComponent<CanvasGroup>();

        cg.alpha = 0f;

        float time = 0f;
        float duration = 0.2f;

        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, time / duration);
            yield return null;
        }

        cg.alpha = 1f;
    }

    System.Collections.IEnumerator FlipAndPop(int slotIndex, UnitData newCardData)
    {
        Transform cardTransform = cardButtons[slotIndex].transform;

        float flipDuration = 0.15f;
        float popDuration = 0.1f;

        Vector3 originalScale = Vector3.one;
        cardTransform.localScale = originalScale;

        float time = 0f;

        while (time < flipDuration)
        {
            time += Time.deltaTime;
            float scaleX = Mathf.Lerp(1f, 0f, time / flipDuration);
            cardTransform.localScale = new Vector3(scaleX, 1f, 1f);
            yield return null;
        }

        handSlots[slotIndex] = newCardData;
        UpdateButtonVisuals(cardButtons[slotIndex], newCardData);

        time = 0f;

        while (time < flipDuration)
        {
            time += Time.deltaTime;
            float scaleX = Mathf.Lerp(0f, 1f, time / flipDuration);
            cardTransform.localScale = new Vector3(scaleX, 1f, 1f);
            yield return null;
        }

        time = 0f;
        while (time < popDuration)
        {
            time += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 1.15f, time / popDuration);
            cardTransform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        time = 0f;
        while (time < popDuration)
        {
            time += Time.deltaTime;
            float scale = Mathf.Lerp(1.15f, 1f, time / popDuration);
            cardTransform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        cardTransform.localScale = originalScale;
    }

    public void OnClickQuitToMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Home");
    }

    void ShuffleDeck(List<UnitData> deck)
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            UnitData temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }
}