using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New  IsTarget", menuName = "Condition/IsDuringAttack")]
public class IsDuringAttack : ConditionEffectsInf
{
    public override bool ApplyEffect(ApplyEffectEventArgs e)
    {
        return conditionMethod.IsDuringAttack(e);
    }
}
