using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New  IsTarget", menuName = "Condition/IsParticularCardOnField")]
public class IsParticularCardOnField : ConditionEffectsInf
{
    public string target;
    public override bool ApplyEffect(ApplyEffectEventArgs e)
    {
        if(e.Card.CardOwner == PlayerID.Player1){
            return conditionMethod.P1IsParticularCardOnField(e,target);
        }else{
             return conditionMethod.P2IsParticularCardOnField(e,target);
        }
        
    }
}
