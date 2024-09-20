using System.Collections;
using System.Collections.Generic;
using GameNamespace;
using UnityEditor;
using UnityEngine;

public class ConditionEffects
{
    GameObject manager;
    GameManager gameManager;

    public bool P1IsMoreShieldOnDamage(int conditionDamageAmount)
    {
        manager = GameObject.Find("GameManager");
        gameManager = manager.GetComponent<GameManager>();
        if (gameManager.myLeader.GetComponent<Leader>().ProtectShield <= conditionDamageAmount)
        {
            return false;
        }
        gameManager.myLeader.GetComponent<Leader>().ProtectShield -= conditionDamageAmount;
        gameManager.myLeader.GetComponent<Leader>().protectShieldText.text = gameManager.myLeader.GetComponent<Leader>().ProtectShield.ToString();
        return true;
    }
    public bool P2IsMoreShieldOnDamage(int conditionDamageAmount)
    {
        manager = GameObject.Find("GameManager");
        gameManager = manager.GetComponent<GameManager>();
        if (gameManager.enemyLeader.GetComponent<Leader>().ProtectShield <= conditionDamageAmount)
        {
            return false;
        }
        gameManager.enemyLeader.GetComponent<Leader>().ProtectShield -= conditionDamageAmount;
        gameManager.enemyLeader.GetComponent<Leader>().protectShieldText.text = gameManager.enemyLeader.GetComponent<Leader>().ProtectShield.ToString();
        return true;
    }

    public bool P1IsFirstTurn()
    {
        manager = GameObject.Find("GameManager");
        gameManager = manager.GetComponent<GameManager>();
        if (gameManager.p1_turnElapsed == 1)
        {
            return true;
        }
        return false;
    }

    public bool P2IsFirstTurn()
    {
        manager = GameObject.Find("GameManager");
        gameManager = manager.GetComponent<GameManager>();
        if (gameManager.p1_turnElapsed == 1)
        {
            return true;
        }
        return false;
    }

    public bool P1NoCardOnField(TargetType targetType)
    {
        CardManager cardManager = GameObject.Find("P1CardManager").GetComponent<CardManager>();
        if (targetType == TargetType.All)
        {
            if (cardManager.AllFields.Count == 0)
            {
                return true;
            }
            return false;
        }
        else if (targetType == TargetType.Defence)
        {
            if (cardManager.DefenceFields.Count == 0)
            {
                return true;
            }
            return false;
        }
        else
        {
            if (cardManager.AttackFields.Count == 0)
            {
                return true;
            }
            return false;
        }

    }

    public bool P2NoCardOnField(TargetType targetType)
    {
        CardManager cardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
        if (targetType == TargetType.All)
        {
            if (cardManager.AllFields.Count == 0)
            {
                return true;
            }
            return false;
        }
        else if (targetType == TargetType.Defence)
        {
            if (cardManager.DefenceFields.Count == 0)
            {
                return true;
            }
            return false;
        }
        else
        {
            if (cardManager.AttackFields.Count == 0)
            {
                return true;
            }
            return false;
        }
    }

    public bool P1SomeCardOnField(TargetType targetType,int some)
    {
        CardManager cardManager = GameObject.Find("P1CardManager").GetComponent<CardManager>();
        if (targetType == TargetType.All)
        {
            if (cardManager.AllFields.Count >= some)
            {
                return true;
            }
            return false;
        }
        else if (targetType == TargetType.Defence)
        {
            if (cardManager.DefenceFields.Count  >= some)
            {
                return true;
            }
            return false;
        }
        else
        {
            if (cardManager.AttackFields.Count  >= some)
            {
                return true;
            }
            return false;
        }

    }
    public bool P2SomeCardOnField(TargetType targetType,int some)
    {
        CardManager cardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
        if (targetType == TargetType.All)
        {
            if (cardManager.AllFields.Count >= some)
            {
                return true;
            }
            return false;
        }
        else if (targetType == TargetType.Defence)
        {
            if (cardManager.DefenceFields.Count >= some)
            {
                return true;
            }
            return false;
        }
        else
        {
            if (cardManager.AttackFields.Count >= some)
            {
                return true;
            }
            return false;
        }
    }

    public bool ElapsedTurns(ApplyEffectEventArgs e, int waitThisTime)
    {

        if (e.Card.elapsedTurns % waitThisTime == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool OneTimeElapsedTurns(ApplyEffectEventArgs e, int waitThisTime)
    {

        if (e.Card.elapsedTurns  ==  waitThisTime)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool LimitTurns(ApplyEffectEventArgs e, int limitTime){
        if (e.Card.elapsedTurns >= limitTime)
        {
            return false;
        }
        else
        {
            return true;
           
        }
    }

    public bool JudgeCardProparty(ApplyEffectEventArgs e, CardType target){
        if(e.Card.inf.cardType == target){
            return true;
        }else{
            return false;
        }
    }

    public bool IsTarget(ApplyEffectEventArgs e, string property,CardType cardType)
    {
        Card card = GameManager.defenceObject.GetComponent<Card>();
        Leader leader = GameManager.defenceObject.GetComponent<Leader>();
        if (card != null && leader == null && cardType != CardType.None)
        {
            if (cardType == card.inf.cardType)
            {

                return true;
            }
        }
        else if (leader != null && card == null)
        {
            if (property == "プロテクトバリア" && leader.ProtectShield > 0)
            {
                return true;
            }
            else if (property == "リーダー" && leader.ProtectShield <= 0)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsDuringAttack(ApplyEffectEventArgs e)
    {

        if (e.Card.CardOwner == PlayerID.Player1)
        {
            CardManager cardManager = GameObject.Find("P1CardManager").GetComponent<CardManager>();
            cardManager.EffectDuringAttacking.Add(e.Card);
        }
        else
        {
            CardManager cardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
            cardManager.EffectDuringAttacking.Add(e.Card);
        }
        return true;

    }

    public bool IsClearOdds(float odds)
    {
        float randomValue = UnityEngine.Random.value;
        if (randomValue <= odds)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool PlayIsTargetParticularCard(ApplyEffectEventArgs e, string target)
    {
        if (e.Card.inf.name == target)
        {
            return true;
        }
        return false;
    }

    public bool P1IsParticularCardOnField(ApplyEffectEventArgs e, string target)
    {
        foreach (Card card in e.PCards)
        {
            if (card.inf.name == target)
            {
                return true;
            }
        }
        return false;
    }

    public bool P2IsParticularCardOnField(ApplyEffectEventArgs e, string target)
    {
        foreach (Card card in e.Cards)
        {
            if (card.inf.name == target)
            {
                return true;
            }
        }
        return false;
    }

    public bool P1IsTargetTurn(ApplyEffectEventArgs e, int targetTurn)
    {
        manager = GameObject.Find("GameManager");
        gameManager = manager.GetComponent<GameManager>();
        if (gameManager.p1_turnElapsed == targetTurn)
        {
            return true;
        }
        return false;
    }

    public bool P2IsTargetTurn(ApplyEffectEventArgs e, int targetTurn)
    {
        manager = GameObject.Find("GameManager");
        gameManager = manager.GetComponent<GameManager>();
        if (gameManager.p2_turnElapsed == targetTurn)
        {
            return true;
        }
        return false;
    }

    public bool ProtectShiledIsThresholdValue(int threshold, Leader leader)
    {
        if (leader.ProtectShield <= threshold)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
