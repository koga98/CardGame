using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using GameNamespace;
using System.Threading.Tasks;
using System.Linq;

[CreateAssetMenu(fileName = "New BuffAttack FieldCards", menuName = "Effect/BuffAttackFieldCards")]
public class BuffAttackFieldCards : EffectInf
{
    public int buffAmount;
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;
    public bool ApplyToMyself;
    public bool IsConditionClear;
    public TargetType targetType;

    public override async Task Apply(ApplyEffectEventArgs e)
    {
        Func<ApplyEffectEventArgs, int, TargetType, BuffAttackFieldCards,Task> buffMethod = GetBuffMethod(e.Card.CardOwner, ApplyToMyself);

        if (AreConditionsMet(conditionOnEffects, e))
        {
           await buffMethod(e, buffAmount, targetType, this);
        }

        if (additionalEffects.Count > 0 && AreConditionsMet(conditionOnAdditionalEffects, e))
        {
            foreach (var additionalEffect in additionalEffects)
            {
                await additionalEffect.Apply(e);
            }
        }
    }

    private Func<ApplyEffectEventArgs, int, TargetType, BuffAttackFieldCards ,Task> GetBuffMethod(PlayerID cardOwner, bool applyToMyself)
    {
        if (cardOwner == PlayerID.Player1)
        {
            return applyToMyself ? effectMethod.P1BuffAttackFieldCards : effectMethod.P2BuffAttackFieldCards;
        }
        else if (cardOwner == PlayerID.Player2)
        {
            return applyToMyself ? effectMethod.P2BuffAttackFieldCards : effectMethod.P1BuffAttackFieldCards;
        }
        throw new InvalidOperationException("Invalid card owner.");
    }

    private bool AreConditionsMet(List<ConditionEffectsInf> conditions, ApplyEffectEventArgs e)
    {
        return conditions.Count == 0 || conditions.All(condition => condition.ApplyEffect(e));
    }
    public override async Task EffectOfEffect(ApplyEffectEventArgs e)
    {
        AudioManager.Instance.EffectSound(audioClip);
        await Task.CompletedTask;
    }
}
