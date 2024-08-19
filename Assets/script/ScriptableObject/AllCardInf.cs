using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AllCardInf",menuName = "自作データ/AllCardInf")]
public class AllCardInf :ScriptableObject
{
    public List<CardInf> allList = new List<CardInf>();
    
}
