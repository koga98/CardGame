using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

public interface IConditionEffect
{
    bool ApplyEffect(ApplyEffectEventArgs e);
}
