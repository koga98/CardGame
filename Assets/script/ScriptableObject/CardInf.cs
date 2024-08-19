using System.Collections;
using System.Collections.Generic;
using GameNamespace;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "CardInf",menuName = "自作データ/CardInf")]
public class CardInf :ScriptableObject
{
    public AnimationClip attackClip;
    public int Id;
    public int attack;
    public int hp;
    public int cost;
    public CardType cardType;
    public string cardName;
    [TextArea(3, 10)]
    public string longText;
    public Sprite cardSprite;
    public List<EffectInf> effectInfs;
    
}
