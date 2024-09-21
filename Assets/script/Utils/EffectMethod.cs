using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameNamespace;
using Unity.VisualScripting;
using UnityEngine;


public class EffectMethod
{
    private GameObject choiceCard;
    UtilMethod utilMethod = new UtilMethod();
    public async Task P1DrawCard(ApplyEffectEventArgs e, int drawAmount, DrawCard drawCard)
    {
        CardManager cardManager = GameObject.Find("P1CardManager").GetComponent<CardManager>();
        GameObject manager = GameObject.Find("GameManager");

        for (int i = 0; i < drawAmount; i++)
        {
            cardManager.drawCard();
        }
        await drawCard.EffectOfEffect(e);
    }

    public async Task P2DrawCard(ApplyEffectEventArgs e, int drawAmount, DrawCard drawCard)
    {
        CardManager cardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
        for (int i = 0; i < drawAmount; i++)
        {
            cardManager.drawCard();
        }
        await drawCard.EffectOfEffect(e);
    }

    public async Task P1DestroyAllCard(ApplyEffectEventArgs e, TargetType targetFieldType, AoeDestroy aoeDestroy)
    {
        await aoeDestroy.EffectOfEffect(e);
        List<Card> targetFieldCards = P1GetTargetCards(targetFieldType);
        if (targetFieldCards != null)
        if (targetFieldCards == null || targetFieldCards.Count == 0)
        {
            return;
        }
        for (int i = targetFieldCards.Count - 1; i >= 0; i--)
        {
            targetFieldCards[i].gameObject.SetActive(false);
            await Task.Delay(1);
            await targetFieldCards[i].DestoryThis();
        }
    }

    public async Task P2DestroyAllCard(ApplyEffectEventArgs e, TargetType targetFieldType, AoeDestroy aoeDestroy)
    {
        await aoeDestroy.EffectOfEffect(e);
        List<Card> targetFieldCards = P2GetTargetCards(targetFieldType);
        if (targetFieldCards != null)
        for (int i = targetFieldCards.Count - 1; i >= 0; i--)
        {
            targetFieldCards[i].gameObject.SetActive(false);
            await Task.Delay(1);
            await targetFieldCards[i].DestoryThis();
        }
    }

    public async Task CannotPlayThis(ApplyEffectEventArgs e, CannotPlayThis cannotPlayThis)
    {
        GameObject card = e.Card.gameObject;
        card.GetComponent<CardDragAndDrop>().canDrag = false;
        await cannotPlayThis.EffectOfEffect(e);
    }

