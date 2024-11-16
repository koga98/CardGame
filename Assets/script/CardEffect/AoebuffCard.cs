using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameNamespace;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Aoebuff Card", menuName = "Effect/AoebuffCard")]
public class AoebuffCard : EffectInf
{
    public int buffAmount;
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;
    public bool ApplyToMyself;
    public bool IsConditionClear;
    public string target;
    public TargetType targetBuffType;
    public TargetType targetFieldType;

    public override async Task Apply(ApplyEffectEventArgs e)
    {
        PlayerID targetOwner = ApplyToMyself ? e.Card.CardOwner : (e.Card.CardOwner == PlayerID.Player1 ? PlayerID.Player2 : PlayerID.Player1);

        bool mainConditionClear = conditionOnEffects.Count == 0 || conditionOnEffects.All(condition => condition.ApplyEffect(e));

        if (mainConditionClear)
        {
            await ApplyBuff(e, targetOwner);
        }

        bool additionalConditionClear = additionalEffects.Count > 0 && (conditionOnAdditionalEffects.Count == 0 || conditionOnAdditionalEffects.All(condition => condition.ApplyEffect(e)));

        if (additionalConditionClear)
        {
            await ApplyAdditionalEffects(e);
        }
    }

    private async Task ApplyBuff(ApplyEffectEventArgs e, PlayerID targetOwner)
    {
        if (targetOwner == PlayerID.Player1)
        {
            await effectMethod.P1AoeBuffCard(e, targetFieldType, targetBuffType, buffAmount, target, this);
        }
        else if (targetOwner == PlayerID.Player2)
        {
            await effectMethod.P2AoeBuffCard(e, targetFieldType, targetBuffType, buffAmount, target, this);
        }
    }

    private async Task ApplyAdditionalEffects(ApplyEffectEventArgs e)
    {
        foreach (var additionalEffect in additionalEffects)
        {
            await additionalEffect.Apply(e);
        }
    }

    public override async Task EffectOfEffect(ApplyEffectEventArgs e)
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        EffectAnimationManager effectAnimationManager = manager.GetComponent<EffectAnimationManager>();

        async Task PlayAnimationOnCard(Card card)
        {
            GameObject attackEffect = Instantiate(effectAnimationManager.buffEffectPrefab, card.gameObject.transform);
            Animator attackEffectAnimator = attackEffect.GetComponent<Animator>();
            attackEffectAnimator.Play(animationClip.name);
            AudioManager.Instance.EffectSound(audioClip);

            await WaitForAnimation(attackEffectAnimator, animationClip.name);

            Destroy(attackEffect);
        }

        async Task WaitForAnimation(Animator animator, string animationName)
        {
            while (true)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName(animationName) && stateInfo.normalizedTime >= 1.0f)
                {
                    break;
                }
                await Task.Yield(); // 次のフレームまで待機
            }
        }

        List<Card> GetTargetNameCards(List<Card> fieldCards)
        {
            return fieldCards.Where(card => card.inf.cardName == target).ToList();
        }

        List<Card> GetTargetFieldCards(ApplyEffectEventArgs e)
        {
            if (string.IsNullOrEmpty(target))
            {
                if (e.Card.CardOwner == PlayerID.Player1)
                {
                    return ApplyToMyself ? targetFieldType switch
                    {
                        TargetType.All => e.PCards.ToList(),
                        TargetType.Attack => e.PAttackCards,
                        TargetType.Defence => e.PDefenceCards,
                        _ => new List<Card>()
                    } : targetFieldType switch
                    {
                        TargetType.All => e.Cards.ToList(),
                        TargetType.Attack => e.EAttackCards,
                        TargetType.Defence => e.EDefenceCards,
                        _ => new List<Card>()
                    };
                }
                else if (e.Card.CardOwner == PlayerID.Player2)
                {
                    return ApplyToMyself ? targetFieldType switch
                    {
                        TargetType.All => e.Cards.ToList(),
                        TargetType.Attack => e.EAttackCards.ToList(),
                        TargetType.Defence => e.EDefenceCards.ToList(),
                        _ => new List<Card>()
                    } : targetFieldType switch
                    {
                        TargetType.All => e.PCards.ToList(),
                        TargetType.Attack => e.PAttackCards.ToList(),
                        TargetType.Defence => e.PDefenceCards.ToList(),
                        _ => new List<Card>()
                    };
                }
            }
            else
            {
                if (e.Card.CardOwner == PlayerID.Player1)
                {
                    return ApplyToMyself ? targetFieldType switch
                    {
                        TargetType.All => GetTargetNameCards(e.PCards.ToList()),
                        TargetType.Attack => GetTargetNameCards(e.PAttackCards.ToList()),
                        TargetType.Defence => GetTargetNameCards(e.PDefenceCards.ToList()),
                        _ => new List<Card>()
                    } : targetFieldType switch
                    {
                        TargetType.All => GetTargetNameCards(e.Cards.ToList()),
                        TargetType.Attack => GetTargetNameCards(e.EAttackCards.ToList()),
                        TargetType.Defence => GetTargetNameCards(e.EDefenceCards.ToList()),
                        _ => new List<Card>()
                    };
                }
                else if (e.Card.CardOwner == PlayerID.Player2)
                {
                    return ApplyToMyself ? targetFieldType switch
                    {
                        TargetType.All => GetTargetNameCards(e.Cards.ToList()),
                        TargetType.Attack => GetTargetNameCards(e.EAttackCards.ToList()),
                        TargetType.Defence => GetTargetNameCards(e.EDefenceCards.ToList()),
                        _ => new List<Card>()
                    } : targetFieldType switch
                    {
                        TargetType.All => GetTargetNameCards(e.PCards.ToList()),
                        TargetType.Attack => GetTargetNameCards(e.PAttackCards.ToList()),
                        TargetType.Defence => GetTargetNameCards(e.PDefenceCards.ToList()),
                        _ => new List<Card>()
                    };
                }
            }
            return new List<Card>();
        }

        List<Card> targetCards = GetTargetFieldCards(e);
        foreach (var card in targetCards)
        {
            await PlayAnimationOnCard(card);
        }
    }
}