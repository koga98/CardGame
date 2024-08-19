using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Linq;

[CreateAssetMenu(fileName = "New Cannot Attack OtherCard", menuName = "Effect/CannotAttackOtherCard")]
public class CannotAttackOtherCard : EffectInf, ICardEffect
{
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;
    public override async Task Apply(ApplyEffectEventArgs e)
    {
        Func<ApplyEffectEventArgs, CannotAttackOtherCard,Task> cannotAttackMethod = GetCannotAttackMethod(e.Card.CardOwner);

        if (AreConditionsMet(conditionOnEffects, e))
        {
            await cannotAttackMethod(e, this);
        }

        if (additionalEffects.Count > 0 && AreConditionsMet(conditionOnAdditionalEffects, e))
        {
            foreach (var additionalEffect in additionalEffects)
            {
                await additionalEffect.Apply(e);
            }
        }

    }

    private Func<ApplyEffectEventArgs, CannotAttackOtherCard,Task> GetCannotAttackMethod(PlayerID cardOwner)
    {
        switch (cardOwner)
        {
            case PlayerID.Player1:
                return effectMethod.P1CannotAttackOtherCard;
            case PlayerID.Player2:
                return effectMethod.P2CannotAttackOtherCard;
            default:
                throw new InvalidOperationException("Invalid card owner.");
        }
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
