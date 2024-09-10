using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameNamespace;

[CreateAssetMenu(fileName = "New  IsTarget", menuName = "Condition/IsTarget")]
public class IsTarget : ConditionEffectsInf
{
    public string property;
    public CardType cardType;
    public override bool ApplyEffect(ApplyEffectEventArgs e)
    {
        Debug.Log("起動");
        return conditionMethod.IsTarget(e,property,cardType);
    }
}
