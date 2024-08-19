using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameNamespace;
using UnityEngine;

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
            effectMethod.P2AoeBuffCard(e, targetFieldType, targetBuffType, buffAmount, target, this);
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

        // アニメーションを再生する共通メソッド
        async Task PlayAnimationOnCard(Card card)
        {
            GameObject attackEffect = Instantiate(gameManager.buffEffectPrefab, card.gameObject.transform);
            Animator attackEffectAnimator = attackEffect.GetComponent<Animator>();
            attackEffectAnimator.Play(animationClip.name);
            AudioManager.Instance.EffectSound(audioClip);

            // アニメーションの終了を待機
            await WaitForAnimation(attackEffectAnimator, animationClip.name);

            Destroy(attackEffect);
        }

        // アニメーションの終了を待機するメソッド
        async Task WaitForAnimation(Animator animator, string animationName)
        {
            while (true)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName(animationName) && stateInfo.normalizedTime >= 1.0f)
                {
                    break; // アニメーションが終了したらループを抜ける
                }
                await Task.Yield(); // 次のフレームまで待機
            }
        }

        List<Card> GetTargetNameCards(List<Card> fieldCards)
        {
            List<Card> cards = new List<Card>();
            foreach (Card card in fieldCards)
            {
                if (card.inf.name == target)
                {
                    cards.Add(card);
                }
            }
            return cards;
        }


        // 対象となるカードのリストを取得するメソッド
        List<Card> GetTargetFieldCards(ApplyEffectEventArgs e)
        {
            if (target == null)
            {
                if (e.Card.CardOwner == PlayerID.Player1)
                {
                    if (ApplyToMyself)
                    {
                        return targetFieldType switch
                        {
                            TargetType.All => e.PCards.ToList(),
                            TargetType.Attack => e.PAttackCards.ToList(),
                            TargetType.Defence => e.PDefenceCards.ToList(),
                            _ => new List<Card>()
                        };
                    }
                    else
                    {
                        return targetFieldType switch
                        {
                            TargetType.All => e.Cards.ToList(),
                            TargetType.Attack => e.EAttackCards.ToList(),
                            TargetType.Defence => e.EDefenceCards.ToList(),
                            _ => new List<Card>()
                        };
                    }
                }
                else if (e.Card.CardOwner == PlayerID.Player2)
                {
                    if (ApplyToMyself)
                    {
                        return targetFieldType switch
                        {
                            TargetType.All => e.Cards.ToList(),
                            TargetType.Attack => e.EAttackCards.ToList(),
                            TargetType.Defence => e.EDefenceCards.ToList(),
                            _ => new List<Card>()
                        };
                    }
                    else
                    {
                        return targetFieldType switch
                        {
                            TargetType.All => e.PCards.ToList(),
                            TargetType.Attack => e.PAttackCards.ToList(),
                            TargetType.Defence => e.PDefenceCards.ToList(),
                            _ => new List<Card>()
                        };
                    }
                }
            }
            else
            {
                if (e.Card.CardOwner == PlayerID.Player1)
                {
                    if (ApplyToMyself)
                    {
                        return targetFieldType switch
                        {
                            TargetType.All => GetTargetNameCards(e.PCards.ToList()),
                            TargetType.Attack => GetTargetNameCards(e.PAttackCards.ToList()),
                            TargetType.Defence => GetTargetNameCards(e.PDefenceCards.ToList()),
                            _ => new List<Card>()
                        };
                    }
                    else
                    {
                        return targetFieldType switch
                        {
                            TargetType.All => GetTargetNameCards(e.Cards.ToList()),
                            TargetType.Attack => GetTargetNameCards(e.EAttackCards.ToList()),
                            TargetType.Defence => GetTargetNameCards(e.EDefenceCards.ToList()),
                            _ => new List<Card>()
                        };
                    }
                }
                else if (e.Card.CardOwner == PlayerID.Player2)
                {
                    if (ApplyToMyself)
                    {
                        return targetFieldType switch
                        {
                            TargetType.All => GetTargetNameCards(e.Cards.ToList()),
                            TargetType.Attack => GetTargetNameCards(e.EAttackCards.ToList()),
                            TargetType.Defence => GetTargetNameCards(e.EDefenceCards.ToList()),
                            _ => new List<Card>()
                        };
                    }
                    else
                    {
                        return targetFieldType switch
                        {
                            TargetType.All => GetTargetNameCards(e.PCards.ToList()),
                            TargetType.Attack => GetTargetNameCards(e.PAttackCards.ToList()),
                            TargetType.Defence => GetTargetNameCards(e.PDefenceCards.ToList()),
                            _ => new List<Card>()
                        };
                    }
                }
            }
            return new List<Card>(); // カードがない場合
        }

        // 取得したカードリストに対してアニメーションを適用
        List<Card> targetCards = GetTargetFieldCards(e);
        foreach (var card in targetCards)
        {
            await PlayAnimationOnCard(card);
        }
    }
}
