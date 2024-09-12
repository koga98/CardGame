using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

public abstract class EffectInf : ScriptableObject,ICardEffect
{
    public EffectMethod effectMethod = new EffectMethod();
    public AnimationClip animationClip;
    public AudioClip audioClip;
    public abstract Task Apply(ApplyEffectEventArgs e);
    public abstract Task EffectOfEffect(ApplyEffectEventArgs e);

    public List<CardTrigger> triggers;

    public enum CardTrigger{
        OnPlay,
        OnTurnStart,
        AfterPlay,
        AfterDie,
        OnTurnEnd,
        OnAttack,
        OnDuringAttack,
        EndAttack,
        OnBeginDrag,
        OnDefence,
        OnFieldOnAfterPlay,
        ProtectShieldDecrease,
        OnEnemyTurnEnd,
        OnEnemyTurnStart,
        SpelEffectSomeTurn,
        ButtonOperetion,
        StopButtonOperetion
    } 

}
    

