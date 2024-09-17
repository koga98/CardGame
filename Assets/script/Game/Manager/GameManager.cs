using UnityEngine;
using System;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    public CardManager player1CardManager;
    public CardManager player2CardManager;
    public ManaManager manaManager;
    public EffectManager effectManager;
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
    public bool isGameOver;

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

    private void InitializeGameSettings()
    {
        nowEnemyAttack = false;
        restrictionClick = false;
        isGameOver = false;
        p1_turnElapsed = 0;
        p2_turnElapsed = 0;
        finishEnemyTurn = true;
        completeButtonChoice = false;
    }

    private void DecideTurnOrder()
    {
        System.Random random = new System.Random();
        bool isP1First = random.Next(2) == 0;
        p1_turn = isP1First;
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

    void Update()
    {
        if (!isGameOver)
        {
            ManageEachTurns();
            if (Input.GetMouseButtonDown(0))
                uIManager.DetailPanelInactive();
        }

    }

    private void ManageEachTurns()
    {
        if (!p1_turn && !isEnemyTurnProcessing)
        {
            isEnemyTurnProcessing = true;
            finishEnemyTurn = false;
            StartEnemyTurn();
        }

        if (p1_turn && !oneTime && !isPlayerTurnProcessing)
        {
            isPlayerTurnProcessing = true;
            oneTime = true;
            turnStatus = TurnStatus.OnTurnStart;
            StartPlayerTurn();
        }
    }

    async void StartPlayerTurn()
    {
        await SetUpYourTurn();
        isPlayerTurnProcessing = false;
    }

    private async Task SetUpYourTurn()
    {
        // GameObjectをオフにする
        uIManager.messagePanel.SetActive(true);
        uIManager.ChangeMessageTexts("Your Turn");
        // さらに一定時間待機
        await Task.Delay(1000);
        await TurnStartPhaze();
        uIManager.messagePanel.SetActive(false);

        turnStatus = TurnStatus.OnPlay;
        uIManager.phazeOperateButton.SetActive(true);
        uIManager.ChangePhazeOperateButtonText("攻撃フェーズへ");
    }

    private async Task TurnStartPhaze()
    {
        turnStatus = TurnStatus.OnTurnStart;
        p1_turnElapsed++;
        player1CardManager.drawCard();
        manaManager.P1TurnStart();
        await effectManager.TurnStartEffect(p1_turn);
    }

    public void AttackPhaze()
    {
        turnStatus = TurnStatus.OnAttack;
        uIManager.ChangePhazeOperateButtonText("ターンエンド");
        foreach (Card hands in player1CardManager.Hands)
            hands.ActivePanel.SetActive(false);
        foreach (Card card in player1CardManager.AllFields)
            card.ActivePanel.SetActive(!card.Attacked);
    }

    public async Task TurnEndPhaze()
    {
        //効果処理が終わってから作動するようにしないとダメ
        turnStatus = TurnStatus.OnTurnEnd;
        foreach (var card in player1CardManager.AllFields)
            card.elapsedTurns++;
        await effectManager.TurnEndEffect(p1_turn);
        foreach (var card in player1CardManager.AllFields)
            card.ActivePanel.SetActive(false);

        turnChange();
        uIManager.phazeOperateButton.SetActive(false);
    }

    async void StartEnemyTurn()
    {
        await WaitEnemyTurn();
    }

    private async Task WaitEnemyTurn()
    {
        uIManager.messagePanel.SetActive(true);
        uIManager.ChangeMessageTexts("Enemy Turn");
        // さらに一定時間待機
        await Task.Delay(1000);
        uIManager.messagePanel.SetActive(false);
        turnStatus = TurnStatus.OnEnemyTurnStart;
        p2_turnElapsed++;
        player2CardManager.drawCard();
        manaManager.P2TurnStart();
        await effectManager.TurnStartEffect(p1_turn);
        turnStatus = TurnStatus.OnEnemyOnPlay;
        await enemyAI.PlayCard(manaManager);
        turnStatus = TurnStatus.OnEnemyAttack;
        await enemyAI.Attack();
        if(isGameOver)
            return;
        turnStatus = TurnStatus.OnEnemyTurnEnd;
        await EnemyTurnEndPhaze();
    }

    private async Task EnemyTurnEndPhaze()
    {
        foreach (var card in player2CardManager.AllFields)
            card.elapsedTurns++;

        await effectManager.TurnEndEffect(p1_turn);
        finishEnemyTurn = true;
        await WaitUntilFinishEnemyTurn(() => finishEnemyTurn);
        oneTime = false;
        isEnemyTurnProcessing = false;
        turnChange();
    }

    private async Task WaitUntilFinishEnemyTurn(Func<bool> condition)
    {
        while (!condition())
            await Task.Yield();
    }

    private void turnChange()
    {
        if (p1_turn == true)
            p1_turn = false;
        else
            p1_turn = true;
    }

}

