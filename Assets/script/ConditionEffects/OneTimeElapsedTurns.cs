using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New  IsClearOdds", menuName = "Condition/OneTimeElapsedTurns")]
public class OneTimeElapsedTurns : ConditionEffectsInf
{
    public int waitThisTime;
    public override bool ApplyEffect(ApplyEffectEventArgs e)
    {
        return conditionMethod.OneTimeElapsedTurns(e,waitThisTime);;
    }
}
