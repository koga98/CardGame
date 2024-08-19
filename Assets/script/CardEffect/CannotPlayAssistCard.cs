using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Linq;

[CreateAssetMenu(fileName = "New CannotPlayAssist Card", menuName = "Effect/CannotPlayAssistCard")]
public class CannotPlayAssistCard : EffectInf, ICardEffect
{
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;
    public bool ApplyToMyself;
    public bool IsConditionClear;

    public override async Task Apply(ApplyEffectEventArgs e)
    {
        Func<ApplyEffectEventArgs, CannotPlayAssistCard,Task> effectMethodToCall = GetEffectMethod(e.Card.CardOwner);

        if (AreConditionsMet(conditionOnEffects, e))
        {
            await effectMethodToCall(e, this);
        }

        if (additionalEffects.Count > 0 && AreConditionsMet(conditionOnAdditionalEffects, e))
        {
            foreach (var additionalEffect in additionalEffects)
            {
                await additionalEffect.Apply(e);
            }
        }
    }

    private Func<ApplyEffectEventArgs, CannotPlayAssistCard,Task> GetEffectMethod(PlayerID cardOwner)
    {
        if (cardOwner == PlayerID.Player1)
        {
            return ApplyToMyself ? effectMethod.P1CannotPlayAssistCard : effectMethod.P2CannotPlayAssistCard;
        }
        else if (cardOwner == PlayerID.Player2)
        {
            return ApplyToMyself ? effectMethod.P2CannotPlayAssistCard : effectMethod.P1CannotPlayAssistCard;
        }
        else
        {
            throw new InvalidOperationException("Invalid card owner.");
        }
    }

    private bool AreConditionsMet(List<ConditionEffectsInf> conditions, ApplyEffectEventArgs e)
    {
        return conditions.Count == 0 || conditions.All(condition => condition.ApplyEffect(e));
    }
    public override async Task EffectOfEffect(ApplyEffectEventArgs e)
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();

        // アニメーション再生を共通化
        async Task PlayAnimationOnLeader(Leader leader)
        {
            GameObject attackEffect = Instantiate(gameManager.buffEffectPrefab, leader.gameObject.transform);
            Animator attackEffectAnimator = attackEffect.GetComponent<Animator>();
            attackEffectAnimator.Play(animationClip.name);
            AudioManager.Instance.EffectSound(audioClip);

            // アニメーションの終了を待つ
            await WaitForAnimation(attackEffectAnimator, animationClip.name);

            Destroy(attackEffect);
        }

        // アニメーションの終了を待つためのTask
        async Task WaitForAnimation(Animator animator, string clipName)
        {
            while (true)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName(clipName) && stateInfo.normalizedTime >= 1.0f)
                {
                    break; // アニメーションが終了したらループを抜ける
                }
                await Task.Yield(); // 次のフレームまで待機
            }
        }

        // 条件に応じたリーダーを取得
        Leader GetLeader(PlayerID owner, bool applyToSelf)
        {
            if (owner == PlayerID.Player1)
            {
                return applyToSelf ? gameManager.myLeader.GetComponent<Leader>() : gameManager.enemyLeader.GetComponent<Leader>();
            }
            else // owner == PlayerID.Player2
            {
                return applyToSelf ? gameManager.enemyLeader.GetComponent<Leader>() : gameManager.myLeader.GetComponent<Leader>();
            }
        }

        // 条件を満たしているか確認
        if (conditionOnEffects.Count == 0 || IsConditionClear)
        {
            Leader leader = GetLeader(e.Card.CardOwner, ApplyToMyself);
            await PlayAnimationOnLeader(leader);
        }
    }
}
