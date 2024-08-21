using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using GameNamespace;
using System.Threading.Tasks;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "New Draw Aoe", menuName = "Effect/AoeDestroy")]
public class AoeDestroy : EffectInf
{
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;
    public bool ApplyToMyself;
    public bool IsConditionClear;
    public TargetType targetFieldType;

    public override async Task Apply(ApplyEffectEventArgs e)
    {
        bool isPlayer1 = e.Card.CardOwner == PlayerID.Player1;
        bool isApplyToMyself = ApplyToMyself;

        Func<ApplyEffectEventArgs, TargetType, AoeDestroy ,Task> destroyAllCardMethod = isPlayer1
            ? (isApplyToMyself ? effectMethod.P1DestroyAllCard : effectMethod.P2DestroyAllCard)
            : (isApplyToMyself ? effectMethod.P2DestroyAllCard : effectMethod.P1DestroyAllCard);

        // 条件に基づく効果の適用
        if (conditionOnEffects.Count == 0 || ApplyConditionEffects(conditionOnEffects, e))
        {
            await destroyAllCardMethod(e, targetFieldType, this);
        }

        // 追加効果の適用
        if (additionalEffects.Count > 0 && ApplyConditionEffects(conditionOnAdditionalEffects, e))
        {
            foreach (var additionalEffect in additionalEffects)
            {
                await additionalEffect.Apply(e);
            }
        }
    }

    private bool ApplyConditionEffects(IEnumerable<ConditionEffectsInf> conditions, ApplyEffectEventArgs e)
    {
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
        GameObject objectenemyAI = GameObject.Find("EnemyAI");
        EnemyAI enemyAI = objectenemyAI.GetComponent<EnemyAI>();

        Transform attackFieldTransform;
        Transform defenceFieldTransform;

        // プレイヤーIDとApplyToMyselfによってどのフィールドを使うか決定
        (attackFieldTransform, defenceFieldTransform) = GetFieldTransforms(e.Card.CardOwner, ApplyToMyself, enemyAI);

        // エフェクトを適用するための処理
        if (targetFieldType == TargetType.All)
        {
            if (attackFieldTransform != null)
            {
                await ApplyEffectAsync(gameManager.aoeEffectPrefab, attackFieldTransform);
            }
            if (defenceFieldTransform != null)
            {
                await ApplyEffectAsync(gameManager.aoeEffectPrefab, defenceFieldTransform);
            }
        }
        else if (targetFieldType == TargetType.Attack)
        {
            if (attackFieldTransform != null)
            {
                await ApplyEffectAsync(gameManager.aoeEffectPrefab, attackFieldTransform);
            }
        }
        else if (targetFieldType == TargetType.Defence)
        {
            if (defenceFieldTransform != null)
            {
                await ApplyEffectAsync(gameManager.aoeEffectPrefab, defenceFieldTransform);
            }
        }
    }

    private (Transform attackField, Transform defenceField) GetFieldTransforms(PlayerID cardOwner, bool applyToMyself, EnemyAI enemyAI)
    {
        if (cardOwner == PlayerID.Player1)
        {
            if (applyToMyself)
            {
                return (GameObject.Find("myAttackField").transform, GameObject.Find("myDefenceField").transform);
            }
            else
            {
                return (enemyAI.enemyAttackField.transform, enemyAI.enemyDefenceField.transform);
            }
        }
        else // PlayerID.Player2
        {
            if (applyToMyself)
            {
                return (enemyAI.enemyAttackField.transform, enemyAI.enemyDefenceField.transform);
            }
            else
            {
                return (GameObject.Find("myAttackField").transform, GameObject.Find("myDefenceField").transform);
            }
        }
    }

    private async Task ApplyEffectAsync(GameObject effectPrefab, Transform fieldTransform)
    {
        fieldTransform.GetComponent<HorizontalLayoutGroup>().enabled = false;
        GameObject effect = Instantiate(effectPrefab, fieldTransform);
        Animator effectAnimator = effect.GetComponent<Animator>();
        effectAnimator.Play(animationClip.name);
        AudioManager.Instance.EffectSound(audioClip);
        await WaitForAnimationAsync(effectAnimator, animationClip.name);
        Destroy(effect);
        fieldTransform.GetComponent<HorizontalLayoutGroup>().enabled = true;
    }

    private async Task WaitForAnimationAsync(Animator animator, string animationName)
    {
        while (true)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName(animationName) && stateInfo.normalizedTime >= 1.0f)
            {
                break; 
            }
            await Task.Yield(); 
        }
    }
}

