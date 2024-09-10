using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.UI;
using GameNamespace;

[CreateAssetMenu(fileName = "New Draw Aoe", menuName = "Effect/Aoe")]
public class AoeCard : EffectInf
{
    public int damageAmount;
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;
    public bool ApplyToMyself;
    public bool IsConditionClear;
    public TargetType targetFieldType;

    public override async Task Apply(ApplyEffectEventArgs e)
    {
        PlayerID targetOwner = ApplyToMyself ? e.Card.CardOwner : GetOpponentPlayer(e.Card.CardOwner);

        if (AreConditionsMet(conditionOnEffects, e))
        {
            await ApplyAoeEffect(e, targetOwner);
        }

        if (additionalEffects.Count > 0 && AreConditionsMet(conditionOnAdditionalEffects, e))
        {
            await ApplyAdditionalEffects(e);
        }
    }

    private PlayerID GetOpponentPlayer(PlayerID player)
    {
        return player == PlayerID.Player1 ? PlayerID.Player2 : PlayerID.Player1;
    }

    private async Task ApplyAoeEffect(ApplyEffectEventArgs e, PlayerID targetOwner)
    {
        if (targetOwner == PlayerID.Player1)
        {
            await effectMethod.P1Aoe(e, damageAmount, targetFieldType, this);
        }
        else if (targetOwner == PlayerID.Player2)
        {
            await effectMethod.P2Aoe(e, damageAmount, targetFieldType, this);
        }
    }

    private bool AreConditionsMet(List<ConditionEffectsInf> conditions, ApplyEffectEventArgs e)
    {
        return conditions.Count == 0 || conditions.All(condition => condition.ApplyEffect(e));
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
        EffectAnimationManager effectAnimationManager = manager.GetComponent<EffectAnimationManager>();
        GameObject objectenemyAI = GameObject.Find("EnemyAI");
        EnemyAI enemyAI = objectenemyAI.GetComponent<EnemyAI>();

        Transform attackField, defenceField;

        if (e.Card.CardOwner == PlayerID.Player1)
        {
            attackField = ApplyToMyself ? GameObject.Find("myAttackField").transform : enemyAI.enemyAttackField.transform;
            defenceField = ApplyToMyself ? GameObject.Find("myDefenceField").transform : enemyAI.enemyDefenceField.transform;
        }
        else
        {
            attackField = ApplyToMyself ? enemyAI.enemyAttackField.transform : GameObject.Find("myAttackField").transform;
            defenceField = ApplyToMyself ? enemyAI.enemyDefenceField.transform : GameObject.Find("myDefenceField").transform;
        }

        if (targetFieldType == TargetType.All || targetFieldType == TargetType.Attack)
        {
            await PlayEffect(effectAnimationManager.aoeEffectPrefab, attackField);
        }

        if (targetFieldType == TargetType.All || targetFieldType == TargetType.Defence)
        {
            await PlayEffect(effectAnimationManager.aoeEffectPrefab, defenceField);
        }
    }

    private async Task PlayEffect(GameObject effectPrefab, Transform field)
    {
        field.GetComponent<HorizontalLayoutGroup>().enabled = false;

        GameObject effectInstance = Instantiate(effectPrefab, field);
        Animator effectAnimator = effectInstance.GetComponent<Animator>();

        effectAnimator.Play(animationClip.name);

        AudioManager.Instance.EffectSound(audioClip);

        while (true)
        {
            AnimatorStateInfo stateInfo = effectAnimator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName(animationClip.name) && stateInfo.normalizedTime >= 1.0f)
            {
                break;
            }

            await Task.Yield(); // フレーム待機
        }

        Destroy(effectInstance);

        field.GetComponent<HorizontalLayoutGroup>().enabled = true;
    }
}