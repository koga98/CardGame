using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New  MoreShield OnDamage", menuName = "Condition/MoreShieldOnDamage")]

public class MoreShieldOnDamage : ConditionEffectsInf
{
    public int conditionDamage;
    public override bool ApplyEffect(ApplyEffectEventArgs e)
    {
        if(e.Card.CardOwner == PlayerID.Player1){
            return conditionMethod.P1IsMoreShieldOnDamage(conditionDamage);
        }else{
            return conditionMethod.P2IsMoreShieldOnDamage(conditionDamage);
        }
        
    }
}
