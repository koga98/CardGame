using System.Collections.Generic;
using UnityEngine;
using GameNamespace;
using System.Threading.Tasks;
using System.Linq;

[CreateAssetMenu(fileName = "New Attack BuffCard", menuName = "Effect/AttackBuffCard")]
public class AttackBuffCard : EffectInf, ICardEffect
{
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;
    public bool IsConditionClear;
    public int buffAmount;
    public TargetType targetType;

    public override async Task Apply(ApplyEffectEventArgs e)
    {
        if (ApplyConditionEffects(conditionOnEffects, e))
        {
            if (triggers[0] == CardTrigger.OnDuringAttack)
            {
                if (e.Card.CardOwner == PlayerID.Player1)
                {
                    Debug.Log("ばぐってる");
                    CardManager.P1EffectDuringAttacking.Add(e.Card);
                }
                else if (e.Card.CardOwner == PlayerID.Player2)
                {
                    CardManager.P2EffectDuringAttacking.Add(e.Card);
                }
            }
            await effectMethod.BuffMyCard(e, buffAmount, targetType, this);
        }

        if (additionalEffects.Count > 0 && ApplyConditionEffects(conditionOnAdditionalEffects, e))
        {
            foreach (var additionalEffect in additionalEffects)
            {
                await additionalEffect.Apply(e);
            }
        }
    }

    private bool ApplyConditionEffects(List<ConditionEffectsInf> conditions, ApplyEffectEventArgs e)
    {
        IsConditionClear = conditions.Count == 0 || conditions.All(condition => condition.ApplyEffect(e));
        return IsConditionClear;
    }


    public override async Task EffectOfEffect(ApplyEffectEventArgs e)
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();

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
