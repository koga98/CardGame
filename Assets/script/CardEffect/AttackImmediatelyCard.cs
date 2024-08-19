using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Linq;

[CreateAssetMenu(fileName = "New Attack Immediately Effect", menuName = "Effect/AttackImmediately")]
public class AttackImmediatelyCard : EffectInf, ICardEffect
{
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;
    public bool IsConditionClear;
    public override async Task Apply(ApplyEffectEventArgs e)
    {
        if (AreConditionsMet(conditionOnEffects, e))
        {
            await effectMethod.AttackImmediately(e, this);
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
        return conditions.Count == 0 || conditions.All(condition => condition.ApplyEffect(e));
    }

    public override async Task EffectOfEffect(ApplyEffectEventArgs e)
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        GameObject objectenemyAI = GameObject.Find("EnemyAI");
        EnemyAI enemyAI = objectenemyAI.GetComponent<EnemyAI>();

        GameObject attackEffect = Instantiate(gameManager.buffEffectPrefab, e.Card.gameObject.transform);
        Animator attackEffectAnimator = attackEffect.GetComponent<Animator>();
        attackEffectAnimator.Play(animationClip.name);

        AudioManager.Instance.EffectSound(audioClip);

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
