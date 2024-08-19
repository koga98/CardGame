using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System;
using System.Threading.Tasks;


[CreateAssetMenu(fileName = "New Protect Shield DamageCut", menuName = "Effect/ProtectShieldDamageCut")]
public class ProtectShieldDamageCut : EffectInf
{
    public double damageCutRatio;
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;
    public bool ApplyToMyself;

    public override async Task Apply(ApplyEffectEventArgs e)
    {
        // Determine if conditions are met for applying effects
        bool conditionsMet = AreConditionsMet(conditionOnEffects, e);

        if (conditionsMet)
        {
            await ApplyShieldDamageCut(e);
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

    // Helper method to apply shield damage cut effect based on player and target
    private async Task ApplyShieldDamageCut(ApplyEffectEventArgs e)
    {
        if (e.Card.CardOwner == PlayerID.Player1)
        {
            if (ApplyToMyself)
            {
                await effectMethod.P1ShieldDamageCut(e, damageCutRatio, this);
            }
            else
            {
                await effectMethod.P2ShieldDamageCut(e, damageCutRatio, this);
            }
        }
        else if (e.Card.CardOwner == PlayerID.Player2)
        {
            if (ApplyToMyself)
            {
                await effectMethod.P2ShieldDamageCut(e, damageCutRatio, this);
            }
            else
            {
                await effectMethod.P1ShieldDamageCut(e, damageCutRatio, this);
            }
        }
    }

    public override async Task EffectOfEffect(ApplyEffectEventArgs e)
    {
        AudioManager.Instance.EffectSound(audioClip);
        await Task.CompletedTask;
    }

}
