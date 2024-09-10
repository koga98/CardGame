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

    public async Task EffectDuringBattleEndDeal()
    {
        await ProcessEffectEndAsync(player1CardManager.EffectDuringAttacking);
        await ProcessEffectEndAsync(player2CardManager.EffectDuringAttacking);
    }

    private async Task ProcessEffectEndAsync(List<Card> effectList)
    {
        if (effectList.Count == 0 || effectList[0] == null) return;

        var currentCard = effectList[0];

        foreach (var effectInf in currentCard.inf.effectInfs)
        {
            foreach (EffectInf.CardTrigger cardTrigger in effectInf.triggers)
            {
                if (cardTrigger == EffectInf.CardTrigger.EndAttack && effectInf is ICardEffect cardEffect)
                {
                    await CoroutineTaskAdapterAsync(cardEffect, currentCard);
                }
            }
        }

        effectList[0] = null;
    }

    public async Task EffectAfterDie(Card card)
    {
        var attackValidTriggers = new List<EffectInf.CardTrigger>
    {
        EffectInf.CardTrigger.AfterDie,
        EffectInf.CardTrigger.FromPlayToDie
    };

        await ApplyCardEffectsAsync(card, attackValidTriggers, async (cardEffect) =>
        {
            await CoroutineTaskAdapterAsync(cardEffect, card);
        });

    }

    public async Task OnProtectShieldChangedAsync(PlayerType playerType)
    {
        if (playerType == PlayerType.Player1)
        {
            await ProtectShieldEffectAsync(player1CardManager.CardsWithEffectOnField);
        }
        else
        {
            await ProtectShieldEffectAsync(player2CardManager.CardsWithEffectOnField);
        }
    }

    private async Task ProtectShieldEffectAsync(List<Card> effectList)
    {
        for (int i = effectList.Count - 1; i >= 0; i--)
        {
            var card = effectList[i];

            for (int j = card.inf.effectInfs.Count - 1; j >= 0; j--)
            {
                var effectInf = card.inf.effectInfs[j];

                for (int k = effectInf.triggers.Count - 1; k >= 0; k--)
                {
                    var cardTrigger = effectInf.triggers[k];

                    if (cardTrigger == EffectInf.CardTrigger.AfterPlayAndProtectShieldDecrease)
                    {
                        if (effectInf is ICardEffect cardEffect)
                        {
                            await CoroutineTaskAdapterAsync(cardEffect, card);
                        }
                    }
                }
            }
        }
    }

    public async Task OnBeginDragEffect(Card card)
    {
        var validTriggers = new List<EffectInf.CardTrigger>
    {
        EffectInf.CardTrigger.OnBeginDrag,
    };

        await ApplyCardEffectsAsync(card, validTriggers, async (cardEffect) =>
        {
            await cardEffect.Apply(new ApplyEffectEventArgs(
                card,
                player2CardManager.AllFields,
                player2CardManager.AttackFields,
                player2CardManager.DefenceFields,
                player1CardManager.AllFields,
                player1CardManager.AttackFields,
                player1CardManager.DefenceFields));
        });
    }

    public IEnumerator HandleButtonOperation(Card card, EffectInf effect)
    {
        if (effect is ICardEffect buttonEffect)
        {
            yield return StartCoroutine(ApplyEffectCoroutine(buttonEffect,
                new ApplyEffectEventArgs(card, player2CardManager.AllFields, player2CardManager.AttackFields, player2CardManager.DefenceFields,
                player1CardManager.AllFields, player1CardManager.AttackFields, player1CardManager.DefenceFields)));
        }
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
                    {
                        if (effect is ICardEffect cardEffect)
                        {
                            // 非同期処理を待機するためにコルーチンに変更
                            yield return ApplyEffectAsync(cardEffect, card);
                        }
                    }
                }
            }

            GameManager.completeButtonChoice = false;

            if (card.inf.cardType == CardType.Spel)
            {
                Destroy(card.gameObject);
            }
        }
    }

    public IEnumerator HandleRandomCardEffect(RandomCardInf randomCardInf, Card card)
    {
        int randomValue = UnityEngine.Random.Range(0, randomCardInf.effectInfs.Count);
        yield return StartCoroutine(ApplyEffectCoroutine(randomCardInf.effectInfs[randomValue],
            new ApplyEffectEventArgs(card, player2CardManager.AllFields, player2CardManager.AttackFields, player2CardManager.DefenceFields,
            player1CardManager.AllFields, player1CardManager.AttackFields, player1CardManager.DefenceFields)));
        Destroy(card.gameObject);
    }

    public IEnumerator HandleEffect(Card card, EffectInf effect)
    {
        if (effect is ICardEffect specificEffect)
        {
            yield return StartCoroutine(ApplyEffectCoroutine(specificEffect,
                new ApplyEffectEventArgs(card, player2CardManager.AllFields, player2CardManager.AttackFields, player2CardManager.DefenceFields,
                player1CardManager.AllFields, player1CardManager.AttackFields, player1CardManager.DefenceFields)));
        }

        if (effect.triggers.Contains(EffectInf.CardTrigger.OnFieldOnAfterPlay) ||
            effect.triggers.Contains(EffectInf.CardTrigger.AfterPlayAndProtectShieldDecrease))
        {
            player1CardManager.CardsWithEffectOnField.Add(card);
        }
    }

    private async Task CoroutineTaskAdapterAsync(ICardEffect cardEffect, Card card)
    {
        Task applyEffectTask = cardEffect.Apply(new ApplyEffectEventArgs(card, player2CardManager.AllFields, player2CardManager.AttackFields, player2CardManager.DefenceFields
            , player1CardManager.AllFields, player1CardManager.AttackFields, player1CardManager.DefenceFields));
        await applyEffectTask;
    }

    public async Task EffectWhenAttackingAsync(Card attackCard)
    {
        var validTriggers = new List<EffectInf.CardTrigger>
    {
        EffectInf.CardTrigger.OnAttack,
        EffectInf.CardTrigger.OnDuringAttack,
        EffectInf.CardTrigger.OnAttackLeader
    };

        await ApplyCardEffectsAsync(attackCard, validTriggers, async (cardEffect) =>
        {
            await cardEffect.Apply(new ApplyEffectEventArgs(
                attackCard,
                player2CardManager.AllFields,
                player2CardManager.AttackFields,
                player2CardManager.DefenceFields,
                player1CardManager.AllFields,
                player1CardManager.AttackFields,
                player1CardManager.DefenceFields));
        });
    }

    public async Task AttackingEffectDealAsync(Card attackCard, Card defenceCard)
    {
        var attackValidTriggers = new List<EffectInf.CardTrigger>
    {
        EffectInf.CardTrigger.OnAttack,
        EffectInf.CardTrigger.OnDuringAttack
    };

        await ApplyCardEffectsAsync(attackCard, attackValidTriggers, async (cardEffect) =>
        {
            await CoroutineTaskAdapterAsync(cardEffect, attackCard);
        });

        var defenceValidTriggers = new List<EffectInf.CardTrigger>
    {
        EffectInf.CardTrigger.OnDefence
    };

        await ApplyCardEffectsAsync(defenceCard, defenceValidTriggers, async (cardEffect) =>
        {
            await CoroutineTaskAdapterAsync(cardEffect, defenceCard);
        });
    }

    public async Task EffectWhenCollectionChanged(List<Card> CardsWithEffectOnField, NotifyCollectionChangedEventArgs e)
    {
        if (CardsWithEffectOnField.Count != 0)
        {
            Card card = (Card)e.NewItems[0];
            foreach (var effectCard in CardsWithEffectOnField)
            {
                foreach (var effect in effectCard.inf.effectInfs)
                {
                    for (int i = 0; i < effect.triggers.Count; i++)
                    {
                        if (effect.triggers[i] == EffectInf.CardTrigger.OnFieldOnAfterPlay)
                        {
                            if (effect is ICardEffect cardEffect)
                            {
                                await cardEffect.Apply(new ApplyEffectEventArgs(card, player2CardManager.AllFields, player2CardManager.AttackFields, player2CardManager.DefenceFields,
                                player1CardManager.AllFields, player1CardManager.AttackFields, player1CardManager.DefenceFields));
                            }
                        }
                    }
                }
            }
        }
    }
    public IEnumerator ApplyEffectCoroutine(ICardEffect cardEffect, ApplyEffectEventArgs args)
    {
        Task applyEffectTask = cardEffect.Apply(args);
        // Task の完了を待機
        yield return new WaitUntil(() => applyEffectTask.IsCompleted);
    }

    private IEnumerator ApplyEffectAsync(ICardEffect cardEffect, Card card)
    {
        Task applyEffectTask = cardEffect.Apply(new ApplyEffectEventArgs(card, player2CardManager.AllFields, player2CardManager.AttackFields, player2CardManager.DefenceFields
            , player1CardManager.AllFields, player1CardManager.AttackFields, player1CardManager.DefenceFields));
        yield return new WaitUntil(() => applyEffectTask.IsCompleted);
    }

    private async Task ApplyCardEffectsAsync(Card card, IEnumerable<EffectInf.CardTrigger> validTriggers, Func<ICardEffect, Task> effectAction)
    {
        if (card.inf.effectInfs != null)
        {
            foreach (var effectInf in card.inf.effectInfs)
            {
                foreach (EffectInf.CardTrigger cardTrigger in effectInf.triggers)
                {
                    if (validTriggers.Contains(cardTrigger))
                    {
                        if (effectInf is ICardEffect cardEffect)
                        {
                            await effectAction(cardEffect);
                        }
                    }
                }
            }
        }
    }
}
