using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "New ProtectShieldDamageByRatio Effect", menuName = "Effect/ProtectShieldDamageByRatio")]
public class ProtectShieldDamageByRatio : EffectInf
{
    public double ratio;
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;
    public bool ApplyToMyself;

    public override async Task Apply(ApplyEffectEventArgs e)
    {
        // Check if conditions are met for applying effects
        bool conditionsMet = AreConditionsMet(conditionOnEffects, e);

        if (conditionsMet)
        {
            ApplyDamageEffect(e);
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

    // Helper method to apply damage effects based on player and target
    private async void ApplyDamageEffect(ApplyEffectEventArgs e)
    {
        if (e.Card.CardOwner == PlayerID.Player1)
        {
            if (ApplyToMyself)
            {
                await effectMethod.DamageP1ProtectShieldByRatio(e, ratio, this);
            }
            else
            {
                await effectMethod.DamageP2ProtectShieldByRatio(e, ratio, this);
            }
        }
        else if (e.Card.CardOwner == PlayerID.Player2)
        {
            if (ApplyToMyself)
            {
                await effectMethod.DamageP2ProtectShieldByRatio(e, ratio, this);
            }
            else
            {
                await effectMethod.DamageP1ProtectShieldByRatio(e, ratio, this);
            }
        }
    }

    public override async Task EffectOfEffect(ApplyEffectEventArgs e)
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        EffectAnimationManager effectAnimationManager = manager.GetComponent<EffectAnimationManager>();

        Leader leader = null;

        if (e.Card.CardOwner == PlayerID.Player1)
        {
            leader = ApplyToMyself ? gameManager.myLeader.GetComponent<Leader>() : gameManager.enemyLeader.GetComponent<Leader>();
        }
        else if (e.Card.CardOwner == PlayerID.Player2)
        {
            leader = ApplyToMyself ? gameManager.enemyLeader.GetComponent<Leader>() : gameManager.myLeader.GetComponent<Leader>();
        }

        if (leader != null)
        {
            GameObject attackEffect = Instantiate(effectAnimationManager.animationPrefab, leader.gameObject.transform);
            Animator attackEffectAnimator = attackEffect.GetComponent<Animator>();
            attackEffectAnimator.Play(animationClip.name);
            AudioManager.Instance.EffectSound(audioClip);

            // アニメーションの完了を非同期で待機
            await WaitForAnimationAsync(attackEffectAnimator, animationClip.name);

            Destroy(attackEffect);
        }
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
