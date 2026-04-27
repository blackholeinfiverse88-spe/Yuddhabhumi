using UnityEngine;

public class VRButtonTroop : MonoBehaviour
{
    public BattleUIManager battleUI;

    [Header("Hand Slot Index (0, 1, 2)")]
    public int index;

    public void PressButton()
    {
        if (battleUI != null)
        {
            battleUI.UseCard(index);
        }
    }
}