using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using GameNamespace;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "New Random Destroy Card", menuName = "Effect/RandomDestroy")]
public class RandomDestroyCard : EffectInf, ICardEffect
{
    public int destroyAmount;
    public TargetType targetType;
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;
    public bool ApplyToMyself;

    public override async Task Apply(ApplyEffectEventArgs e)
    {
        if (AreConditionsMet(conditionOnEffects, e))
        {
            await ApplyRandomDestroyCard(e);
        }

        if (additionalEffects.Count > 0 && AreConditionsMet(conditionOnAdditionalEffects, e))
        {
            foreach (var additionalEffect in additionalEffects)
            {
                await additionalEffect.Apply(e);
            }
        }
    }

    private bool AreConditionsMet(List<ConditionEffectsInf> conditions, ApplyEffectEventArgs e)
    {
        if (conditions.Count == 0) return true;

        foreach (var condition in conditions)
        {
            if (!condition.ApplyEffect(e))
            {
                return false;
            }
        }
        return true;
    }

    private async Task ApplyRandomDestroyCard(ApplyEffectEventArgs e)
    {
        if (e.Card.CardOwner == PlayerID.Player1)
        {
            if (ApplyToMyself)
            {
                await effectMethod.P1RandomDestroyCard(e, destroyAmount, targetType, this);
            }
            else
            {
                await effectMethod.P2RandomDestroyCard(e, destroyAmount, targetType, this);
            }
        }
        else if (e.Card.CardOwner == PlayerID.Player2)
        {
            if (ApplyToMyself)
            {
                await effectMethod.P2RandomDestroyCard(e, destroyAmount, targetType, this);
            }
            else
            {
                await effectMethod.P1RandomDestroyCard(e, destroyAmount, targetType, this);
            }
        }
    }
    public override async Task EffectOfEffect(ApplyEffectEventArgs e)
    {
        GameObject manager = GameObject.Find("GameManager");
        EffectAnimationManager effectAnimationManager = manager.GetComponent<EffectAnimationManager>();
        EffectManager effectManager = manager.GetComponent<EffectManager>();

        GameObject attackEffect = Instantiate(effectAnimationManager.animationPrefab, e.ChoiceCard.gameObject.transform);
        Animator attackEffectAnimator = attackEffect.GetComponent<Animator>();
        attackEffectAnimator.Play(animationClip.name);

        // サウンド再生
        AudioManager.Instance.EffectSound(audioClip);

        // アニメーションの完了を非同期で待機
        await WaitForAnimationAsync(attackEffectAnimator, animationClip.name);

        Destroy(attackEffect);
    }

    private async Task WaitForAnimationAsync(Animator animator, string animationName)
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
}
