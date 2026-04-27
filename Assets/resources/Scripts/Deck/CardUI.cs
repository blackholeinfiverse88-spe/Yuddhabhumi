using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    public UnitData data; // Drag the Knight/Archer/Tank data here
    public Image iconImage;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI nameText;
    
    private DeckBuilderManager manager;

    public void Setup(UnitData unit, DeckBuilderManager dm)
    {
        data = unit;
        manager = dm;

        // Update Visuals
        if(nameText) nameText.text = unit.unitName;
        if(costText) costText.text = unit.cost.ToString();
        if(iconImage && unit.icon) iconImage.sprite = unit.icon;
    }

    public void OnClick()
    {
        // Tell the manager "I was clicked!"
        if (manager != null) manager.OnCardClicked(data);
    }
}