using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DeckInf",menuName = "自作データ/DeckInf")]
public class DeckInf :ScriptableObject
{
   public List<Card> deck = new List<Card>();
}
