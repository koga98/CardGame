using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using GameNamespace;
using System.Threading.Tasks;


[CreateAssetMenu(fileName = "New Draw Card", menuName = "Effect/Draw")]
public class DrawCard : EffectInf, ICardEffect
{
    public int drawAmount;
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;
    public bool ApplyToMyself;

    public override async Task Apply(ApplyEffectEventArgs e)
{
    if (conditionOnEffects.Count == 0 || AreConditionsMet(conditionOnEffects, e))
    {
        await ApplyDrawCardEffect(e);
    }

    if (additionalEffects.Count > 0 && AreConditionsMet(conditionOnAdditionalEffects, e))
    {
        foreach (var additionalEffect in additionalEffects)
        {
            await additionalEffect.Apply(e);
        }
    }
}

private async Task ApplyDrawCardEffect(ApplyEffectEventArgs e)
{
    if (e.Card.CardOwner == PlayerID.Player1)
    {
        if (ApplyToMyself)
        {
            await effectMethod.P1DrawCard(e, drawAmount, this);
        }
        else
        {
            await effectMethod.P2DrawCard(e, drawAmount, this);
        }
    }
    else if (e.Card.CardOwner == PlayerID.Player2)
    {
        if (ApplyToMyself)
        {
            await effectMethod.P2DrawCard(e, drawAmount, this);
        }
        else
        {
            await effectMethod.P1DrawCard(e, drawAmount, this);
        }
    }
}

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
      await Task.CompletedTask;
    }
}
