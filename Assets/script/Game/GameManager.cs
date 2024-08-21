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
    public List<CardInf> enemyDeck;
    [SerializeField] private LeaderInf myLeaderInf;
    [SerializeField] private LeaderInf enemyLeaderInf;
    public GameObject panel;
    public GameObject canvas;
    [SerializeField] EnemyAI enemyAI;
    public int myDeckIndex = 0;
    public int enemyDeckIndex = 0;
    public bool finishEnemyTurn;
    public static bool P1canDraw;
    public static bool P2canDraw;
    public GameObject choiceCard;
    public GameObject myLeader;
    public GameObject enemyLeader;
    public static bool canPlayDefenceCard = true;
    public static bool completeButtonChoice;
    public int p1_turnElapsed;
    public int p2_turnElapsed;
    private int p1_mana;
    private int p2_mana;
    public bool oneTime = false;
    private int p1MaxMana;
    private int p2MaxMana;
    public static TurnStatus turnStatus;
    [SerializeField] private Text buttonText;
    [SerializeField] private GameObject button;
    public static List<Card> PAttackFields;
    public static List<Card> PDefenceFields;
    public static ObservableCollection<Card> PAllFields;
    public static List<Card> PHands;
    public GameObject checkPanel;
    public GameObject detailPanel;
    public static List<bool> p1CannotDrawEffectList;
    public static List<bool> p2CannotDrawEffectList;
    public bool p1_turn;
    public static GameObject attackObject;
    public static GameObject defenceObject;
    public Text someTexts;
    public List<Text> cardDetailText;
    public Text P1_manaText;
    public Text P2_manaText;
    private static System.Random rng = new System.Random();
    public GameObject gameSetPanel;
    public Text gameSetText;
    bool isEnemyTurnProcessing = false;
    bool isPlayerTurnProcessing = false;
    public bool isDealing = false;
    public bool nowDestory = false;
    UtilMethod utilMethod = new UtilMethod();
    public int P1_mana
    {
        get { return p1_mana; }
        set
        {
            if (p1_mana != value)
            {
                p1_mana = value;
                P1_manaText.text = manaChange();
                StartCoroutine(OnP1ManaChanged());
            }
        }
    }

    public int P1MaxMana
    {
        get { return p1MaxMana; }
        set
        {
            if (p1MaxMana != value)
            {
                p1MaxMana = value;
                P1_manaText.text = manaChange();
            }
        }

    }

    public int P2_mana
    {
        get { return p2_mana; }
        set
        {
            if (p2_mana != value)
            {
                p2_mana = value;
                P2_manaText.text = P2manaChange();
            }
        }
    }

    public int P2MaxMana
    {
        get { return p2MaxMana; }
        set
        {
            if (p2MaxMana != value)
            {
                p2MaxMana = value;
                P2_manaText.text = P2manaChange();
            }
        }
    }

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
        PHands = new List<Card>();
        p1_turnElapsed = 0;
        p2_turnElapsed = 0;
        P1canDraw = true;
        P2canDraw = true;
        finishEnemyTurn = true;
        PAttackFields = new List<Card>();
        PDefenceFields = new List<Card>();
        PAllFields = new ObservableCollection<Card>();
        PAllFields.CollectionChanged += CollectionChanged;
        completeButtonChoice = false;
        canPlayDefenceCard = true;
        p1CannotDrawEffectList = new List<bool>();
        p2CannotDrawEffectList = new List<bool>();
        P1_mana = 0;
        P2_mana = 0;
        P1MaxMana = P1_mana;
        P2MaxMana = P2_mana;

        turnChange();
        Transform leaderTransform = canvas.transform.Find("myLeader");
        myLeader = Instantiate(leaderPrefab, leaderTransform, false);
        myLeader.GetComponent<Leader>().P1SetUp(myLeaderInf);
        enemyAI.leader = myLeader;

        Transform enemyLeaderTransform = canvas.transform.Find("enemyLeader");
        enemyLeader = Instantiate(enemyLeaderPrefab, enemyLeaderTransform, false);
        enemyAI.leader = myLeader;
        enemyLeader.GetComponent<Leader>().P2SetUp(enemyLeaderInf);

        Shuffle(myDeckInf);
        Shuffle(enemyDeckInf);

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

        enemyAI.Instantiate();

    }

    public static void Shuffle<T>(List<T> list)
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
        if (!p1_turn && !isEnemyTurnProcessing)
        {
            isEnemyTurnProcessing = true;
            turnChange();
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

        if (P1_mana > P1MaxMana)
        {
            P1_mana = P1MaxMana;
            P1_manaText.text = manaChange();
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (detailPanel.activeSelf)
            {
                detailPanel.SetActive(false);
            }
        }
    }

    async void StartEnemyTurnAsync()
    {
        await WaitEnemyTurn();
        isEnemyTurnProcessing = false;
    }

    async void StartPlayerTurnAsync()
    {
        await WaitYourTurn();
        isPlayerTurnProcessing = false;
    }

    public string manaChange()
    {
        string manaAmount = "マナ:" + P1_mana + "/" + P1MaxMana;
        return manaAmount;
    }

    public string P2manaChange()
    {
        string manaAmount = "マナ:" + P2_mana + "/" + P2MaxMana;
        return manaAmount;
    }


    public void drawCard()
    {
        if (p1CannotDrawEffectList.Count == 0)
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
            P2MaxMana += 100;
            P2_mana = P2MaxMana;
            p2_turnElapsed++;
            turnStatus = TurnStatus.OnEnemyTurnStart;
            if (EnemyAI.EAllFields != null)
            {
                foreach (Card card in EnemyAI.EAllFields)
                {
                    for (int c = 0; c < card.attackedList.Count; c++)
                    {
                        card.attackedList[c] = false;
                    }
                }
            }
        }
        else
        {
            p1_turn = true;
            P1MaxMana += 100;
            P1_mana = P1MaxMana;
            P1_manaText.text = manaChange();
            p1_turnElapsed++;
            if (PAllFields != null)
            {
                foreach (Card card in PAllFields)
                {
                    for (int c = 0; c < card.attackedList.Count; c++)
                    {
                        card.attackedList[c] = false;
                    }
                }
            }

        }
    }

    public async void nextPlay()
    {
        await WaitUntilFalse(() => isDealing);
        AudioManager.Instance.TurnEndButtonSound();
        if (turnStatus == TurnStatus.OnPlay)
        {
            turnStatus = TurnStatus.OnAttack;
            buttonText.text = "ターンエンド";
            foreach (Card card in PAllFields)
            {
                card.ActivePanel.SetActive(!card.Attacked);
            }
        }
        else if (turnStatus == TurnStatus.OnAttack)
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

            Invoke("turnChange", 1.0f);
            button.SetActive(false);
        }
    }

    private async Task WaitYourTurn()
    {

        // GameObjectをオフにする
        panel.SetActive(true);
        someTexts.text = "Your Turn";

        // さらに一定時間待機
        await Task.Delay(1000);
        await TriggerEffectsAsync(EnemyAI.EAllFields.ToList(), EffectInf.CardTrigger.OnEnemyTurnStart);
        await TriggerEffectsAsync(PAllFields.ToList(), EffectInf.CardTrigger.OnTurnStart);
        panel.SetActive(false);
        drawCard();
        turnStatus = TurnStatus.OnPlay;
        foreach (var card in PHands)
        {
            GameObject cardObject = card.gameObject;
            if (card.cost > P1_mana)
            {
                cardObject.GetComponent<CardDragAndDrop>().canDrag = false;
            }
            else
            {
                cardObject.GetComponent<CardDragAndDrop>().canDrag = true;
            }
            if (CardManager.P1CannotPlayDefenceCard.Count != 0 && card.inf.cardType == CardType.Defence)
            {
                cardObject.GetComponent<CardDragAndDrop>().canDrag = false;
            }
            foreach (var effect in card.inf.effectInfs)
            {
                for (int i = 0; i < effect.triggers.Count; i++)
                {
                    if (effect.triggers[i] == EffectInf.CardTrigger.OnBeginDrag)
                    {
                        if (effect is ICardEffect cardEffect)
                        {
                            await cardEffect.Apply(new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields
                            , GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
                        }
                    }
                }
            }
            card.ActivePanel.SetActive(cardObject.GetComponent<CardDragAndDrop>().canDrag);

        }
        button.SetActive(true);
        buttonText.text = "攻撃フェーズへ";

    }

    private async Task WaitEnemyTurn()
    {
        // Trigger effects at the start of the enemy's turn
        await TriggerEffectsAsync(PAllFields.ToList(), EffectInf.CardTrigger.OnEnemyTurnStart);
        await TriggerEffectsAsync(EnemyAI.EAllFields.ToList(), EffectInf.CardTrigger.OnTurnStart);

        // Enemy AI actions
        enemyAI.drawCard(enemyDeckIndex, allCardInf.allList[enemyDeckInf[enemyDeckIndex]], canvas, enemyCardPrefab, this);
        turnStatus = TurnStatus.OnEnemyOnPlay;
        await enemyAI.PlayCard(this.GetComponent<GameManager>());
        turnStatus = TurnStatus.OnEnemyAttack;
        await enemyAI.Attack();
        turnStatus = TurnStatus.OnEnemyTurnEnd;

        // Increment elapsed turns for enemy cards
        foreach (var card in EnemyAI.EAllFields)
        {
            card.elapsedTurns++;
        }

        // Trigger effects at the end of the enemy's turn
        await TriggerEffectsAsync(PAllFields.ToList(), EffectInf.CardTrigger.OnEnemyTurnEnd);
        await TriggerEffectsAsync(EnemyAI.EAllFields.ToList(), EffectInf.CardTrigger.OnTurnEnd);
        await TriggerEffectsAsync(CardManager.P1SpelEffectAfterSomeTurn, EffectInf.CardTrigger.SpelEffectSomeTurn);

        finishEnemyTurn = true;

        // Wait until the enemy turn is finished
        await WaitUntilAsync(() => finishEnemyTurn);
        oneTime = false;
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
            await Task.Yield(); // Unityエンジンのフレームワーク上で実行するために次のフレームを待機
        }
    }

    private async Task WaitUntilFalse(Func<bool> condition)
    {
        while (condition())
        {
            await Task.Yield(); // Unityエンジンのフレームワーク上で実行するために次のフレームを待機
        }
    }
    private async void TriggerEffects(IEnumerable<Card> fields, EffectInf.CardTrigger trigger)
    {
        foreach (var card in fields)
        {
            foreach (var effect in card.inf.effectInfs)
            {
                if (effect.triggers.Contains(trigger))
                {
                    if (effect is ICardEffect cardEffect)
                    {
                        await cardEffect.Apply(new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields, PAllFields, PAttackFields, PDefenceFields));
                    }
                }
            }
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
            EffectEndDeal();
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
    }

    private void ResetStatus(Card attackCard)
    {
        attackCard.attackPre = false;
        attackObject = null;
        defenceObject = null;
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
                        if (attackCard.CardOwner == PlayerID.Player1)
                        {
                            CardManager.P1EffectDuringAttacking.Add(attackCard);
                        }
                        else if (attackCard.CardOwner == PlayerID.Player2)
                        {
                            CardManager.P2EffectDuringAttacking.Add(attackCard);
                        }
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

        if (CardManager.P1EffectDuringAttacking.Count != 0)
        {
            for (int i = 0; i < CardManager.P1EffectDuringAttacking[0].inf.effectInfs.Count; i++)
            {
                foreach (EffectInf.CardTrigger cardTrigger in CardManager.P1EffectDuringAttacking[0].inf.effectInfs[i].triggers)
                {
                    if (cardTrigger == EffectInf.CardTrigger.EndAttack)
                    {
                        if (CardManager.P1EffectDuringAttacking[0].inf.effectInfs[i] is ICardEffect cardEffect)
                        {
                            await cardEffect.Apply(new ApplyEffectEventArgs(CardManager.P1EffectDuringAttacking[0], EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields
                            , PAllFields, PAttackFields, PDefenceFields));
                        }
                    }
                }
            }
            CardManager.P1EffectDuringAttacking[0] = null;
        }
        else if (CardManager.P2EffectDuringAttacking.Count != 0 && CardManager.P2EffectDuringAttacking[0] != null)
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
                            , PAllFields, PAttackFields, PDefenceFields));
                        }
                    }
                }
            }
            CardManager.P2EffectDuringAttacking[0] = null;
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
    private IEnumerator OnP1ManaChanged()
    {
        for (int i = PHands.Count - 1; i >= 0; i--)
        {
            var card = PHands[i];
            GameObject cardObject = card.gameObject;

            // コストがP1_manaより大きい場合、カードをドラッグ不可に設定
            if (card.cost > P1_mana)
            {
                cardObject.GetComponent<CardDragAndDrop>().canDrag = false;
            }
            else
            {
                cardObject.GetComponent<CardDragAndDrop>().canDrag = true;
            }

            // P1CannotPlayDefenceCardが存在し、カードが防御タイプの場合、ドラッグ不可に設定
            if (CardManager.P1CannotPlayDefenceCard.Count != 0 && card.inf.cardType == CardType.Defence)
            {
                cardObject.GetComponent<CardDragAndDrop>().canDrag = false;
            }

            // カードのY座標が180より大きい場合、ドラッグ不可に設定
            if (card.transform.position.y > 180)
            {
                cardObject.GetComponent<CardDragAndDrop>().canDrag = false;
            }

            // カードのエフェクトに対するトリガー処理
            foreach (var effect in card.inf.effectInfs)
            {
                for (int j = 0; j < effect.triggers.Count; j++)
                {
                    if (effect.triggers[j] == EffectInf.CardTrigger.OnBeginDrag)
                    {
                        if (effect is ICardEffect cardEffect)
                        {
                            ApplyEffectEventArgs args = new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                                GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields);
                            yield return StartCoroutine(ApplyEffectCoroutine(cardEffect, args));
                        }
                    }
                }
            }

            // カードのActivePanelの状態をcanDragプロパティに基づいて設定
            card.ActivePanel.SetActive(cardObject.GetComponent<CardDragAndDrop>().canDrag);
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

    private IEnumerator ApplyEffectAsync(ICardEffect cardEffect, Card card)
    {
        Task applyEffectTask = cardEffect.Apply(new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields
            , GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
        yield return new WaitUntil(() => applyEffectTask.IsCompleted);
    }

    private IEnumerator ApplyEffectCoroutine(ICardEffect cardEffect, ApplyEffectEventArgs args)
    {
        Task applyEffectTask = cardEffect.Apply(args);
        // Task の完了を待機
        yield return new WaitUntil(() => applyEffectTask.IsCompleted);
    }

}

