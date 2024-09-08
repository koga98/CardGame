using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static List<Card> PAttackFields;
    public static List<Card> PDefenceFields;
    //守護用のリスト
    public static List<Card> P1CardsWithProtectEffectOnField;
    public static List<Card> P2CardsWithProtectEffectOnField;
    //アシストかーどをプレイできなくするのリスト
    public static List<Card> P1CannotPlayDefenceCard;
    public static List<Card> P2CannotPlayDefenceCard;
    //フィールド上で効果を発揮するバフカードのリスト
    public static List<Card> P1CardsWithEffectOnField;
    public static List<Card> P2CardsWithEffectOnField;
    //プロテクトシールドの変動に反応するカード
    
    //攻撃中の効果用のリスト
    public static List<Card> P1EffectDuringAttacking;
    public static List<Card> P2EffectDuringAttacking;
    //効果にターン経過を要するスペルカード用のリスト
    public static List<Card> P1SpelEffectAfterSomeTurn;
    public static List<Card> P2SpelEffectAfterSomeTurn;
    //フィールドにいる限り補助エリアを攻撃できなくさせるカードのリスト
    public static List<Card> P1CannotAttackMyDefenceCard;
    public static List<Card> P2CannotAttackMyDefenceCard;

    public static List<bool> p1CannotDrawEffectList;
    public static List<bool> p2CannotDrawEffectList;

    void Start(){
        P1EffectDuringAttacking = new List<Card>();
        P2EffectDuringAttacking = new List<Card>();
        P1CannotPlayDefenceCard = new List<Card>();
        P2CannotPlayDefenceCard = new List<Card>();
        P1CardsWithEffectOnField = new List<Card>();
        P2CardsWithEffectOnField = new List<Card>();
        P1SpelEffectAfterSomeTurn = new List<Card>();
        P2SpelEffectAfterSomeTurn = new List<Card>();
        P2CardsWithProtectEffectOnField = new List<Card>();
        P1CardsWithProtectEffectOnField = new List<Card>();
        P1CannotAttackMyDefenceCard = new List<Card>();
        P2CannotAttackMyDefenceCard = new List<Card>();
        p1CannotDrawEffectList = new List<bool>();
        p2CannotDrawEffectList = new List<bool>();
    }
}
