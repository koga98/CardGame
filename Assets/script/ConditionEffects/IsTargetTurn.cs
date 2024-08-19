using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New  IsTargetTurn", menuName = "Condition/IsTargetTurn")]
public class IsTargetTurn : ConditionEffectsInf
{
   public int targetTurn;
    public override bool ApplyEffect(ApplyEffectEventArgs e)
    {
        if(e.Card.CardOwner == PlayerID.Player1){
            
            return conditionMethod.P1IsTargetTurn(e,targetTurn);
        }else{
            return conditionMethod.P2IsTargetTurn(e,targetTurn);
        }
    }
}
