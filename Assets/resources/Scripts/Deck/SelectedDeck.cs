using System.Collections.Generic;
using UnityEngine;

public static class SelectedDeck
{
    // This static list survives scene changes automatically
    public static List<UnitData> deck = new List<UnitData>();

    public static void AddUnit(UnitData unit)
    {
        if (deck.Count < 3) // Limit deck size to 3
        {
            deck.Add(unit);
        }
    }

    public static void ClearDeck()
    {
        deck.Clear();
    }
}