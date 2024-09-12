using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "New Heal Card", menuName = "Effect/HpHeal")]
public class HpHealCard : EffectInf
{
  public int healAmount;
  public string target;
  public List<ConditionEffectsInf> conditionOnEffects;
  public List<EffectInf> additionalEffects;
  public List<ConditionEffectsInf> conditionOnAdditionalEffects;

  public override async Task Apply(ApplyEffectEventArgs e)
  {
    if (AreConditionsMet(conditionOnEffects, e))
    {
      await effectMethod.HealCard(e, healAmount, this, target);
    }
    
    if (additionalEffects.Count > 0 && AreConditionsMet(conditionOnAdditionalEffects, e))
    {
      foreach (var additionalEffect in additionalEffects)
      {
        await additionalEffect.Apply(e);
      }
    }
  }

  private bool AreConditionsMet(List<ConditionEffectsInf> conditions, ApplyEffectEventArgs e)
  {
    foreach (var condition in conditions)
    {
      if (!condition.ApplyEffect(e))
      {
        return false;
      }
    }
    return true;
  }

  public override async Task EffectOfEffect(ApplyEffectEventArgs e)
  {
    GameObject manager = GameObject.Find("GameManager");
    EffectAnimationManager effectAnimationManager = manager.GetComponent<EffectAnimationManager>();
    GameObject attackEffect;

    if(e.ChoiceCard != null){
      attackEffect= Instantiate(effectAnimationManager.buffEffectPrefab, e.ChoiceCard.gameObject.transform);
    }else{
      attackEffect= Instantiate(effectAnimationManager.buffEffectPrefab, e.Card.gameObject.transform);
    }
     
    Animator attackEffectAnimator = attackEffect.GetComponent<Animator>();
    attackEffectAnimator.Play(animationClip.name);

    // サウンド再生
    AudioManager.Instance.EffectSound(audioClip);

    // アニメーションの完了を非同期で待機
    await WaitForAnimationAsync(attackEffectAnimator, animationClip.name);

    Destroy(attackEffect);
  }
  private async Task WaitForAnimationAsync(Animator animator, string animationName)
  {
    while (true)
    {
      AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
      if (stateInfo.IsName(animationName) && stateInfo.normalizedTime >= 1.0f)
      {
        break; // アニメーションが終了したらループを抜ける
      }
      await Task.Yield(); // 次のフレームまで待機
    }
  }
}
