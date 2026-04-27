using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Game/Unit Data")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public GameObject prefab;
    public float cost;
    public Sprite icon; // For the UI button
    
    [TextArea]
    public string description;
}