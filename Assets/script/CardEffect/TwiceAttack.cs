using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "New TwiceAttack OnCondition Card", menuName = "Effect/TwiceAttack")]
public class TwiceAttack : EffectInf, ICardEffect
{
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;

    public override async Task Apply(ApplyEffectEventArgs e)
    {
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

            return true;
        }

        if (conditionOnEffects.Count == 0 || CheckConditionsAndApplyEffects(conditionOnEffects, new List<EffectInf>()))
        {
            await effectMethod.CanAttackTwice(e, this);
        }

        if (additionalEffects.Count > 0)
        {
            CheckConditionsAndApplyEffects(conditionOnAdditionalEffects, additionalEffects);
        }
    }

    public override async Task EffectOfEffect(ApplyEffectEventArgs e)
    {
        AudioManager.Instance.EffectSound(audioClip);
        await Task.CompletedTask;
    }
}
