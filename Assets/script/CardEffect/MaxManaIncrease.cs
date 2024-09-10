using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GameNamespace;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "New Max Mana Increase", menuName = "Effect/MaxManaIncrease")]
public class MaxManaIncrease : EffectInf
{
    public int increaseAmount;
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;
    public bool ApplyToMyself;
    public bool IsConditionClear;
    public override async Task Apply(ApplyEffectEventArgs e)
    {
        // Apply the max mana increase effect based on conditions
        if (AreConditionsMet(conditionOnEffects, e))
        {
            ApplyMaxManaIncrease(e);
        }

        // Apply additional effects if conditions are met
        if (additionalEffects.Count > 0 && AreConditionsMet(conditionOnAdditionalEffects, e))
        {
            foreach (var additionalEffect in additionalEffects)
            {
                await additionalEffect.Apply(e);
            }
        }
    }

    // Helper method to check if all conditions are met
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

    // Helper method to apply the max mana increase effect
    private async void ApplyMaxManaIncrease(ApplyEffectEventArgs e)
    {
        if (e.Card.CardOwner == PlayerID.Player1)
        {
            if (ApplyToMyself)
            {
                await effectMethod.P1MaxManaIncrease(e, increaseAmount, this);
            }
            else
            {
                await effectMethod.P2MaxManaIncrease(e, increaseAmount, this);
            }
        }
        else if (e.Card.CardOwner == PlayerID.Player2)
        {
            if (ApplyToMyself)
            {
                await effectMethod.P2MaxManaIncrease(e, increaseAmount, this);
            }
            else
            {
                await effectMethod.P1MaxManaIncrease(e, increaseAmount, this);
            }
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
            GameObject attackEffect = Instantiate(effectAnimationManager.buffEffectPrefab, leader.gameObject.transform);
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
