using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "New InstantiateToken Card", menuName = "Effect/InstantiateToken")]
public class InstantiateToken : EffectInf
{
    public CardInf tokeninf;
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;
    public bool ApplyToMyself;
    public override Task Apply(ApplyEffectEventArgs e)
    {
        // Determine the method for instantiating tokens based on conditions
        Action<CardInf> instantiateTokenMethod = GetInstantiateTokenMethod(e.Card.CardOwner);

        // Apply token instantiation if conditions are met
        if (AreConditionsMet(conditionOnEffects, e))
        {
            instantiateTokenMethod?.Invoke(tokeninf);
        }

        // Apply additional effects if conditions are met
        if (additionalEffects.Count > 0 && AreConditionsMet(conditionOnAdditionalEffects, e))
        {
            foreach (var additionalEffect in additionalEffects)
            {
                additionalEffect.Apply(e);
            }
        }

        return Task.CompletedTask;
    }

    // Helper method to determine the appropriate token instantiation method
    private Action<CardInf> GetInstantiateTokenMethod(PlayerID cardOwner)
    {
        if (cardOwner == PlayerID.Player1)
        {
            return ApplyToMyself ? effectMethod.P1InstantiateToken : effectMethod.P2InstantiateToken;
        }
        else if (cardOwner == PlayerID.Player2)
        {
            return ApplyToMyself ? effectMethod.P2InstantiateToken : effectMethod.P1InstantiateToken;
        }
        return null;
    }

    // Helper method to check if all conditions are met
    private bool AreConditionsMet(List<ConditionEffectsInf> conditions, ApplyEffectEventArgs e)
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
        AudioManager.Instance.EffectSound(audioClip);
        await Task.CompletedTask;
    }
}
