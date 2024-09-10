using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    public CardManager player1CardManager;
    public CardManager player2CardManager;
    public GameManager gameManager;
    public EffectManager effectManager;
    public EffectAnimationManager effectAnimationManager;
    public static GameObject attackObject;
    public static GameObject defenceObject;
    UtilMethod utilMethod = new UtilMethod();
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator AttackLeaderCoroutine()
    {
        // 非同期メソッドを実行し、その完了を待つyield return AttackedLeaderAsync().AsCoroutine();
        yield return AttackLeader().AsCoroutine();
    }

    public IEnumerator AttackCardCoroutine()
    {
        // 非同期メソッドを実行し、その完了を待つ
        yield return AttackCard().AsCoroutine();
    }

    public async Task AttackCard()
    {
        gameManager.isDealing = true;
        if (GameManager.defenceObject != null && GameManager.attackObject != null)
        {
            Card attackCard = GameManager.attackObject.GetComponent<Card>();
            CardAnimation cardAnimation = GameManager.attackObject.GetComponent<CardAnimation>();
            EnemyCardAnimation enemyCardAnimation = GameManager.defenceObject.GetComponent<EnemyCardAnimation>();
            Card defenceCard = GameManager.defenceObject.GetComponent<Card>();

            UpdateAttackCount(attackCard);
            effectAnimationManager.ResetAnimations(cardAnimation, enemyCardAnimation);

            await effectManager.AttackingEffectDealAsync(attackCard, defenceCard);
            if (attackCard.inf.attackClip != null)
            {
                await effectAnimationManager.EffectDuringBattle(defenceCard, attackCard);
            }

            if (defenceCard.inf.attackClip != null)
            {
                await effectAnimationManager.EffectDuringBattle(attackCard, defenceCard);
            }

            await CalculateDamageAsync(attackCard, defenceCard);
            await effectManager.EffectDuringBattleEndDeal();

            // 必要に応じてカードの破壊処理などを追加
            // 攻撃後に攻撃オブジェクトをリセット
            ResetStatus(attackCard);
            gameManager.isDealing = false;
        }
        else
        {
            Debug.Log("attackCardなし");
        }
    }

    public async Task AttackLeader()
    {
        Leader leader = GameManager.defenceObject.GetComponent<Leader>();
        leaderAnimation anim = GameManager.defenceObject.GetComponent<leaderAnimation>();
        Card attackCard = GameManager.attackObject.GetComponent<Card>();
        EnemyCardAnimation enemyCardAnimation = null;
        CardAnimation cardAnimation = null;

        if (GameManager.attackObject.tag == "Enemy")
        {
            enemyCardAnimation = GameManager.attackObject.GetComponent<EnemyCardAnimation>();
        }
        else if (GameManager.attackObject.tag == "Card")
        {
            cardAnimation = GameManager.attackObject.GetComponent<CardAnimation>();
        }

        if (attackCard != null && leader != null)
        {
            UpdateAttackCount(attackCard);
            if (attackCard.inf.effectInfs != null)
            {
                await effectManager.EffectWhenAttackingAsync(attackCard);
            }
            if (attackCard.inf.attackClip != null)
            {
                await effectAnimationManager.EffectDuringAttackingLeader(attackCard, leader);
            }

            LeaderDamageCalculate(leader, attackCard);
            await effectManager.EffectDuringBattleEndDeal();
            await effectManager.OnProtectShieldChangedAsync(leader.playerType);
            // 必要に応じてカードの破壊処理などを追加
            attackCard.attackPre = false;
            effectAnimationManager.ResetAnimations(cardAnimation, enemyCardAnimation, anim);
            GameManager.attackObject = null;
            GameManager.defenceObject = null;
        }
        else
        {
            Debug.Log("attackCardなし");
        }
    }

    private void LeaderDamageCalculate(Leader leader, Card attackCard)
    {
        if (leader.ProtectShield > 0)
        {
            leader.ProtectShield -= (int)(attackCard.attack * (1 - leader.shieldDamageCutAmount));
            leader.protectShieldText.text = leader.ProtectShield.ToString();
        }
        else
        {
            leader.Hp -= attackCard.attack;
            leader.healthText.text = leader.Hp.ToString();
        }

        if (leader.Hp <= 0)
        {
        }
    }

    private void ResetStatus(Card attackCard)
    {
        attackCard.attackPre = false;
        GameManager.attackObject = null;
        GameManager.defenceObject = null;
        gameManager.restrictionClick = false;
    }

    private async Task CalculateDamageAsync(Card attackCard, Card defenceCard)
    {
        if (!attackCard.canAvoidAttack)
        {
            await utilMethod.DamageMethod(attackCard, defenceCard.attack);
        }

        if (!defenceCard.canAvoidAttack)
        {
            await utilMethod.DamageMethod(defenceCard, attackCard.attack);
        }
        attackCard.canAvoidAttack = false;
        defenceCard.canAvoidAttack = false;
    }

    private void UpdateAttackCount(Card attackCard)
    {
        for (int i = 0; i < attackCard.attackedList.Count; i++)
        {
            if (!attackCard.attackedList[i])
            {
                attackCard.attackedList[i] = true;
                break; // 一つ変更したらループを終了する
            }
        }
    }
}
