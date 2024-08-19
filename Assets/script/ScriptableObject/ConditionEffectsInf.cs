using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

public abstract class ConditionEffectsInf : ScriptableObject, IConditionEffect
{
    public ConditionEffects conditionMethod = new ConditionEffects();
    public abstract bool  ApplyEffect(ApplyEffectEventArgs e);
}
