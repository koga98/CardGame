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
    public TextMeshProUGUI healthText;
    public Text protectShieldText;
    GameObject manager;
    GameManager gameManager;
    UIManager uIManager;
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
                if (protectShield < 0)
                {
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
        uIManager = manager.GetComponent<UIManager>();
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (GameManager.defenceObject != null && !restrictionClick)
            {
                restrictionClick = true;
                StartCoroutine(AttackCoroutine());
            }
        }
    }
     private IEnumerator AttackCoroutine()
    {
        // 非同期メソッドを実行し、その完了を待つ
        yield return AttackedLeaderAsync().AsCoroutine();
        restrictionClick = false;
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
            if (attackCard.inf.effectInfs != null)
            {
                await EffectWhenAttacking(attackCard);
            }
            if (attackCard.inf.attackClip != null)
            {
                await AttackEffectAsync(attackCard, leader);
            }

            // 必要に応じてカードの破壊処理などを追加
            if (leader.Hp <= 0)
            {
                Destroy(GameManager.defenceObject);
            }
            attackCard.attackPre = false;
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
        if (leader.ProtectShield > 0)
        {
            leader.ProtectShield -= (int)(attackCard.attack * (1 - shieldDamageCutAmount));
            leader.protectShieldText.text = leader.ProtectShield.ToString();
        }
        else
        {
            leader.Hp -= attackCard.attack;
            leader.healthText.text = leader.Hp.ToString();
        }

        EffectEndDeal();
    }

    private async void EffectEndDeal()
    {
        await ProcessEffectEnd(CardManager.P1EffectDuringAttacking);
        await ProcessEffectEnd(CardManager.P2EffectDuringAttacking);
    }

    private async Task ProcessEffectEnd(List<Card> effectList)
    {
        if (effectList.Count == 0 || effectList[0] == null) return;

        var currentCard = effectList[0];

        foreach (var effectInf in currentCard.inf.effectInfs)
        {
            foreach (EffectInf.CardTrigger cardTrigger in effectInf.triggers)
            {
                if (cardTrigger == EffectInf.CardTrigger.EndAttack && effectInf is ICardEffect cardEffect)
                {
                    await cardEffect.Apply(new ApplyEffectEventArgs(
                        currentCard,
                        EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                        GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields
                    ));
                }
            }
        }

        effectList[0] = null;
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
        if (leader.playerType == PlayerType.Player1)
        {
            ReverseAnim(attackEffect);
        }
        AudioManager.Instance.PlayBattleSound();
        attackEffectAnimator.Play(shieldClip.name);
        // アニメーションが終了するまで待機するタスク
        await WaitForAnimationToEndAsync(attackEffectAnimator, shieldClip.name);

        Destroy(attackEffect);
        await OnProtectShieldChanged();
    }

    private void ReverseAnim(GameObject attackEffect)
    {
        Transform transform = attackEffect.transform;
        Vector3 localScale = transform.localScale;
        localScale.x = localScale.x * -1;
        transform.localScale = localScale;
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

    private async Task EffectWhenAttacking(Card attackCard)
    {
        if (attackCard.inf.effectInfs != null)
        {
            for (int i = 0; i < attackCard.inf.effectInfs.Count; i++)
            {
                foreach (EffectInf.CardTrigger cardTrigger in attackCard.inf.effectInfs[i].triggers)
                {
                    if (cardTrigger == EffectInf.CardTrigger.OnAttack)
                    {
                        if (attackCard.inf.effectInfs[i] is ICardEffect cardEffect)
                        {
                            await cardEffect.Apply(new ApplyEffectEventArgs(attackCard, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields
                            , GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
                        }
                    }
                    else if (cardTrigger == EffectInf.CardTrigger.OnDuringAttack)
                    {
                        if (attackCard.inf.effectInfs[i] is ICardEffect cardEffect)
                        {
                            await cardEffect.Apply(new ApplyEffectEventArgs(attackCard, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields
                            , GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
                        }
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
            await ProcessBasicEffect(CardManager.P1CardsWithEffectOnField);
        }
        else
        {
            await ProcessBasicEffect(CardManager.P2CardsWithEffectOnField);
        }
    }

    private async Task ProcessBasicEffect(List<Card> effectList)
    {
        for (int i = 0; i < effectList.Count; i++)
        {
            for (int e = 0; e < effectList[i].inf.effectInfs.Count; e++)
            {
                for (int k = 0; k < effectList[i].inf.effectInfs[e].triggers.Count; k++)
                {
                    if (effectList[i].inf.effectInfs[e].triggers[k] == EffectInf.CardTrigger.AfterPlayAndProtectShieldDecrease)
                    {
                        if (effectList[i].inf.effectInfs[e] is ICardEffect cardEffect)
                        {
                            await cardEffect.Apply(new ApplyEffectEventArgs(effectList[i], EnemyAI.EAllFields, EnemyAI.AttackFields,
                            EnemyAI.DefenceFields, GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
                        }
                    }
                }
            }

        }
    }

    private void GameOver()
    {
        uIManager.GameSetPanelActive();
        if (playerType == PlayerType.Player1)
        {
            uIManager.ChangeGameSetText("You Lose");
        }
        else if (playerType == PlayerType.Player2)
        {
            uIManager.ChangeGameSetText("You Win");
        }
    }

}
