using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GameNamespace;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

[CreateAssetMenu(fileName = "New CannnotDraw Effect", menuName = "Effect/CannnotDraw")]
public class CannnotDraw : EffectInf
{
  public List<ConditionEffectsInf> conditionOnEffects;
  public List<EffectInf> additionalEffects;
  public List<ConditionEffectsInf> conditionOnAdditionalEffects;
  public bool IsConditionClear;
  public bool ApplyToMyself;
  public override async Task Apply(ApplyEffectEventArgs e)
  {
    Func<ApplyEffectEventArgs, CannnotDraw,Task> drawMethod = GetDrawMethod(e.Card.CardOwner, ApplyToMyself);

    if (AreConditionsMet(conditionOnEffects, e))
    {
      await drawMethod(e, this);
    }

    if (additionalEffects.Count > 0 && AreConditionsMet(conditionOnAdditionalEffects, e))
    {
      foreach (var additionalEffect in additionalEffects)
      {
        await additionalEffect.Apply(e);
      }
    }
  }

  private Func<ApplyEffectEventArgs, CannnotDraw,Task> GetDrawMethod(PlayerID cardOwner, bool applyToMyself)
  {
    if (cardOwner == PlayerID.Player1)
    {
      return applyToMyself ? effectMethod.P1CannnotDraw : effectMethod.P2CannnotDraw;
    }
    else if (cardOwner == PlayerID.Player2)
    {
      return applyToMyself ? effectMethod.P2CannnotDraw : effectMethod.P1CannnotDraw;
    }
    throw new InvalidOperationException("Invalid card owner.");
  }

  private bool AreConditionsMet(List<ConditionEffectsInf> conditions, ApplyEffectEventArgs e)
  {
    return conditions.Count == 0 || conditions.All(condition => condition.ApplyEffect(e));
  }
  public override async Task EffectOfEffect(ApplyEffectEventArgs e)
  {
    GameObject manager = GameObject.Find("GameManager");
    GameManager gameManager = manager.GetComponent<GameManager>();

    async Task PlayAnimationOnLeader(Leader leader)
    {
      GameObject attackEffect = Instantiate(gameManager.buffEffectPrefab, leader.gameObject.transform);
      Animator attackEffectAnimator = attackEffect.GetComponent<Animator>();
      attackEffectAnimator.Play(animationClip.name);
      AudioManager.Instance.EffectSound(audioClip);

      await WaitForAnimation(attackEffectAnimator, animationClip.name);

      Destroy(attackEffect);
    }

    async Task WaitForAnimation(Animator animator, string clipName)
    {
      while (true)
      {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(clipName) && stateInfo.normalizedTime >= 1.0f)
        {
          break; // アニメーションが終了したらループを抜ける
        }
        await Task.Yield(); // 次のフレームまで待機
      }
    }

    // 条件に応じたリーダーを取得
    Leader GetLeader(PlayerID owner, bool applyToSelf)
    {
      if (owner == PlayerID.Player1)
      {
        return applyToSelf ? gameManager.myLeader.GetComponent<Leader>() : gameManager.enemyLeader.GetComponent<Leader>();
      }
      else // owner == PlayerID.Player2
      {
        return applyToSelf ? gameManager.enemyLeader.GetComponent<Leader>() : gameManager.myLeader.GetComponent<Leader>();
      }
    }

    // 条件を満たしているか確認
    if (conditionOnEffects.Count == 0 || IsConditionClear)
    {
      Leader leader = GetLeader(e.Card.CardOwner, ApplyToMyself);
      await PlayAnimationOnLeader(leader);
    }
  }

}
