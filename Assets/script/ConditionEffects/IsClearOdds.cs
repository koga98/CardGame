using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New  IsClearOdds", menuName = "Condition/IsClearOdds")]
public class IsClearOdds : ConditionEffectsInf
{
    public float odds;
    public override bool ApplyEffect(ApplyEffectEventArgs e)
    {
        return conditionMethod.IsClearOdds(odds);
    }
}
