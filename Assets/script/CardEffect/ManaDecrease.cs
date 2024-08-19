using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "New Mana Decrease", menuName = "Effect/ManaDecrease")]
public class ManaDecrease : EffectInf
{
    public int decreaseAmount;
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;
    public bool ApplyToMyself;
    public bool IsConditionClear;

    public override Task Apply(ApplyEffectEventArgs e)
    {
        async void ApplyManaDecrease()
        {
            if (e.Card.CardOwner == PlayerID.Player1)
            {
                if (ApplyToMyself)
                {
                    await effectMethod.P1ManaDecrease(e, decreaseAmount, this);
                }
                else
                {
                    await effectMethod.P2ManaDecrease(e, decreaseAmount, this);
                }
            }
            else if (e.Card.CardOwner == PlayerID.Player2)
            {
                if (ApplyToMyself)
                {
                    await effectMethod.P2ManaDecrease(e, decreaseAmount, this);
                }
                else
                {
                    await effectMethod.P1ManaDecrease(e, decreaseAmount, this);
                }
            }
        }

        if (AreConditionsMet(conditionOnEffects, e))
        {
            ApplyManaDecrease();
        }

        if (additionalEffects.Count > 0 && AreConditionsMet(conditionOnAdditionalEffects, e))
        {
            foreach (var additionalEffect in additionalEffects)
            {
                additionalEffect.Apply(e);
            }
        }

        return Task.CompletedTask;
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


    public override async Task EffectOfEffect(ApplyEffectEventArgs e)
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();

        async Task PlayAnimationOnLeader(Leader leader)
        {
            GameObject attackEffect = Instantiate(gameManager.buffEffectPrefab, leader.gameObject.transform);
            Animator attackEffectAnimator = attackEffect.GetComponent<Animator>();
            attackEffectAnimator.Play(animationClip.name);
            AudioManager.Instance.EffectSound(audioClip);

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

        if (conditionOnEffects.Count == 0 || IsConditionClear)
        {
            Leader leader = GetLeader(e.Card.CardOwner, ApplyToMyself);
            await PlayAnimationOnLeader(leader);
        }
    }
}
