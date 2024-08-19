using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New  IsTarget", menuName = "Condition/PlayCardIsTargetParticularCard")]
public class PlayCardIsTargetParticularCard :ConditionEffectsInf
{
    public string target;
    public override bool ApplyEffect(ApplyEffectEventArgs e)
    {
        return conditionMethod.PlayIsTargetParticularCard(e,target);
    }
}
