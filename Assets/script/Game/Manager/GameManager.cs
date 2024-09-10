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
    public CardManager player1CardManager;
    public CardManager player2CardManager;
    public ManaManager manaManager;
    public UIManager uIManager;
    public GameObject leaderPrefab;
    public GameObject enemyLeaderPrefab;
    public AllCardInf allCardInf;
    [SerializeField] private LeaderInf myLeaderInf;
    [SerializeField] private LeaderInf enemyLeaderInf;
    public GameObject canvas;
    [SerializeField] EnemyAI enemyAI;
    public bool finishEnemyTurn;
    public GameObject myLeader;
    public GameObject enemyLeader;
    public static bool completeButtonChoice;
    public int p1_turnElapsed;
    public int p2_turnElapsed;
    public bool oneTime = false;
    public static TurnStatus turnStatus;
    public bool p1_turn;
    public static GameObject attackObject;
    public static GameObject defenceObject;
    bool isEnemyTurnProcessing = false;
    public bool isPlayerTurnProcessing = false;
    public bool isDealing = false;
    public bool nowDestory = false;
    public bool restrictionClick;
    public bool nowEnemyAttack;

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
        InitializeGameSettings();
        DecideTurnOrder();
        LeaderSetUp();
    }

    private void DecideTurnOrder()
    {
        System.Random random = new System.Random();
        bool isP1First = random.Next(2) == 0;
        p1_turn = isP1First;

    }

    void Update()
    {
        ManageEachTurns();
        if (Input.GetMouseButtonDown(0))
        {
            uIManager.DetailPanelInactive();
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
        
        AudioManager.Instance.TurnEndButtonSound();
        if (turnStatus == TurnStatus.OnPlay)
        {
            AttackPhaze();
        }
        else if (turnStatus == TurnStatus.OnAttack)
        {
            uIManager.phazeOperateButton.SetActive(false);
            await WaitUntilFalse(() => isDealing);
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
        uIManager.messagePanel.SetActive(true);
        uIManager.ChangeMessageTexts("Your Turn");
        // さらに一定時間待機
        await Task.Delay(1000);
        turnStatus = TurnStatus.OnTurnStart;
        p1_turnElapsed++;
        player1CardManager.drawCard();
        manaManager.P1TurnStart();
        await TriggerEffectsAsync(player2CardManager.AllFields.ToList(), EffectInf.CardTrigger.OnEnemyTurnStart);
        await TriggerEffectsAsync(player1CardManager.AllFields.ToList(), EffectInf.CardTrigger.OnTurnStart);
        uIManager.messagePanel.SetActive(false);
        
        turnStatus = TurnStatus.OnPlay;
        uIManager.phazeOperateButton.SetActive(true);
        uIManager.ChangePhazeOperateButtonText("攻撃フェーズへ");
    }

    private async Task WaitEnemyTurn()
    {
        turnStatus = TurnStatus.OnEnemyTurnStart;
        p2_turnElapsed++;
        player2CardManager.drawCard();
        manaManager.P2TurnStart();
        await TriggerEffectsAsync(player1CardManager.AllFields.ToList(), EffectInf.CardTrigger.OnEnemyTurnStart);
        await TriggerEffectsAsync(player2CardManager.AllFields.ToList(), EffectInf.CardTrigger.OnTurnStart);

        

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
        foreach (Card card in player1CardManager.AllFields)
        {
            card.ActivePanel.SetActive(!card.Attacked);
        }
    }

    private async Task TurnEndPhaze()
    {
        //効果処理が終わってから作動するようにしないとダメ
        turnStatus = TurnStatus.OnTurnEnd;
        foreach (var card in player1CardManager.AllFields)
        {
            card.elapsedTurns++;
        }
        await TriggerEffectsAsync(player1CardManager.AllFields.ToList(), EffectInf.CardTrigger.OnTurnEnd);
        await TriggerEffectsAsync(player2CardManager.AllFields.ToList(), EffectInf.CardTrigger.OnEnemyTurnEnd);
        await TriggerEffectsAsync(player2CardManager.SpelEffectAfterSomeTurn, EffectInf.CardTrigger.SpelEffectSomeTurn);

        foreach (var card in player1CardManager.AllFields)
        {
            card.ActivePanel.SetActive(false);
        }

        turnChange();
        uIManager.phazeOperateButton.SetActive(false);
    }

    private async Task EnemyTurnEndPhaze()
    {
        foreach (var card in player2CardManager.AllFields)
        {
            card.elapsedTurns++;
        }

        await TriggerEffectsAsync(player1CardManager.AllFields.ToList(), EffectInf.CardTrigger.OnEnemyTurnEnd);
        await TriggerEffectsAsync(player2CardManager.AllFields.ToList(), EffectInf.CardTrigger.OnTurnEnd);
        await TriggerEffectsAsync(player2CardManager.SpelEffectAfterSomeTurn, EffectInf.CardTrigger.SpelEffectSomeTurn);

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
                    await cardEffect.Apply(new ApplyEffectEventArgs(card, player2CardManager.AllFields, player2CardManager.AttackFields, player2CardManager.DefenceFields, player1CardManager.AllFields, player1CardManager.AttackFields,player1CardManager.DefenceFields));
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

    private void LeaderSetUp()
    {
        Transform leaderTransform = canvas.transform.Find("myLeader");
        myLeader = Instantiate(leaderPrefab, leaderTransform, false);
        myLeader.GetComponent<Leader>().P1SetUp(myLeaderInf);
        enemyAI.leader = enemyLeader;

        Transform enemyLeaderTransform = canvas.transform.Find("enemyLeader");
        enemyLeader = Instantiate(enemyLeaderPrefab, enemyLeaderTransform, false);
        enemyAI.leader = myLeader;
        enemyLeader.GetComponent<Leader>().P2SetUp(enemyLeaderInf);
    }

    private void InitializeGameSettings()
    {
        nowEnemyAttack = false;
        restrictionClick = false;
        p1_turnElapsed = 0;
        p2_turnElapsed = 0;
        finishEnemyTurn = true;
        completeButtonChoice = false;
    }

    private async void EffectEndDeal()
    {
        await EffectProcess(player1CardManager.EffectDuringAttacking);
        await EffectProcess(player2CardManager.EffectDuringAttacking);
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
                            await cardEffect.Apply(new ApplyEffectEventArgs(cards[0], player2CardManager.AllFields, player2CardManager.AttackFields, player2CardManager.DefenceFields
                            , player1CardManager.AllFields, player1CardManager.AttackFields, player1CardManager.DefenceFields));
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

}