    public async Task DamageP1ProtectShieldByRatio(ApplyEffectEventArgs e, double ratio, ProtectShieldDamageByRatio protectShieldDamageByRatio)
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        Leader leader = gameManager.myLeader.GetComponent<Leader>();
        double damage = leader.ProtectShield * ratio;
        leader.ProtectShield -= (int)(damage * (1 - leader.shieldDamageCutAmount));
        leader.protectShieldText.text = leader.ProtectShield.ToString();
        await protectShieldDamageByRatio.EffectOfEffect(e);
    }

    public async Task DamageP2ProtectShieldByRatio(ApplyEffectEventArgs e, double ratio, ProtectShieldDamageByRatio protectShieldDamageByRatio)
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        Leader leader = gameManager.enemyLeader.GetComponent<Leader>();
        double damage = leader.ProtectShield * ratio;
        leader.ProtectShield -= (int)(damage * (1 - leader.shieldDamageCutAmount));
        leader.protectShieldText.text = leader.ProtectShield.ToString();
        await protectShieldDamageByRatio.EffectOfEffect(e);
    }

    public async Task ShieldAndLeaderAttack(ApplyEffectEventArgs e, LeaderAndShieldAttackCard leaderAndShieldAttackCard)
    {
        Leader leader = GameManager.defenceObject.GetComponent<Leader>();
        if (leader != null && leader.ProtectShield > 0)
        {
            leader.Hp -= GameManager.attackObject.GetComponent<Card>().attack;
            leader.healthText.text = leader.Hp.ToString();
            await leaderAndShieldAttackCard.EffectOfEffect(e);
        }
    }

    public async Task DamageCard(ApplyEffectEventArgs e, int damageAmount, HpDamageCard hpDamageCard)
    {
        await hpDamageCard.EffectOfEffect(e);
        await utilMethod.DamageMethod(e.Card, damageAmount);
    }

    public async Task P1CannotAttackOtherCard(ApplyEffectEventArgs e, CannotAttackOtherCard cannotAttackOtherCard)
    {
        CardManager cardManager = GameObject.Find("P1CardManager").GetComponent<CardManager>();
        cardManager.CardsWithProtectEffectOnField.Add(e.Card);
        e.Card.shiledIcon.SetActive(true);
        await cannotAttackOtherCard.EffectOfEffect(e);
    }

    public async Task P2CannotAttackOtherCard(ApplyEffectEventArgs e, CannotAttackOtherCard cannotAttackOtherCard)
    {
        CardManager cardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
        cardManager.CardsWithProtectEffectOnField.Add(e.Card);
        e.Card.shiledIcon.SetActive(true);
        await cannotAttackOtherCard.EffectOfEffect(e);
    }

    public async Task P1CannotAttackOtherDeffenceCard(ApplyEffectEventArgs e, CannotAttackOtherDeffenceCard cannotAttackOtherDeffenceCard)
    {
        CardManager cardManager = GameObject.Find("P1CardManager").GetComponent<CardManager>();
        cardManager.CannotAttackMyDefenceCard.Add(e.Card);
        await cannotAttackOtherDeffenceCard.EffectOfEffect(e);
    }

    public async Task P2CannotAttackOtherDeffenceCard(ApplyEffectEventArgs e, CannotAttackOtherDeffenceCard cannotAttackOtherDeffenceCard)
    {
        CardManager cardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
        cardManager.CannotAttackMyDefenceCard.Add(e.Card);
        await cannotAttackOtherDeffenceCard.EffectOfEffect(e);
    }

    public async Task BuffMyCard(ApplyEffectEventArgs e, int buffAmount, TargetType targetType, AttackBuffCard attackBuffCard)
    {
        ApplyBuffToCard(e.Card, targetType, buffAmount);
        await attackBuffCard.EffectOfEffect(e);
    }

    public async Task CancelBuffMyAttackCard(ApplyEffectEventArgs e, TargetType buffType, CancelAttackBuff cancelAttackBuff, int cancleBuffAmount)
    {
        int amount = cancleBuffAmount;
        if (cancleBuffAmount == 2)
        {
            amount = e.Card.attack / cancleBuffAmount;
        }
        if (buffType == TargetType.All)
        {
            e.Card.attack -= amount;
            e.Card.attackText.text = e.Card.attack.ToString();
            await utilMethod.DamageMethod(e.Card, amount);
        }
        else if (buffType == TargetType.Attack)
        {
            e.Card.attack -= amount;
            e.Card.attackText.text = e.Card.attack.ToString();
        }
        else if (buffType == TargetType.Defence)
        {
            await utilMethod.DamageMethod(e.Card, amount);
        }
        await cancelAttackBuff.EffectOfEffect(e);
    }

    public async Task DestroyCard(ApplyEffectEventArgs e, DestroyCard destroyCard)
    {
        if (e.Card.inf.cardType != CardType.Spel)
        {
            await destroyCard.EffectOfEffect(e);
        }
        if (e.ChoiceCard != null)
        {
            e.ChoiceCard.gameObject.SetActive(false);
            await e.ChoiceCard.DestoryThis();
        }
        else
        {
            e.Card.gameObject.SetActive(false);
            await e.Card.DestoryThis();
        }
    }

    public async Task HealCard(ApplyEffectEventArgs e, int healAmount, HpHealCard hpHealCard, string target = null)
    {
        if (!string.IsNullOrEmpty(target))
        {
            foreach (Card card in e.PCards)
            {
                if (card.inf.name == target)
                {
                    e.ChoiceCard = card;
                    if (healAmount == 0)
                    {
                        card.hp = card.inf.hp;
                        card.healthText.text = card.hp.ToString();
                        await hpHealCard.EffectOfEffect(e);
                    }
                    else
                    {
                        card.hp += healAmount;
                        card.healthText.text = card.hp.ToString();
                        await hpHealCard.EffectOfEffect(e);
                    }
                }
            }
            return;
        }
        else
        {
            if (healAmount == 0)
            {
                e.Card.hp = e.Card.inf.hp;
                e.Card.healthText.text = e.Card.hp.ToString();
                await hpHealCard.EffectOfEffect(e);
                return;
            }
            e.Card.hp += healAmount;
            e.Card.healthText.text = e.Card.hp.ToString();
            await hpHealCard.EffectOfEffect(e);
        }
    }

    public async Task P1DamageShield(ApplyEffectEventArgs e, int damageAmount, ShieldDamageCard shieldDamageCard)
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        Leader leader = gameManager.myLeader.GetComponent<Leader>();
        if (damageAmount == 0)
        {
            leader.ProtectShield = 0;
            leader.protectShieldText.text = leader.ProtectShield.ToString();
            await shieldDamageCard.EffectOfEffect(e);
            return;
        }
        leader.ProtectShield -= (int)(damageAmount * (1 - leader.shieldDamageCutAmount));
        leader.protectShieldText.text = leader.ProtectShield.ToString();
        await shieldDamageCard.EffectOfEffect(e);
    }

    public async Task P2DamageShield(ApplyEffectEventArgs e, int damageAmount, ShieldDamageCard shieldDamageCard)
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        Leader leader = gameManager.enemyLeader.GetComponent<Leader>();
        if (damageAmount == 0)
        {
            leader.ProtectShield = 0;
            leader.protectShieldText.text = leader.ProtectShield.ToString();
            await shieldDamageCard.EffectOfEffect(e);
            return;
        }
        leader.ProtectShield -= (int)(damageAmount * (1 - leader.shieldDamageCutAmount));
        leader.protectShieldText.text = leader.ProtectShield.ToString();
        await shieldDamageCard.EffectOfEffect(e);
    }

    public async Task P1CannnotDraw(ApplyEffectEventArgs e, CannnotDraw cannnotDraw)
    {
        CardManager cardManager = GameObject.Find("P1CardManager").GetComponent<CardManager>();
        cardManager.CannotDrawEffectList.Add(e.Card);
        await cannnotDraw.EffectOfEffect(e);
    }

    public async Task P1CannnotDrawCancel(ApplyEffectEventArgs e, CannnotDrawCancel cannnotDrawCancel)
    {
        CardManager cardManager = GameObject.Find("P1CardManager").GetComponent<CardManager>();
        cardManager.CannotDrawEffectList.RemoveAt(cardManager.CannotDrawEffectList.Count - 1);
        await cannnotDrawCancel.EffectOfEffect(e);
    }

    public async Task P2CannnotDrawCancel(ApplyEffectEventArgs e, CannnotDrawCancel cannnotDrawCancel)
    {
        CardManager cardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
        cardManager.CannotDrawEffectList.RemoveAt(cardManager.CannotDrawEffectList.Count - 1);
        await cannnotDrawCancel.EffectOfEffect(e);
    }

    public async Task P2CannnotDraw(ApplyEffectEventArgs e, CannnotDraw cannnotDraw)
    {
        CardManager cardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
        cardManager.CannotDrawEffectList.Add(e.Card);
        await cannnotDraw.EffectOfEffect(e);
    }

    public async Task AttackImmediately(ApplyEffectEventArgs e, AttackImmediatelyCard attackImmediatelyCard)
    {
        for (int i = 0; i < e.Card.attackedList.Count; i++)
        {
            e.Card.attackedList[i] = false;
        }
        await attackImmediatelyCard.EffectOfEffect(e);
    }

    public async Task P1HealShield(ApplyEffectEventArgs e, int healAmount, ProtectShieldHeal protectShieldHeal)
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        gameManager.myLeader.GetComponent<Leader>().ProtectShield += healAmount;
        gameManager.myLeader.GetComponent<Leader>().protectShieldText.text = gameManager.myLeader.GetComponent<Leader>().ProtectShield.ToString();
        await protectShieldHeal.EffectOfEffect(e);
    }

    public async Task P2HealShield(ApplyEffectEventArgs e, int healAmount, ProtectShieldHeal protectShieldHeal)
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        gameManager.enemyLeader.GetComponent<Leader>().ProtectShield += healAmount;
        gameManager.enemyLeader.GetComponent<Leader>().protectShieldText.text = gameManager.enemyLeader.GetComponent<Leader>().ProtectShield.ToString();
        await protectShieldHeal.EffectOfEffect(e);
    }

    public async Task P1Aoe(ApplyEffectEventArgs e, int damageAmount, TargetType targetType, AoeCard aoeCard)
    {
        await aoeCard.EffectOfEffect(e);
        List<Card> targetFieldsCards = P1GetTargetCards(targetType);
        if (targetFieldsCards != null)
            for (int i = targetFieldsCards.Count - 1; i >= 0; i--)
            {
                await utilMethod.DamageMethod(targetFieldsCards[i], damageAmount);
            }
    }

    public async Task P2Aoe(ApplyEffectEventArgs e, int damageAmount, TargetType targetType, AoeCard aoeCard)
    {
        await aoeCard.EffectOfEffect(e);
        List<Card> targetFieldsCards = P2GetTargetCards(targetType);
        if (targetFieldsCards != null)
        for (int i = targetFieldsCards.Count - 1; i >= 0; i--)
        {
            await utilMethod.DamageMethod(targetFieldsCards[i], damageAmount);
        }
    }

    public async Task AvoidAttack(ApplyEffectEventArgs e, AvoidAttackCard avoidAttackCard)
    {
        e.Card.canAvoidAttack = true;
        await avoidAttackCard.EffectOfEffect(e);
    }

    public async Task P2ManaDecrease(ApplyEffectEventArgs e, int decreaseAmount, ManaDecrease manaDecrease)
    {
        GameObject manager = GameObject.Find("GameManager");
        ManaManager manaManager = manager.GetComponent<ManaManager>();
        await manaDecrease.EffectOfEffect(e);
        manaManager.P2ManaDecrease(decreaseAmount);
    }

    public async Task P1ManaDecrease(ApplyEffectEventArgs e, int decreaseAmount, ManaDecrease manaDecrease)
    {
        GameObject manager = GameObject.Find("GameManager");
        ManaManager manaManager = manager.GetComponent<ManaManager>();
        await manaDecrease.EffectOfEffect(e);
        manaManager.P1ManaDecrease(decreaseAmount);
    }

    public async Task P1MaxManaIncrease(ApplyEffectEventArgs e, int increaseAmount, MaxManaIncrease maxManaIncrease)
    {
        GameObject manager = GameObject.Find("GameManager");
        ManaManager manaManager = manager.GetComponent<ManaManager>();
        await maxManaIncrease.EffectOfEffect(e);
        manaManager.P1MaxManaIncrease(increaseAmount);
    }
    public async Task P2MaxManaIncrease(ApplyEffectEventArgs e, int increaseAmount, MaxManaIncrease maxManaIncrease)
    {
        GameObject manager = GameObject.Find("GameManager");
        ManaManager manaManager = manager.GetComponent<ManaManager>();
        await maxManaIncrease.EffectOfEffect(e);
        manaManager.P2MaxManaIncrease(increaseAmount);
    }

    public async Task P1ShieldDamageCut(ApplyEffectEventArgs e, double damageCut, ProtectShieldDamageCut protectShieldDamageCut)
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        await protectShieldDamageCut.EffectOfEffect(e);
        gameManager.myLeader.GetComponent<Leader>().shieldDamageCutAmount += damageCut;
    }

    public async Task P2ShieldDamageCut(ApplyEffectEventArgs e, double damageCut, ProtectShieldDamageCut protectShieldDamageCut)
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        await protectShieldDamageCut.EffectOfEffect(e);
        gameManager.enemyLeader.GetComponent<Leader>().shieldDamageCutAmount += damageCut;
    }

    public async Task NotAttacked(ApplyEffectEventArgs e, NotAttackedCard notAttackedCard)
    {
        await notAttackedCard.EffectOfEffect(e);
        e.Card.canAttackTarget = false;
    }

    public async Task P1CannotPlayAssistCard(ApplyEffectEventArgs e, CannotPlayAssistCard cannotPlayAssistCard)
    {
        CardManager cardManager = GameObject.Find("P1CardManager").GetComponent<CardManager>();
        await cannotPlayAssistCard.EffectOfEffect(e);
        cardManager.CannotPlayDefenceCard.Add(e.Card);
    }

    public async Task P2CannotPlayAssistCard(ApplyEffectEventArgs e, CannotPlayAssistCard cannotPlayAssistCard)
    {
        CardManager cardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
        await cannotPlayAssistCard.EffectOfEffect(e);
        cardManager.CannotPlayDefenceCard.Add(e.Card);
    }

    public async Task P1BuffAttackFieldCards(ApplyEffectEventArgs e, int buffAmount, TargetType targetType, BuffAttackFieldCards buffAttackFieldCards)
    {
        await buffAttackFieldCards.EffectOfEffect(e);
        if (targetType == TargetType.All)
        {
            if (e.PCards != null && e.PCards.Count != 0)
            foreach (Card card in e.PCards)
            {
                card.attack += buffAmount;
                card.attackText.text = card.attack.ToString();
            }
        }
        else if (targetType == TargetType.Attack)
        {
            if (e.PCards != null && e.PCards.Count != 0)
            foreach (Card card in e.PAttackCards)
            {
                card.attack += buffAmount;
                card.attackText.text = card.attack.ToString();
            }
        }
        else if (targetType == TargetType.Defence)
        {
            if (e.PCards != null && e.PCards.Count != 0)
            foreach (Card card in e.PDefenceCards)
            {
                card.attack += buffAmount;
                card.attackText.text = card.attack.ToString();
            }
        }
    }
    public async Task P2BuffAttackFieldCards(ApplyEffectEventArgs e, int buffAmount, TargetType targetType, BuffAttackFieldCards buffAttackFieldCards)
    {
        await buffAttackFieldCards.EffectOfEffect(e);
        if (targetType == TargetType.All)
        {
            if (e.Cards != null && e.Cards.Count != 0)
            foreach (Card card in e.Cards)
            {
                card.attack += buffAmount;
                card.attackText.text = card.attack.ToString();
            }
        }
        else if (targetType == TargetType.Attack)
        {
            if (e.EAttackCards != null && e.EAttackCards.Count != 0)
            foreach (Card card in e.EAttackCards)
            {
                card.attack += buffAmount;
                card.attackText.text = card.attack.ToString();
            }
        }
        else if (targetType == TargetType.Defence)
        {
            if (e.EDefenceCards != null && e.EDefenceCards.Count != 0)
            foreach (Card card in e.EDefenceCards)
            {
                card.attack += buffAmount;
                card.attackText.text = card.attack.ToString();
            }
        }
    }

    public async Task P1RandomDestroyCard(ApplyEffectEventArgs e, int destroyAmount, TargetType targetType, RandomDestroyCard randomDestroyCard)
    {
        for (int i = 0; i < destroyAmount; i++)
        {
            List<Card> cards = P1GetTargetCards(targetType);
            int randomValue = UnityEngine.Random.Range(0, cards.Count);
            if (cards.Count == 0 && randomValue == 0)
                return;
            else
            {
                e.ChoiceCard = cards[randomValue];
                await randomDestroyCard.EffectOfEffect(e);
                cards[randomValue].gameObject.SetActive(false);
                await Task.Delay(1);
                await cards[randomValue].DestoryThis();
            }
        }
    }

    public async Task P2RandomDestroyCard(ApplyEffectEventArgs e, int destroyAmount, TargetType targetType, RandomDestroyCard randomDestroyCard)
    {
        for (int i = 0; i < destroyAmount; i++)
        {
            List<Card> cards = P2GetTargetCards(targetType);
            int randomValue = UnityEngine.Random.Range(0, cards.Count);
            if (cards.Count == 0 && randomValue == 0)
                return;
            else
            {
                e.ChoiceCard = cards[randomValue];
                await randomDestroyCard.EffectOfEffect(e);
                cards[randomValue].gameObject.SetActive(false);
                await Task.Delay(1);
                await cards[randomValue].DestoryThis();
            }
        }
    }

    public async Task CanAttackTwice(ApplyEffectEventArgs e, TwiceAttack twice)
    {
        if (e.Card.attackedList.Count == 1)
        {
            await twice.EffectOfEffect(e);
            e.Card.attackedList.Add(true);
        }
        else
            return;
    }

    public async Task P1ManaHeal(ApplyEffectEventArgs e, int healAmount, ManaHeal manaHeal)
    {
        GameObject manager = GameObject.Find("GameManager");
        ManaManager manaManager = manager.GetComponent<ManaManager>();
        await manaHeal.EffectOfEffect(e);
        manaManager.P1ManaHeal(healAmount);
    }

    public async Task P2ManaHeal(ApplyEffectEventArgs e, int healAmount, ManaHeal manaHeal)
    {
        GameObject manager = GameObject.Find("GameManager");
        ManaManager manaManager = manager.GetComponent<ManaManager>();
        await manaHeal.EffectOfEffect(e);
        manaManager.P2ManaHeal(healAmount);
    }

    public async Task CancelNotAttacked(ApplyEffectEventArgs e, CancelNotAttacked cancelNotAttacked)
    {
        await cancelNotAttacked.EffectOfEffect(e);
        e.Card.canAttackTarget = true;
    }

    public void P1InstantiateToken(CardInf token)
    {
        CardManager cardManager = GameObject.Find("P1CardManager").GetComponent<CardManager>();
        GameObject cardObject = UnityEngine.Object.Instantiate(cardManager.cardPrefab);
        cardObject.GetComponent<Card>().P1SetUp(token);
        GameObject myAttackField = GameObject.Find("myAttackField");
        GameObject myDefenceField = GameObject.Find("myDefenceField");
        if (token.cardType == CardType.Attack)
        {
            cardObject.transform.SetParent(myAttackField.transform, false);
            cardManager.AttackFields.Add(cardObject.GetComponent<Card>());
            cardManager.AllFields.Add(cardObject.GetComponent<Card>());
        }
        else if (token.cardType == CardType.Defence)
        {
            cardObject.transform.SetParent(myDefenceField.transform, false);
            cardManager.DefenceFields.Add(cardObject.GetComponent<Card>());
            cardManager.AllFields.Add(cardObject.GetComponent<Card>());
        }
        cardObject.transform.localScale = Vector3.one;
    }

    public void P2InstantiateToken(CardInf token)
    {
        CardManager cardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
        GameObject cardObject = UnityEngine.Object.Instantiate(cardManager.cardPrefab);
        cardObject.GetComponent<Card>().P2SetUp(token);
        cardObject.GetComponent<Card>().blindPanel.SetActive(false);
        GameObject enemyAttackField = GameObject.Find("enemyAttackField");
        GameObject enemyDefenceField = GameObject.Find("enemyDefenceField");
        if (token.cardType == CardType.Attack)
        {
            cardObject.transform.SetParent(enemyAttackField.transform, false);
            cardManager.AttackFields.Add(cardObject.GetComponent<Card>());
            cardManager.AllFields.Add(cardObject.GetComponent<Card>());
        }
        else if (token.cardType == CardType.Defence)
        {
            cardObject.transform.SetParent(enemyDefenceField.transform, false);
            cardManager.DefenceFields.Add(cardObject.GetComponent<Card>());
            cardManager.AllFields.Add(cardObject.GetComponent<Card>());
        }
        cardObject.transform.localScale = Vector3.one;
    }

    public void P1CheckTopCard()
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        UIManager uIManager = manager.GetComponent<UIManager>();
        CardManager cardManager = GameObject.Find("P1CardManager").GetComponent<CardManager>();
        uIManager.checkPanel.SetActive(true);
        choiceCard = UnityEngine.Object.Instantiate(cardManager.cardPrefab, uIManager.checkPanel.transform, false);
        choiceCard.GetComponent<Card>().P1SetUp(gameManager.allCardInf.allList[CardManager.DeckInf[cardManager.DeckIndex]]);
        Transform parentTransform = choiceCard.transform;
        Transform top = parentTransform.Find("topButton");
        Transform under = parentTransform.Find("underButton");

        GameObject topButton = top.gameObject;
        GameObject underButton = under.gameObject;
        topButton.SetActive(true);
        underButton.SetActive(true);
    }

    public void P2CheckTopCard()
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        UIManager uIManager = manager.GetComponent<UIManager>();
        CardManager cardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
        uIManager.checkPanel.SetActive(true);
        choiceCard = UnityEngine.Object.Instantiate(cardManager.cardPrefab, uIManager.checkPanel.transform, false);
        choiceCard.GetComponent<Card>().P2SetUp(gameManager.allCardInf.allList[CardManager.enemyDeckInf[cardManager.DeckIndex]]);
        Transform parentTransform = choiceCard.transform;
        Transform top = parentTransform.Find("topButton");
        Transform under = parentTransform.Find("underButton");

        GameObject topButton = top.gameObject;
        GameObject underButton = under.gameObject;
        topButton.SetActive(true);
        underButton.SetActive(true);
    }
    public void CancelCheckTopCard()
    {
        GameObject manager = GameObject.Find("GameManager");
        UIManager uIManager = manager.GetComponent<UIManager>();
        Transform checkPanelTransform = uIManager.checkPanel.transform;
        int childCount = checkPanelTransform.childCount;
        Transform targetTransform = checkPanelTransform.GetChild(childCount - 1);
        GameObject target = targetTransform.gameObject;
        if (target != null)
        {
            UnityEngine.Object.Destroy(target);
        }

        if (uIManager.checkPanel != null)
        {
            uIManager.checkPanel.SetActive(false);
        }
    }

    public async Task P1AoeBuffCard(ApplyEffectEventArgs e, TargetType fieldType, TargetType buffType, int buffAmount, string target, AoebuffCard aoebuffCard)
    {
        await aoebuffCard.EffectOfEffect(e);

        List<Card> targetCards = P1GetTargetCards(fieldType);

        // 各カードに対してバフを適用
        if(targetCards != null)
        foreach (Card card in targetCards)
        {
            // ターゲットが指定されている場合、その名前に一致するカードにのみ適用
            if (!string.IsNullOrEmpty(target) && target != card.inf.cardName)
                continue;

            ApplyBuffToCard(card, buffType, buffAmount);
        }
    }

    public async Task P2AoeBuffCard(ApplyEffectEventArgs e, TargetType fieldType, TargetType buffType, int buffAmount, string target, AoebuffCard aoebuffCard)
    {
        // 効果を適用
        await aoebuffCard.EffectOfEffect(e);

        // 対象となるカードのリストを取得
        List<Card> targetCards = P2GetTargetCards(fieldType);

        // 各カードに対してバフを適用
        foreach (Card card in targetCards)
        {
            // ターゲットが指定されている場合、その名前に一致するカードにのみ適用
            if (!string.IsNullOrEmpty(target) && target != card.inf.cardName)
                continue;

            ApplyBuffToCard(card, buffType, buffAmount);
        }
    }

    private List<Card> P1GetTargetCards(TargetType fieldType)
    {
        CardManager cardManager = GameObject.Find("P1CardManager").GetComponent<CardManager>();
        return fieldType switch
        {
            TargetType.All => cardManager.AllFields?.ToList() ?? new List<Card>(),
            TargetType.Attack => cardManager.AttackFields,
            TargetType.Defence => cardManager.DefenceFields,
            _ => new List<Card>()
        };
    }

    private List<Card> P2GetTargetCards(TargetType fieldType)
    {
        CardManager cardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
        return fieldType switch
        {
            TargetType.All => cardManager.AllFields?.ToList() ?? new List<Card>(),
            TargetType.Attack => cardManager.AttackFields,
            TargetType.Defence => cardManager.DefenceFields,
            _ => new List<Card>()
        };
    }
    private void ApplyBuffToCard(Card card, TargetType buffType, int buffAmount)
    {
        switch (buffType)
        {
            case TargetType.All:
                card.attack += buffAmount;
                card.maxHp += buffAmount;
                card.hp += buffAmount;
                break;
            case TargetType.Attack:
                if (buffAmount == 2)
                {
                    card.attack *= buffAmount;
                }
                else
                {
                    card.attack += buffAmount;
                }
                break;
            case TargetType.Defence:
                card.maxHp += buffAmount;
                card.hp += buffAmount;
                break;
        }

        // テキストを更新
        card.attackText.text = card.attack.ToString();
        card.healthText.text = card.hp.ToString();
    }

}
