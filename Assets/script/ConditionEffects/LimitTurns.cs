using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New  LimitTurns", menuName = "Condition/LimitTurns")]
public class LimitTurns : ConditionEffectsInf
{
    public int limitTime;
    public override bool ApplyEffect(ApplyEffectEventArgs e)
    {
        return conditionMethod.LimitTurns(e,limitTime);;
    }
}
