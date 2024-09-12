using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameNamespace;
using UnityEngine;

public class EffectAnimationManager : MonoBehaviour
{
    public GameObject animationPrefab;
    public GameObject aoeEffectPrefab;
    public GameObject buffEffectPrefab;
    public EffectManager effectManager;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async Task ShieldDamageAsync(Leader leader)
    {
        GameObject attackEffect = Instantiate(animationPrefab, leader.gameObject.transform);
        Animator attackEffectAnimator = attackEffect.GetComponent<Animator>();
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        if (leader.playerType == PlayerType.Player1)
        {
            ReverseAnim(attackEffect);
        }
        AudioManager.Instance.PlayBattleSound();
        attackEffectAnimator.Play(leader.shieldClip.name);
        // アニメーションが終了するまで待機するタスク
        await WaitForAnimationToEndAsync(attackEffectAnimator, leader.shieldClip.name, attackEffect);
        if (!gameManager.nowEnemyAttack)
        {
            await effectManager.OnProtectShieldChanged(leader.playerType);
        }
    }
    private void ReverseAnim(GameObject attackEffect)
    {
        Transform transform = attackEffect.transform;
        Vector3 localScale = transform.localScale;
        localScale.x = -localScale.x;
        transform.localScale = localScale;
    }

    public async Task AnimationDuringAttackingLeader(Card attackCard, Leader leader)
    {
        GameObject attackEffect = Instantiate(animationPrefab, leader.gameObject.transform);
        Animator attackEffectAnimator = attackEffect.GetComponent<Animator>();
        AudioManager.Instance.PlayBattleSound();
        attackEffectAnimator.Play(attackCard.inf.attackClip.name);

        // アニメーションが終了するまで待機するタスク
        await WaitForAnimationToEndAsync(attackEffectAnimator, attackCard.inf.attackClip.name, attackEffect);
        Destroy(attackEffect);
    }

    public async Task AnimationDuringBattle(Card effectTargetCard, Card triggerCard)
    {
        GameObject triggerCardEffect = Instantiate(animationPrefab, effectTargetCard.gameObject.transform);
        Animator triggerCardEffectAnimator = triggerCardEffect.GetComponent<Animator>();
        triggerCardEffectAnimator.Play(triggerCard.inf.attackClip.name);
        AudioManager.Instance.PlayBattleSound();

        await WaitForAnimationToEndAsync(triggerCardEffectAnimator, triggerCard.inf.attackClip.name, triggerCardEffect);
    }
    private async Task WaitForAnimationToEndAsync(Animator animator, string animationName, GameObject effect)
    {
        while (true)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName(animationName) && stateInfo.normalizedTime >= 1.0f)
            {
                break;
            }
            await Task.Yield(); // 次のフレームまで待機
        }
        Destroy(effect);
    }

    public void ResetAnimations(CardAnimation cardAnimation, EnemyCardAnimation enemyCardAnimation, leaderAnimation anim = null)
    {
        if (anim != null)
        {
            anim.animator.SetBool("extendLeader", false);
        }
        if (enemyCardAnimation != null)
        {
            enemyCardAnimation.animator.SetBool("extendEnemy", false);
        }
        if (cardAnimation != null)
        {
            cardAnimation.animator.SetBool("extendMy", false);
        }
    }

    
}
