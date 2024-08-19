using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using GameNamespace;
using UnityEngine.EventSystems;
using System.Threading.Tasks;

public class Leader : MonoBehaviour, IPointerClickHandler
{
    private int hp = 30;
    private int protectShield = 2000;
    public double shieldDamageCutAmount = 0.0;
    public AnimationClip shieldClip;
    public PlayerType playerType;
    public Sprite sprite;
    public Image leaderImage;
    public LeaderInf inf;
    EnemyCardAnimation enemyCardAnimation;
    CardAnimation cardAnimation;
    public TextMeshProUGUI healthText;
    public Text protectShieldText;
    GameObject manager;
    GameManager gameManager;
    public bool restrictionClick;

    public int Hp
    {
        get { return hp; }
        set
        {
            if (hp != value)
            {
                hp = value;
            }
            if (hp <= 0)
            {
                hp = 0;
                GameOver();
            }
        }
    }

    public int ProtectShield
    {
        get { return protectShield; }
        set
        {
            if (protectShield != value)
            {
                protectShield = value;
                if(protectShield < 0){
                    protectShield = 0;
                }
                _ = UpdateShieldAsync();
            }

        }
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>

    void Start()
    {
        manager = GameObject.Find("GameManager");
        gameManager = manager.GetComponent<GameManager>();
        restrictionClick = false;
    }
    void Update()
    {

    }

    private async Task UpdateShieldAsync()
    {
        await ShieldDamageEffectAsync(this);
    }

    public void P1SetUp(LeaderInf leaderInf)
    {
        inf = leaderInf;
        healthText.text = leaderInf.hp.ToString();
        protectShieldText.text = leaderInf.protectShield.ToString();
        sprite = leaderInf.leaderSprite;
        leaderImage.sprite = sprite;
        Hp = leaderInf.hp;
        ProtectShield = leaderInf.protectShield;
        playerType = PlayerType.Player1;
    }
    public void P2SetUp(LeaderInf leaderInf)
    {
        inf = leaderInf;
        healthText.text = leaderInf.hp.ToString();
        protectShieldText.text = leaderInf.protectShield.ToString();
        sprite = leaderInf.leaderSprite;
        leaderImage.sprite = sprite;
        Hp = leaderInf.hp;
        ProtectShield = leaderInf.protectShield;
        playerType = PlayerType.Player2;
    }
    public async Task AttackedLeaderAsync()
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

            if (leader.ProtectShield > 0)
            {
                if (attackCard.inf.effectInfs != null)
                {
                    await EffectDealAsync(attackCard);
                }
                if (attackCard.inf.attackClip != null)
                {
                    await AttackEffectAsync(attackCard, leader);
                }

                leader.ProtectShield -= (int)(attackCard.attack * (1 - shieldDamageCutAmount));
                leader.protectShieldText.text = leader.ProtectShield.ToString();
            }
            else
            {
                if (attackCard.inf.attackClip != null)
                {
                    await AttackEffectAsync(attackCard, leader);
                }
                leader.Hp -= attackCard.attack;
                leader.healthText.text = leader.Hp.ToString();
            }

            // 必要に応じてカードの破壊処理などを追加
            if (leader.Hp <= 0)
            {
                Destroy(GameManager.defenceObject);
                // 防御オブジェクトのリセット
            }
            // 攻撃後に攻撃オブジェクトをリセット
            attackCard.attackPre = false;
            await EffectEndDealAsync();
            ResetAnimations(cardAnimation, enemyCardAnimation, anim);

            GameManager.attackObject = null;
            GameManager.defenceObject = null;
        }
        else
        {
            Debug.Log("attackCardなし");
        }
    }

    private async Task AttackEffectAsync(Card attackCard, Leader leader)
    {
        GameObject attackEffect = Instantiate(gameManager.animationPrefab, leader.gameObject.transform);
        Animator attackEffectAnimator = attackEffect.GetComponent<Animator>();
        AudioManager.Instance.PlayBattleSound();
        attackEffectAnimator.Play(attackCard.inf.attackClip.name);

        // アニメーションが終了するまで待機するタスク
        await WaitForAnimationToEndAsync(attackEffectAnimator, attackCard.inf.attackClip.name);

        Destroy(attackEffect);
    }

    private async Task WaitForAnimationToEndAsync(Animator animator, string animationName)
    {
        while (true)
        {
            // 現在のアニメーションのステート情報を取得
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // 指定されたアニメーションが再生中かつアニメーションが完了した場合
            if (stateInfo.IsName(animationName) && stateInfo.normalizedTime >= 1.0f)
            {
                break; // ループを抜ける
            }

            await Task.Yield(); // 次のフレームまで待機
        }
    }

    private async Task ShieldDamageEffectAsync(Leader leader)
    {
        GameObject attackEffect = Instantiate(gameManager.animationPrefab, leader.gameObject.transform);
        Animator attackEffectAnimator = attackEffect.GetComponent<Animator>();
        AudioManager.Instance.PlayBattleSound();
        attackEffectAnimator.Play(shieldClip.name);

        // アニメーションが終了するまで待機するタスク
        await WaitForAnimationToEndAsync(attackEffectAnimator, shieldClip.name);

        Destroy(attackEffect);
        await OnProtectShieldChanged();
    }

    private void ResetAnimations(CardAnimation cardAnimation, EnemyCardAnimation enemyCardAnimation, leaderAnimation anim)
    {
        anim.animator.SetBool("extendLeader", false);
        if (enemyCardAnimation != null)
        {
            enemyCardAnimation.animator.SetBool("extendEnemy", false);
        }
        if (cardAnimation != null)
        {
            cardAnimation.animator.SetBool("extendMy", false);
        }
    }

    private async Task EffectEndDealAsync()
    {
        if (CardManager.P1EffectDuringAttacking.Count != 0 && CardManager.P1EffectDuringAttacking[0] != null)
        {
            var attackingCard = CardManager.P1EffectDuringAttacking[0];
            for (int i = 0; i < attackingCard.inf.effectInfs.Count; i++)
            {
                foreach (EffectInf.CardTrigger cardTrigger in attackingCard.inf.effectInfs[i].triggers)
                {
                    if (cardTrigger == EffectInf.CardTrigger.EndAttack)
                    {
                        if (attackingCard.inf.effectInfs[i] is ICardEffect cardEffect)
                        {
                            await cardEffect.Apply(new ApplyEffectEventArgs(attackingCard, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields
                                , GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
                        }
                    }
                }
            }
            CardManager.P1EffectDuringAttacking[0] = null;
        }else if (CardManager.P2EffectDuringAttacking.Count != 0 && CardManager.P2EffectDuringAttacking[0] != null)
        {
            for (int i = 0; i < CardManager.P2EffectDuringAttacking[0].inf.effectInfs.Count; i++)
            {
                foreach (EffectInf.CardTrigger cardTrigger in CardManager.P2EffectDuringAttacking[0].inf.effectInfs[i].triggers)
                {
                    if (cardTrigger == EffectInf.CardTrigger.EndAttack)
                    {
                        if (CardManager.P2EffectDuringAttacking[0].inf.effectInfs[i] is ICardEffect cardEffect)
                        {
                            await cardEffect.Apply(new ApplyEffectEventArgs(CardManager.P2EffectDuringAttacking[0], EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields
                            , GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
                        }
                    }
                }
            }
            CardManager.P2EffectDuringAttacking[0] = null;
        }
    }

    private async Task EffectDealAsync(Card attackCard)
    {
        for (int i = 0; i < attackCard.inf.effectInfs.Count; i++)
        {
            foreach (EffectInf.CardTrigger cardTrigger in attackCard.inf.effectInfs[i].triggers)
            {
                if (cardTrigger == EffectInf.CardTrigger.OnAttackProtectShield || cardTrigger == EffectInf.CardTrigger.OnAttack)
                {
                    if (attackCard.inf.effectInfs[i] is ICardEffect cardEffect)
                    {
                        // Applyメソッドが非同期メソッドであると仮定してawaitで呼び出す
                        await cardEffect.Apply(new ApplyEffectEventArgs(attackCard, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields
                            , GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
                    }
                }
            }
        }
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

    public async Task OnProtectShieldChanged()
    {
        if (playerType == PlayerType.Player1)
        {
            for (int i = 0; i < CardManager.P1CardsWithEffectOnField.Count; i++)
            {
                for (int e = 0; e < CardManager.P1CardsWithEffectOnField[i].inf.effectInfs.Count; e++)
                {
                    for (int k = 0; k < CardManager.P1CardsWithEffectOnField[i].inf.effectInfs[e].triggers.Count; k++)
                    {
                        if (CardManager.P1CardsWithEffectOnField[i].inf.effectInfs[e].triggers[k] == EffectInf.CardTrigger.AfterPlayAndProtectShieldDecrease)
                        {
                            if (CardManager.P1CardsWithEffectOnField[i].inf.effectInfs[e] is ICardEffect cardEffect)
                            {
                                await cardEffect.Apply(new ApplyEffectEventArgs(CardManager.P1CardsWithEffectOnField[i], EnemyAI.EAllFields, EnemyAI.AttackFields,
                                EnemyAI.DefenceFields, GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
                            }
                        }
                    }
                }

            }
        }
        else
        {
            for (int i = 0; i < CardManager.P2CardsWithEffectOnField.Count; i++)
            {
                for (int e = 0; e < CardManager.P2CardsWithEffectOnField[i].inf.effectInfs.Count; e++)
                {
                    for (int k = 0; i < CardManager.P2CardsWithEffectOnField[i].inf.effectInfs[e].triggers.Count; k++)
                    {
                        if (CardManager.P2CardsWithEffectOnField[i].inf.effectInfs[e].triggers[k] == EffectInf.CardTrigger.AfterPlayAndProtectShieldDecrease)
                        {
                            if (CardManager.P2CardsWithEffectOnField[i].inf.effectInfs[e] is ICardEffect cardEffect)
                            {
                                await cardEffect.Apply(new ApplyEffectEventArgs(CardManager.P2CardsWithEffectOnField[i], EnemyAI.EAllFields, EnemyAI.AttackFields,
                                EnemyAI.DefenceFields, GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
                            }
                        }
                    }
                }

            }
        }
    }

    private void GameOver()
    {
        gameManager.gameSetPanel.SetActive(true);
        if (playerType == PlayerType.Player1)
        {
            gameManager.gameSetText.text = "You Lose";
        }
        else if (playerType == PlayerType.Player2)
        {
            gameManager.gameSetText.text = "You Win";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (GameManager.defenceObject != null && !restrictionClick)
            {
                restrictionClick = true;
                StartCoroutine(OnPointerClickCoroutine());
            }
        }
    }

    private IEnumerator OnPointerClickCoroutine()
    {
        // GameManagerを取得
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();

        // 非同期メソッドを実行し、その完了を待つ
        yield return AttackedLeaderAsync().AsCoroutine();
        restrictionClick = false;
    }


}
