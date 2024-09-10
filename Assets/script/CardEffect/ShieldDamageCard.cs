using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "New ShieldDamage Effect", menuName = "Effect/ShieldDamage")]
public class ShieldDamageCard : EffectInf, ICardEffect
{
    public int damageAmount;
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;
    public bool ApplyToMyself;
    public bool IsConditionClear;

    public override async Task Apply(ApplyEffectEventArgs e)
    {
        IsConditionClear = false;

        // Helper method to apply damage shield based on player and target
        async Task ApplyDamageShield(PlayerID owner, bool applyToSelf)
        {
            if (owner == PlayerID.Player1)
            {
                if (applyToSelf)
                    await effectMethod.P1DamageShield(e, damageAmount, this);
                else
                    await effectMethod.P2DamageShield(e, damageAmount, this);
            }
            else if (owner == PlayerID.Player2)
            {
                if (applyToSelf)
                    await effectMethod.P2DamageShield(e, damageAmount, this);
                else
                    await effectMethod.P1DamageShield(e, damageAmount, this);
            }
        }

        // Helper method to check conditions and apply effects
        bool CheckConditionsAndApplyEffects(List<ConditionEffectsInf> conditions, List<EffectInf> effects)
        {
            foreach (var condition in conditions)
            {
                if (!condition.ApplyEffect(e))
                    return false;
            }

            foreach (var effect in effects)
            {
                effect.Apply(e);
            }
            IsConditionClear = true;
            return true;
        }

        // Apply main effect if conditions are met or there are no conditions
        if (conditionOnEffects.Count == 0 || CheckConditionsAndApplyEffects(conditionOnEffects, new List<EffectInf>()))
        {
            await ApplyDamageShield(e.Card.CardOwner, ApplyToMyself);
        }

        // Apply additional effects if conditions are met
        if (additionalEffects.Count > 0)
        {
            CheckConditionsAndApplyEffects(conditionOnAdditionalEffects, additionalEffects);
        }
    }

    public override async Task EffectOfEffect(ApplyEffectEventArgs e)
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        EffectAnimationManager effectAnimationManager = manager.GetComponent<EffectAnimationManager>();

        // アニメーション再生を共通化
        async Task PlayAnimationOnLeader(Leader leader)
        {
            GameObject attackEffect = Instantiate(effectAnimationManager.animationPrefab, leader.gameObject.transform);
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
