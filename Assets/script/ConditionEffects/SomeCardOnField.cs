using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameNamespace;

[CreateAssetMenu(fileName = "New SomeCard OnField", menuName = "Condition/SomeCardOnField")]
public class SomeCardOnField : ConditionEffectsInf
{
    public int some;
    public TargetType targetType;
    public bool ApplyToMyself;
    public override bool ApplyEffect(ApplyEffectEventArgs e)
    {
        if(e.Card.CardOwner == PlayerID.Player1){
            if(ApplyToMyself){
                return conditionMethod.P1SomeCardOnField(targetType,some);
            }else{
                return conditionMethod.P2SomeCardOnField(targetType,some);
            }
            
        }else{
            if(ApplyToMyself){
                return conditionMethod.P2SomeCardOnField(targetType,some);
            }else{
                return conditionMethod.P1SomeCardOnField(targetType,some);
            }
            
        }
        
    }
}
