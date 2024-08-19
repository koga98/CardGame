using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "New CancelCheckTopCard Effect", menuName = "Effect/CancelCheckTopCard")]
public class CancelCheckTopCard : EffectInf
{
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;
    public override Task Apply(ApplyEffectEventArgs e)
    {
        if (AreConditionsMet(conditionOnEffects, e))
        {
            effectMethod.CancelCheckTopCard();
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
        AudioManager.Instance.EffectSound(audioClip);
        await Task.CompletedTask;
    }
}
