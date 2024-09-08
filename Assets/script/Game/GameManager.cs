using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using GameNamespace;
using System.Threading.Tasks;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public ManaManager manaManager;
    public UIManager uIManager;
    public GameObject cardPrefab;
    public GameObject enemyCardPrefab;
    public GameObject leaderPrefab;
    public GameObject animationPrefab;
    public GameObject aoeEffectPrefab;
    public GameObject buffEffectPrefab;
    public GameObject enemyLeaderPrefab;
    public AllCardInf allCardInf;
    public static List<int> myDeckInf = new List<int>();
    public List<int> enemyDeckInf = new List<int>();
    [SerializeField] private LeaderInf myLeaderInf;
    [SerializeField] private LeaderInf enemyLeaderInf;
    public GameObject canvas;
    [SerializeField] EnemyAI enemyAI;
    public int myDeckIndex = 0;
    public int enemyDeckIndex = 0;
    public bool finishEnemyTurn;
    public GameObject choiceCard;
    public GameObject myLeader;
    public GameObject enemyLeader;
    public static bool completeButtonChoice;
    public int p1_turnElapsed;
    public int p2_turnElapsed;
    public bool oneTime = false;
    public static TurnStatus turnStatus;
    public static List<Card> PAttackFields;
    public static List<Card> PDefenceFields;
    public static ObservableCollection<Card> PAllFields;
    public static List<Card> PHands;
    public GameObject detailPanel;
    public bool p1_turn;
    public static GameObject attackObject;
    public static GameObject defenceObject;
    public Text someTexts;
    public List<Text> cardDetailText;
    private System.Random rng = new System.Random();
    bool isEnemyTurnProcessing = false;
    bool isPlayerTurnProcessing = false;
    public bool isDealing = false;
    public bool nowDestory = false;
    UtilMethod utilMethod = new UtilMethod();

    public enum TurnStatus
    {
        OnPlay,
        OnTurnStart,
        OnTurnEnd,
        OnEnemyTurnEnd,
        OnEnemyOnPlay,
        OnAttack,
        OnEnemyTurnStart,
        OnEnemyAttack
    }
    // Start is called before the first frame update
    void Start()
    {
        InitializeFields();
        InitializeGameSettings();
        DecideTurnOrder();
        Shuffle(myDeckInf);
        Shuffle(enemyDeckInf);
        LeaderSetUp();
        FirstHandSetUp();
        enemyAI.Instantiate();
    }

    private void DecideTurnOrder()
    {
        System.Random random = new System.Random();
        bool isP1First = random.Next(2) == 0;
        p1_turn = isP1First;

    }

    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    void Update()
    {
        ManageEachTurns();

        if (Input.GetMouseButtonDown(0))
        {
            if (detailPanel.activeSelf)
            {
                detailPanel.SetActive(false);
            }
        }
    }

    public void drawCard()
    {
        if (CardManager.p1CannotDrawEffectList.Count == 0)
        {
            AudioManager.Instance.PlayDrawSound();
            Transform hand = canvas.transform.Find("myHand");
            GameObject card = Instantiate(cardPrefab);
            card.transform.SetParent(hand);
            card.GetComponent<Card>().P1SetUp(allCardInf.allList[myDeckInf[myDeckIndex]]);
            card.transform.localScale = Vector3.one;
            PHands.Add(card.GetComponent<Card>());
            myDeckIndex++;
        }
    }

    public void turnChange()
    {
        if (p1_turn == true)
        {
            p1_turn = false;
        }
        else
        {
            p1_turn = true;
        }
    }

    public async void nextPlay()
    {
        await WaitUntilFalse(() => isDealing);
        AudioManager.Instance.TurnEndButtonSound();
        if (turnStatus == TurnStatus.OnPlay)
        {
            AttackPhaze();
        }
        else if (turnStatus == TurnStatus.OnAttack)
        {
            await TurnEndPhaze();
        }
    }

    private void ManageEachTurns()
    {
        if (!p1_turn && !isEnemyTurnProcessing)
        {
            isEnemyTurnProcessing = true;
            finishEnemyTurn = false;
            StartEnemyTurnAsync();
        }

        if (p1_turn && !oneTime && !isPlayerTurnProcessing)
        {
            isPlayerTurnProcessing = true;
            oneTime = true;
            turnStatus = TurnStatus.OnTurnStart;
            StartPlayerTurnAsync();
        }
    }

    async void StartPlayerTurnAsync()
    {
        await WaitYourTurn();
        isPlayerTurnProcessing = false;
    }

    async void StartEnemyTurnAsync()
    {
        await WaitEnemyTurn();
    }

    private async Task WaitYourTurn()
    {
        // GameObjectをオフにする
        uIManager.MessagePanelActive();
        someTexts.text = "Your Turn";
        // さらに一定時間待機
        await Task.Delay(1000);
        turnStatus = TurnStatus.OnTurnStart;
        p1_turnElapsed++;
        await TriggerEffectsAsync(EnemyAI.EAllFields.ToList(), EffectInf.CardTrigger.OnEnemyTurnStart);
        await TriggerEffectsAsync(PAllFields.ToList(), EffectInf.CardTrigger.OnTurnStart);
        uIManager.MessagePanelInActive();
        drawCard();
        
        manaManager.P1TurnStart();
        turnStatus = TurnStatus.OnPlay;
        uIManager.PhazeOperateButtonActive();
        uIManager.ChangePhazeOperateButtonText("攻撃フェーズへ");
    }

    private async Task WaitEnemyTurn()
    {
        turnStatus = TurnStatus.OnEnemyTurnStart;
        p2_turnElapsed++;
        await TriggerEffectsAsync(PAllFields.ToList(), EffectInf.CardTrigger.OnEnemyTurnStart);
        await TriggerEffectsAsync(EnemyAI.EAllFields.ToList(), EffectInf.CardTrigger.OnTurnStart);

        enemyAI.drawCard(enemyDeckIndex, allCardInf.allList[enemyDeckInf[enemyDeckIndex]], canvas, enemyCardPrefab, this);
        
        manaManager.P2TurnStart();
        turnStatus = TurnStatus.OnEnemyOnPlay;
        await enemyAI.PlayCard(manaManager);
        turnStatus = TurnStatus.OnEnemyAttack;
        await enemyAI.Attack();
        turnStatus = TurnStatus.OnEnemyTurnEnd;
        await EnemyTurnEndPhaze();
    }

    private void AttackPhaze()
    {
        turnStatus = TurnStatus.OnAttack;
        uIManager.ChangePhazeOperateButtonText("ターンエンド");
        foreach (Card card in PAllFields)
        {
            card.ActivePanel.SetActive(!card.Attacked);
        }
    }

    private async Task TurnEndPhaze()
    {
        //効果処理が終わってから作動するようにしないとダメ
        turnStatus = TurnStatus.OnTurnEnd;
        foreach (var card in PAllFields)
        {
            card.elapsedTurns++;
        }
        await TriggerEffectsAsync(PAllFields.ToList(), EffectInf.CardTrigger.OnTurnEnd);
        await TriggerEffectsAsync(EnemyAI.EAllFields.ToList(), EffectInf.CardTrigger.OnEnemyTurnEnd);
        await TriggerEffectsAsync(CardManager.P2SpelEffectAfterSomeTurn, EffectInf.CardTrigger.SpelEffectSomeTurn);

        foreach (var card in PAllFields)
        {
            card.ActivePanel.SetActive(false);
        }

        turnChange();
        uIManager.PhazeOperateButtonInActive();
    }

    private async Task EnemyTurnEndPhaze()
    {
        foreach (var card in EnemyAI.EAllFields)
        {
            card.elapsedTurns++;
        }

        await TriggerEffectsAsync(PAllFields.ToList(), EffectInf.CardTrigger.OnEnemyTurnEnd);
        await TriggerEffectsAsync(EnemyAI.EAllFields.ToList(), EffectInf.CardTrigger.OnTurnEnd);
        await TriggerEffectsAsync(CardManager.P1SpelEffectAfterSomeTurn, EffectInf.CardTrigger.SpelEffectSomeTurn);

        finishEnemyTurn = true;

        await WaitUntilAsync(() => finishEnemyTurn);
        oneTime = false;
        isEnemyTurnProcessing = false;
        turnChange();
    }

    private async Task TriggerEffectsAsync(List<Card> cards, EffectInf.CardTrigger trigger)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            for (int j = 0; j < card.inf.effectInfs.Count; j++)
            {
                var effect = card.inf.effectInfs[j];
                if (effect.triggers.Contains(trigger) && effect is ICardEffect cardEffect)
                {
                    await cardEffect.Apply(new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields, PAllFields, PAttackFields, PDefenceFields));
                }
            }
        }
    }

    private async Task WaitUntilAsync(Func<bool> condition)
    {
        while (!condition())
        {
            await Task.Yield();
        }
    }

    private async Task WaitUntilFalse(Func<bool> condition)
    {
        while (condition())
        {
            await Task.Yield();
        }
    }

    public async Task AttackCard()
    {
        isDealing = true;
        if (defenceObject != null && attackObject != null)
        {

            Card attackCard = attackObject.GetComponent<Card>();
            CardAnimation cardAnimation = attackObject.GetComponent<CardAnimation>();
            EnemyCardAnimation enemyCardAnimation = defenceObject.GetComponent<EnemyCardAnimation>();
            Card defenceCard = defenceObject.GetComponent<Card>();
            UpdateAttackCount(attackCard);
            ResetAnimations(cardAnimation, enemyCardAnimation);

            await EffectDeal(attackCard, defenceCard);
            if (attackCard.inf.attackClip != null)
            {
                await AttackEffectAsync(defenceCard, attackCard);
            }

            if (defenceCard.inf.attackClip != null)
            {
                await DefenceEffectAsync(defenceCard, attackCard);
            }

            await CalculateDamage(attackCard, defenceCard);
            // 必要に応じてカードの破壊処理などを追加
            // 攻撃後に攻撃オブジェクトをリセット

            ResetStatus(attackCard);
            isDealing = false;
        }
        else
        {
            print("attackCardなし");
        }
    }

    private async Task AttackEffectAsync(Card defenceCard, Card attackCard)
    {
        GameObject attackEffect = Instantiate(animationPrefab, defenceCard.gameObject.transform);
        Animator attackEffectAnimator = attackEffect.GetComponent<Animator>();
        attackEffectAnimator.Play(attackCard.inf.attackClip.name);
        AudioManager.Instance.PlayBattleSound();

        await WaitForAnimationToEndAsync(attackEffectAnimator, attackCard.inf.attackClip.name, attackEffect);
    }

    private async Task WaitForAnimationToEndAsync(Animator animator, string animationName, GameObject attackEffect)
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
        Destroy(attackEffect);
    }

    private async Task DefenceEffectAsync(Card defenceCard, Card attackCard)
    {
        GameObject defenceEffect = Instantiate(animationPrefab, attackCard.gameObject.transform);
        Animator defenceEffectAnimator = defenceEffect.GetComponent<Animator>();
        defenceEffectAnimator.Play(defenceCard.inf.attackClip.name);
        AudioManager.Instance.PlayBattleSound();

        await WaitForAnimationToEndAsync(defenceEffectAnimator, defenceCard.inf.attackClip.name, defenceEffect);
    }

    private async Task CalculateDamage(Card attackCard, Card defenceCard)
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
        EffectEndDeal();
    }

    private void ResetStatus(Card attackCard)
    {
        attackCard.attackPre = false;
        attackObject = null;
        defenceObject = null;
    }

    private void LeaderSetUp()
    {
        Transform leaderTransform = canvas.transform.Find("myLeader");
        myLeader = Instantiate(leaderPrefab, leaderTransform, false);
        myLeader.GetComponent<Leader>().P1SetUp(myLeaderInf);
        enemyAI.leader = myLeader;

        Transform enemyLeaderTransform = canvas.transform.Find("enemyLeader");
        enemyLeader = Instantiate(enemyLeaderPrefab, enemyLeaderTransform, false);
        enemyAI.leader = myLeader;
        enemyLeader.GetComponent<Leader>().P2SetUp(enemyLeaderInf);
    }

    private void FirstHandSetUp()
    {
        for (myDeckIndex = 0; myDeckIndex < 5; myDeckIndex++)
        {
            Transform hand = canvas.transform.Find("myHand");
            GameObject card = Instantiate(cardPrefab, hand, false);
            card.GetComponent<Card>().P1SetUp(allCardInf.allList[myDeckInf[myDeckIndex]]);
            PHands.Add(card.GetComponent<Card>());
            card.transform.localScale = Vector3.one;
        }

        for (enemyDeckIndex = 0; enemyDeckIndex < 5; enemyDeckIndex++)
        {
            Transform hand = canvas.transform.Find("enemyHand ");
            GameObject card = Instantiate(enemyCardPrefab, hand, false);
            card.GetComponent<Card>().P2SetUp(allCardInf.allList[enemyDeckInf[enemyDeckIndex]]);
            card.transform.localScale = Vector3.one;
        }
    }

    private void InitializeFields()
    {
        PHands = new List<Card>();
        PAttackFields = new List<Card>();
        PDefenceFields = new List<Card>();
        PAllFields = new ObservableCollection<Card>();
        PAllFields.CollectionChanged += CollectionChanged;
    }

    private void InitializeGameSettings()
    {
        p1_turnElapsed = 0;
        p2_turnElapsed = 0;
        finishEnemyTurn = true;
        completeButtonChoice = false;
    }

    private async Task EffectDeal(Card attackCard, Card defenceCard)
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
                            , PAllFields, PAttackFields, PDefenceFields));
                        }
                    }
                    else if (cardTrigger == EffectInf.CardTrigger.OnDuringAttack)
                    {
                        if (attackCard.inf.effectInfs[i] is ICardEffect cardEffect)
                        {
                            await cardEffect.Apply(new ApplyEffectEventArgs(attackCard, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields
                            , PAllFields, PAttackFields, PDefenceFields));
                        }
                    }
                }
            }
        }
        if (defenceCard.inf.effectInfs != null)
        {
            for (int i = 0; i < defenceCard.inf.effectInfs.Count; i++)
            {
                foreach (EffectInf.CardTrigger cardTrigger in defenceCard.inf.effectInfs[i].triggers)
                {
                    if (cardTrigger == EffectInf.CardTrigger.OnDefence)
                    {
                        if (defenceCard.inf.effectInfs[i] is ICardEffect cardEffect)
                        {

                            await cardEffect.Apply(new ApplyEffectEventArgs(defenceCard, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields
                            , PAllFields, PAttackFields, PDefenceFields));
                        }
                    }
                }
            }
        }
    }

    private async void EffectEndDeal()
    {
        await EffectProcess(CardManager.P1EffectDuringAttacking);
        await EffectProcess(CardManager.P2EffectDuringAttacking);
    }

    private async Task EffectProcess(List<Card> cards)
    {
        if (cards.Count != 0 && cards[0] != null)
        {
            for (int i = 0; i < cards[0].inf.effectInfs.Count; i++)
            {
                foreach (EffectInf.CardTrigger cardTrigger in cards[0].inf.effectInfs[i].triggers)
                {
                    if (cardTrigger == EffectInf.CardTrigger.EndAttack)
                    {
                        if (cards[0].inf.effectInfs[i] is ICardEffect cardEffect)
                        {
                            await cardEffect.Apply(new ApplyEffectEventArgs(cards[0], EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields
                            , PAllFields, PAttackFields, PDefenceFields));
                        }
                    }
                }
            }
            cards[0] = null;
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

    private void ResetAnimations(CardAnimation cardAnimation, EnemyCardAnimation enemyCardAnimation)
    {
        if (cardAnimation != null)
        {
            cardAnimation.animator.SetBool("extendMy", false);
        }

        if (enemyCardAnimation != null)
        {
            enemyCardAnimation.animator.SetBool("extendEnemy", false);
        }
    }

    private static async void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            if (CardManager.P1CardsWithEffectOnField.Count != 0)
            {
                Card card = (Card)e.NewItems[0];
                foreach (var effectCard in CardManager.P1CardsWithEffectOnField)
                {
                    foreach (var effect in effectCard.inf.effectInfs)
                    {
                        for (int i = 0; i < effect.triggers.Count; i++)
                        {
                            if (effect.triggers[i] == EffectInf.CardTrigger.OnFieldOnAfterPlay)
                            {
                                if (effect is ICardEffect cardEffect)
                                {
                                    await cardEffect.Apply(new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields, PAllFields, PAttackFields, PDefenceFields));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

}

