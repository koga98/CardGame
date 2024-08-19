using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "New CheckTopCard Effect", menuName = "Effect/CheckTopCard")]
public class CheckTopCard : EffectInf
{
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;
    public bool ApplyToMyself;
    public override Task Apply(ApplyEffectEventArgs e)
    {
        if (AreConditionsMet(conditionOnEffects, e))
        {
            HandleTopCardEffect(e);
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

    private void HandleTopCardEffect(ApplyEffectEventArgs e)
    {
        if (e.Card.CardOwner == PlayerID.Player1)
        {
            if (ApplyToMyself)
            {
                effectMethod.P1CheckTopCard();
            }
            else
            {
                effectMethod.P2CheckTopCard();
            }
        }
        else if (e.Card.CardOwner == PlayerID.Player2)
        {
            if (ApplyToMyself)
            {
                effectMethod.P2CheckTopCard();
            }
            else
            {
                effectMethod.P1CheckTopCard();
            }
        }
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
