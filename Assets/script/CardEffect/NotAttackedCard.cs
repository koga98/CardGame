using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "New NotAttacked Effect", menuName = "Effect/NotAttacked")]
public class NotAttackedCard : EffectInf, ICardEffect
{
  public List<ConditionEffectsInf> conditionOnEffects;
  public List<EffectInf> additionalEffects;
  public List<ConditionEffectsInf> conditionOnAdditionalEffects;
  public bool IsConditionClear;


  public override async Task Apply(ApplyEffectEventArgs e)
  {
    // Apply the "Not Attacked" effect if conditions are met
    if (AreConditionsMet(conditionOnEffects, e))
    {
      await effectMethod.NotAttacked(e, this);
    }

    // Apply additional effects if conditions are met
    if (additionalEffects.Count > 0 && AreConditionsMet(conditionOnAdditionalEffects, e))
    {
      foreach (var additionalEffect in additionalEffects)
      {
        await additionalEffect.Apply(e);
      }
    }
  }

  // Helper method to check if all conditions are met
  private bool AreConditionsMet(List<ConditionEffectsInf> conditions, ApplyEffectEventArgs e)
  {
    if (conditions.Count == 0) return true;

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
    GameManager gameManager = manager.GetComponent<GameManager>();
    GameObject objectenemyAI = GameObject.Find("EnemyAI");
    EnemyAI enemyAI = objectenemyAI.GetComponent<EnemyAI>();

    GameObject attackEffect = Instantiate(gameManager.buffEffectPrefab, e.Card.gameObject.transform);
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
