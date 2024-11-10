using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private CardManager player1CardManager;
    public CardManager player2CardManager;
    public ManaManager manaManager;
    public EffectManager effectManager;
    public TutorialManager tutorialManager;
    public UIManager uIManager;
    public GameObject leaderPrefab;
    public GameObject enemyLeaderPrefab;
    public AllCardInf allCardInf;
    [SerializeField]
    TurnManager turnManager;
    [SerializeField] private LeaderInf myLeaderInf;
    [SerializeField] private LeaderInf enemyLeaderInf;
    public GameObject canvas;
    [SerializeField] EnemyAI enemyAI;
    public bool finishEnemyTurn;
    public GameObject myLeader;
    public GameObject enemyLeader;
    public Slider bgmSlider;
    public Slider SeSlider;
    public Slider voiceSlider;
    public static bool completeButtonChoice;
    public int p1_turnElapsed;
    public int p2_turnElapsed;
    public bool oneTime = false;
    public static TurnStatus turnStatus;
    public static GameObject attackObject;
    public static GameObject defenceObject;
    bool isEnemyTurnProcessing = false;
    public bool isPlayerTurnProcessing = false;
    public bool isDealing = false;
    public bool nowDestory = false;
    public bool restrictionClick;
    public bool nowEnemyAttack;
    public bool isGameOver;
    public bool isTutorial;
    public bool nowCollectionChanging;
    string filePath;
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
        LeaderSetUp();
        if(!isTutorial){
            turnManager.DecideTurnOrder();
            player2CardManager.EnemyDeckCreate();
            player1CardManager.P1DeckCreate();
        }
        else if(isTutorial){
            isGameOver = false;
            turnManager.TutorialTurnOrder();
            player1CardManager.TutorialDeckCreate();
            player2CardManager.TutorialDeckCreate();
        }
        
        SoundSet();
    }

    private void InitializeGameSettings()
    {
        filePath = Application.persistentDataPath + "/"  + "SoundData.json";
        nowEnemyAttack = false;
        restrictionClick = false;
        isGameOver = false;
        p1_turnElapsed = 0;
        p2_turnElapsed = 0;
        finishEnemyTurn = true;
        completeButtonChoice = false;
        nowCollectionChanging = false;
        isTutorial = SceneManager.GetActiveScene().name == "tutorial" ? true: false;
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
        if (!isGameOver && !isTutorial)
        {
            ManageEachTurns();
            if (Input.GetMouseButtonDown(0))
                uIManager.DetailPanelInactive();
        }

    }

    private void ManageEachTurns()
    {
        if (!turnManager.IsPlayerTurn && !isEnemyTurnProcessing)
        {
            isEnemyTurnProcessing = true;
            finishEnemyTurn = false;
            StartEnemyTurn();
        }

        if (turnManager.IsPlayerTurn && !oneTime && !isPlayerTurnProcessing)
        {
            isPlayerTurnProcessing = true;
            oneTime = true;
            turnStatus = TurnStatus.OnTurnStart;
            StartPlayerTurn();
        }
    }

    async void StartPlayerTurn()
    {
        await WaitNextPanel(() => uIManager.messagePanel.activeSelf);
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
        await effectManager.TurnStartEffect(turnManager.IsPlayerTurn);
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
        if (player1CardManager?.AllFields != null)
            foreach (var card in player1CardManager.AllFields)
            {
                card.gameObject.GetComponent<CardAnimation>().CancelAttackPrepareAnim();
                card.elapsedTurns++;
            }
        await effectManager.TurnEndEffect(turnManager.IsPlayerTurn);
        if (player1CardManager?.AllFields != null)
            foreach (var card in player1CardManager.AllFields)
            {
                card.ActivePanel.SetActive(false);

            }

        turnManager.ToggleTurn();
        uIManager.phazeOperateButton.SetActive(false);
    }

    async void StartEnemyTurn()
    {
        await WaitNextPanel(() => uIManager.messagePanel.activeSelf);
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
        await effectManager.TurnStartEffect(turnManager.IsPlayerTurn);
        turnStatus = TurnStatus.OnEnemyOnPlay;
        await enemyAI.PlayCard(manaManager);
        turnStatus = TurnStatus.OnEnemyAttack;
        await enemyAI.Attack();
        if (isGameOver)
            return;
        turnStatus = TurnStatus.OnEnemyTurnEnd;
        await EnemyTurnEndPhaze();
    }

    private async Task EnemyTurnEndPhaze()
    {
        foreach (var card in player2CardManager.AllFields)
            card.elapsedTurns++;

        await effectManager.TurnEndEffect(turnManager.IsPlayerTurn);
        finishEnemyTurn = true;
        await WaitUntilFinishEnemyTurn(() => finishEnemyTurn);
        oneTime = false;
        isEnemyTurnProcessing = false;
        turnManager.ToggleTurn();
    }

    private async Task WaitUntilFinishEnemyTurn(Func<bool> condition)
    {
        while (!condition())
            await Task.Yield();
    }

    private async Task WaitNextPanel(Func<bool> condition)
    {
        while (condition())
            await Task.Yield();
    }

    

    public void SoundSet()
    {
        if (File.Exists(filePath))
        {
            StreamReader streamReader = new StreamReader(filePath);
            string data = streamReader.ReadToEnd();
            streamReader.Close();
            TitleGamemanager.soundDataBase = JsonUtility.FromJson<SoundDataBase>(data);

            if (TitleGamemanager.soundDataBase != null && TitleGamemanager.soundDataBase.soundSetting.Count > 0)
            {
                bgmSlider.value = TitleGamemanager.soundDataBase.soundSetting[0];
                SeSlider.value = TitleGamemanager.soundDataBase.soundSetting[1];
                voiceSlider.value = TitleGamemanager.soundDataBase.soundSetting[2];
                AudioManager.Instance.audioSource.volume = bgmSlider.value;
                AudioManager.Instance.SEaudioSource.volume = SeSlider.value;
                AudioManager.Instance.voiceSource.volume = voiceSlider.value;
            }
            else
                Debug.LogError("Failed to load deck data.");
        }
    }

}

