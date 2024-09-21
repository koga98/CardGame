using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using GameNamespace;
using UnityEngine;
using UnityEngine.EventSystems;

public class EffectManager : MonoBehaviour
{
    public CardManager player1CardManager;
    public CardManager player2CardManager;
    public UIManager uIManager;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public async Task AttackEndEffect()
    {
        await ExecuteAttackEndEffect(player1CardManager.EffectDuringAttacking);
        await ExecuteAttackEndEffect(player2CardManager.EffectDuringAttacking);
    }

    private async Task ExecuteAttackEndEffect(List<Card> effectList)
    {
        if (effectList.Count == 0 || effectList[0] == null) return;
        var currentCard = effectList[0];
        await ExecuteEffects(currentCard, EffectInf.CardTrigger.EndAttack);
        effectList[0] = null;
    }

    public async Task EffectAfterDie(Card card)
    {
        await ExecuteEffects(card, EffectInf.CardTrigger.AfterDie);
    }

    public async Task OnProtectShieldChanged(PlayerType playerType)
    {
        if (playerType == PlayerType.Player1)
            await ProtectShieldDecreaseEffect(player1CardManager.CardsWithEffectOnField);

        else
            await ProtectShieldDecreaseEffect(player2CardManager.CardsWithEffectOnField);
    }

    private async Task ProtectShieldDecreaseEffect(List<Card> effectList)
    {
        if(effectList != null && effectList.Count != 0)
        for (int i = effectList.Count - 1; i >= 0; i--)
        {
            var card = effectList[i];
            for (int j = card.inf.effectInfs.Count - 1; j >= 0; j--)
            {
                var effectInf = card.inf.effectInfs[j];

                for (int k = effectInf.triggers.Count - 1; k >= 0; k--)
                {
                    var cardTrigger = effectInf.triggers[k];
                    if (cardTrigger == EffectInf.CardTrigger.ProtectShieldDecrease)
                        await ApplyEffect(effectInf, card);
                }
            }
        }
    }

    public async Task BeforeCardDrag(Card card)
    {
        await ExecuteEffects(card, EffectInf.CardTrigger.OnBeginDrag);
    }

    public IEnumerator PlayCardButtonEffect(Card card, EffectInf effect)
    {
        yield return StartCoroutine(ApplyEffect(effect, card).AsCoroutine());
        CardDragAndDrop.OnButtonCoroutine = true;
        StartCoroutine(WaitForButtonCoroutine(card));
    }

    private IEnumerator WaitForButtonCoroutine(Card card)
    {
        while (CardDragAndDrop.OnButtonCoroutine)
        {
            // `GameManager.completeButtonChoice` が true になるまで待機
            yield return new WaitUntil(() => GameManager.completeButtonChoice);

            foreach (var effect in card.inf.effectInfs)
            {
                for (int i = 0; i < effect.triggers.Count; i++)
                {
                    if (effect.triggers[i] == EffectInf.CardTrigger.StopButtonOperetion)
                        yield return StartCoroutine(ApplyEffect(effect, card).AsCoroutine());
                }
            }

            GameManager.completeButtonChoice = false;
        }
    }

    public async Task PlayCardRandomEffect(RandomCardInf randomCardInf, Card card)
    {
        int randomValue = UnityEngine.Random.Range(0, randomCardInf.effectInfs.Count);
        await ApplyEffect(randomCardInf.effectInfs[randomValue], card);
    }

    public IEnumerator PlayCardChoiceEffect(Card clickedObjectCard)
    {
        yield return StartCoroutine(ApplyEffect(player1CardManager.choiceCard.GetComponent<Card>().inf.effectInfs[0], clickedObjectCard, clickedObjectCard).AsCoroutine());
    }

    public async Task PlayCardChoiceEffect(EffectInf effect, Card clickedObjectCard)
    {
        await ApplyEffect(effect, clickedObjectCard, clickedObjectCard);
    }

    public IEnumerator PlayCardEffect(Card card, EffectInf effect)
    {
        if (effect.triggers.Contains(EffectInf.CardTrigger.ProtectShieldDecrease) && card.CardOwner == PlayerID.Player1)
            player1CardManager.CardsWithEffectOnField.Add(card);
        else if (effect.triggers.Contains(EffectInf.CardTrigger.ProtectShieldDecrease) && card.CardOwner == PlayerID.Player2)
            player2CardManager.CardsWithEffectOnField.Add(card);
        yield return StartCoroutine(ApplyEffect(effect, card).AsCoroutine());
    }
    public async Task AIPlayCardEffect(Card card, EffectInf effect)
    {
        if (effect.triggers.Contains(EffectInf.CardTrigger.ProtectShieldDecrease) && card.CardOwner == PlayerID.Player1)
            player1CardManager.CardsWithEffectOnField.Add(card);
        else if (effect.triggers.Contains(EffectInf.CardTrigger.ProtectShieldDecrease) && card.CardOwner == PlayerID.Player2)
            player2CardManager.CardsWithEffectOnField.Add(card);
        await ApplyEffect(effect, card);
    }

