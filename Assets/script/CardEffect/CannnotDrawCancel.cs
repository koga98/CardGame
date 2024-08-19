using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using GameNamespace;
using System.Threading.Tasks;
using System.Linq;

[CreateAssetMenu(fileName = "New Cannnot Draw Cancel Effect", menuName = "Effect/CannnotDrawCancel")]
public class CannnotDrawCancel : EffectInf
{
    public List<ConditionEffectsInf> conditionOnEffects;
    public List<EffectInf> additionalEffects;
    public List<ConditionEffectsInf> conditionOnAdditionalEffects;
    public bool IsConditionClear;
    public bool ApplyToMyself;
    public override async Task Apply(ApplyEffectEventArgs e)
    {
        Func<ApplyEffectEventArgs, CannnotDrawCancel,Task> drawCancelMethod = GetDrawCancelMethod(e.Card.CardOwner, ApplyToMyself);

        if (AreConditionsMet(conditionOnEffects, e))
        {
            await drawCancelMethod(e, this);
        }

        if (additionalEffects.Count > 0 && AreConditionsMet(conditionOnAdditionalEffects, e))
        {
            foreach (var additionalEffect in additionalEffects)
            {
                await additionalEffect.Apply(e);
            }
        }
    }

    private Func<ApplyEffectEventArgs, CannnotDrawCancel,Task> GetDrawCancelMethod(PlayerID cardOwner, bool applyToMyself)
    {
        if (cardOwner == PlayerID.Player1)
        {
            return applyToMyself ? effectMethod.P1CannnotDrawCancel : effectMethod.P2CannnotDrawCancel;
        }
        else if (cardOwner == PlayerID.Player2)
        {
            return applyToMyself ? effectMethod.P2CannnotDrawCancel : effectMethod.P1CannnotDrawCancel;
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
