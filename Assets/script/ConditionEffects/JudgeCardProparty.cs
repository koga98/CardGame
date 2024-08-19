using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameNamespace;
[CreateAssetMenu(fileName = "New  LimitTurns", menuName = "Condition/JudgeCardProparty")]
public class JudgeCardProparty : ConditionEffectsInf
{
    public string target;
    public CardType cardType;
    public override bool ApplyEffect(ApplyEffectEventArgs e)
    {
        return conditionMethod.JudgeCardProparty(e,cardType);;
    }
}
