using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "LeaderInf",menuName = "自作データ/LeaderInf")]
public class LeaderInf : ScriptableObject
{
    public int hp;
    public int protectShield;
    public string leaderName;
    public Sprite leaderSprite;
}
