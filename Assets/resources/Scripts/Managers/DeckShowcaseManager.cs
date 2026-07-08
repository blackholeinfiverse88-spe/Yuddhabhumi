using UnityEngine;
using TMPro;

public class DeckShowcaseManager : MonoBehaviour
{
    [Header("3D Showcase")]
    public Transform showcaseStage; // Where the 3D model spawns
    private GameObject currentModel; // Tracks the currently spawned model

    [Header("Stats UI Canvas")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI healthText; // Assuming your UnitData has these!
    public TextMeshProUGUI behaviorText; 

    // The Deck Builder buttons will call this function!
    public void DisplayCharacter(UnitData selectedCard)
    {
        if (selectedCard == null) return;

        // 1. Update the Stats Text
        nameText.text = selectedCard.unitName;
        costText.text = "Cost: " + selectedCard.cost.ToString();
        
        // (Add health and behavior variables to your UnitData script if you haven't yet!)
        // healthText.text = "HP: " + selectedCard.maxHealth.ToString();
        // behaviorText.text = selectedCard.behaviorSignature; 

        // 2. Clear the old 3D model from the stage
        if (currentModel != null)
        {
            Destroy(currentModel);
        }

        // 3. Spawn the new 3D model onto the stage
        if (selectedCard.prefab != null)
        {
            currentModel = Instantiate(selectedCard.prefab, showcaseStage.position, showcaseStage.rotation);
            
            // Optional: Disable the model's scripts so it doesn't try to attack or walk away!
            // currentModel.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
            // currentModel.GetComponent<TeamComponent>().enabled = false; 
        }
    }
}