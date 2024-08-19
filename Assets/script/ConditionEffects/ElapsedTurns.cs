using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New  IsClearOdds", menuName = "Condition/ElapsedTurns")]
public class ElapsedTurns : ConditionEffectsInf
{
   public int waitThisTime;
    public override bool ApplyEffect(ApplyEffectEventArgs e)
    {
        return conditionMethod.ElapsedTurns(e,waitThisTime);;
    }
}
