using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameNamespace;

[CreateAssetMenu(fileName = "New NoCard OnField", menuName = "Condition/NoCardOnField")]
public class NoCardOnField : ConditionEffectsInf
{
    public TargetType targetType;
    public bool ApplyToMyself;
    public override bool ApplyEffect(ApplyEffectEventArgs e)
    {
        if(e.Card.CardOwner == PlayerID.Player1){
            if(ApplyToMyself){
                return conditionMethod.P1NoCardOnField(targetType);
            }else{
                return conditionMethod.P2NoCardOnField(targetType);
            }
            
        }else{
            if(ApplyToMyself){
                return conditionMethod.P2NoCardOnField(targetType);
            }else{
                return conditionMethod.P1NoCardOnField(targetType);
            }
            
        }
        
    }
}