    public async Task EffectWhenAttackingLeader(Card attackCard)
    {
        await ExecuteEffects(attackCard, EffectInf.CardTrigger.OnAttack, EffectInf.CardTrigger.OnDuringAttack);
    }

    public async Task EffectWhenAttackingCard(Card attackCard, Card defenceCard)
    {
        await ExecuteEffects(attackCard, EffectInf.CardTrigger.OnAttack, EffectInf.CardTrigger.OnDuringAttack);
        await ExecuteEffects(defenceCard, EffectInf.CardTrigger.OnDefence);
    }

    public async Task EffectWhenCollectionChanged(List<Card> CardsWithEffectOnField, NotifyCollectionChangedEventArgs e)
    {
        if(CardsWithEffectOnField == null ) return;
        if (CardsWithEffectOnField.Count == 0) return;

        Card card = (Card)e.NewItems[0];
        var applyEffectArgs = GetAllFields(card);

        foreach (var effectCard in CardsWithEffectOnField)
        {
            foreach (var effect in effectCard.inf.effectInfs)
            {
                if (effect.triggers.Contains(EffectInf.CardTrigger.OnFieldOnAfterPlay) && effect is ICardEffect cardEffect)
                await cardEffect.Apply(applyEffectArgs);
            }
        }
    }

    public async Task TurnStartEffect(bool p1_turn)
    {
        if (p1_turn)
        {
            await TriggerEffectsAsync(player1CardManager.AllFields?.ToList() ?? new List<Card>(), EffectInf.CardTrigger.OnTurnStart);
            await TriggerEffectsAsync(player2CardManager.AllFields?.ToList() ?? new List<Card>(), EffectInf.CardTrigger.OnEnemyTurnStart);
        }
        else
        {
            await TriggerEffectsAsync(player2CardManager.AllFields?.ToList() ?? new List<Card>(), EffectInf.CardTrigger.OnTurnStart);
            await TriggerEffectsAsync(player1CardManager.AllFields?.ToList() ?? new List<Card>(), EffectInf.CardTrigger.OnEnemyTurnStart);
        }
    }

    public async Task TurnEndEffect(bool p1_turn)
    {
        if (p1_turn)
        {
            await TriggerEffectsAsync(player1CardManager.AllFields?.ToList() ?? new List<Card>(), EffectInf.CardTrigger.OnTurnEnd);
            await TriggerEffectsAsync(player2CardManager.AllFields?.ToList() ?? new List<Card>(), EffectInf.CardTrigger.OnEnemyTurnEnd);
            await TriggerEffectsAsync(player2CardManager.SpelEffectAfterSomeTurn, EffectInf.CardTrigger.SpelEffectSomeTurn);
        }
        else
        {
            await TriggerEffectsAsync(player2CardManager.AllFields?.ToList() ?? new List<Card>(), EffectInf.CardTrigger.OnTurnEnd);
            await TriggerEffectsAsync(player1CardManager.AllFields?.ToList() ?? new List<Card>(), EffectInf.CardTrigger.OnEnemyTurnEnd);
            await TriggerEffectsAsync(player1CardManager.SpelEffectAfterSomeTurn, EffectInf.CardTrigger.SpelEffectSomeTurn);
        }
    }

    public async Task TriggerEffectsAsync(List<Card> cards, EffectInf.CardTrigger trigger)
    {
        if(cards != null)
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            for (int j = 0; j < card.inf.effectInfs.Count; j++)
            {
                var effect = card.inf.effectInfs[j];
                if (effect.triggers.Contains(trigger) && effect is ICardEffect cardEffect)
                {
                    await cardEffect.Apply(new ApplyEffectEventArgs(card, player2CardManager.AllFields, player2CardManager.AttackFields, player2CardManager.DefenceFields, player1CardManager.AllFields, player1CardManager.AttackFields, player1CardManager.DefenceFields));
                }
            }
        }
    }

    private async Task ApplyEffect(EffectInf effect, Card card, Card clickedObjectCard = null)
    {
        if (effect is ICardEffect cardEffect)
        {
            Task task = cardEffect.Apply(GetAllFields(card, clickedObjectCard));
            await task;
        }
    }

    private async Task ExecuteEffects(Card card, params EffectInf.CardTrigger[] triggers)
    {
        if(card != null)
        foreach (var effectInf in card.inf.effectInfs)
        {
            foreach (var trigger in triggers)
            {
                if (effectInf.triggers.Contains(trigger) && effectInf is ICardEffect cardEffect)
                {
                    await cardEffect.Apply(GetAllFields(card));
                }
            }
        }
    }

    private ApplyEffectEventArgs GetAllFields(Card card, Card clickedObjectCard = null)
    {
        return new ApplyEffectEventArgs(
            card, player2CardManager.AllFields, player2CardManager.AttackFields, player2CardManager.DefenceFields,
            player1CardManager.AllFields, player1CardManager.AttackFields, player1CardManager.DefenceFields, clickedObjectCard);
    }
}
