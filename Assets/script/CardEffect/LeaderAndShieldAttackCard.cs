using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "New Leader And ShieldAttack Effect", menuName = "Effect/LeaderAndShieldAttack")]
public class LeaderAndShieldAttackCard : EffectInf
{
  public List<ConditionEffectsInf> conditionOnEffects;
  public List<EffectInf> additionalEffects;
  public List<ConditionEffectsInf> conditionOnAdditionalEffects;

  public override async Task Apply(ApplyEffectEventArgs e)
  {
    // Apply the main effect if conditions are met
    if (AreConditionsMet(conditionOnEffects, e))
    {
      await effectMethod.ShieldAndLeaderAttack(e, this);
    }

    // Apply additional effects if there are any and conditions are met
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

  public override async Task EffectOfEffect(ApplyEffectEventArgs e)
  {
    AudioManager.Instance.EffectSound(audioClip);
    await Task.CompletedTask;
  }
}
