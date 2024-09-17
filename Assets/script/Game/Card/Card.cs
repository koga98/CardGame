using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using GameNamespace;
using System.Threading.Tasks;

public enum PlayerID
{
    Player1,
    Player2
}

public class Card : MonoBehaviour, IPointerClickHandler
{
    public GameObject ActivePanel;
    public GameObject attackPanel;
    public GameObject defencePanel;
    public GameObject blindPanel;
    public GameObject shiledIcon;
    GameObject manager;
    GameManager gameManager;
    CardManager player1CardManager;
    CardManager player2CardManager;
    EffectManager effectManager;
    UIManager uIManager;
    DekeMakeUIManager dekeMakeUIManager;
    AttackManager attackManager;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI costText;
    public Image cardImage;
    public Image backColor;
    public Sprite sprite;
    public int elapsedTurns;
    public PlayerID CardOwner;
    public CardInf inf;
    public Color baseColor;
    public int attack;
    public int hp;
    public int maxHp;
    public int cost;
    public bool canAttackTarget = true;
    public bool canAvoidAttack;
    private bool attacked = false;
    public bool attackPre = false;
    public ObservableCollection<bool> attackedList;

    public bool Attacked
    {
        get { return attacked; }
        set
        {
            if (attacked != value)
            {
                attacked = value;
                OnChangeAttacked();
            }
        }
    }

    private void OnChangeAttacked()
    {
        if (GameManager.turnStatus == GameManager.TurnStatus.OnAttack && CardOwner == PlayerID.Player1)
        {
            ActivePanel.SetActive(!attacked);
        }
    }

    void Start()
    {
        InitializeManagers(SceneManager.GetActiveScene().name);
        InitializeVariables();
    }

    private void InitializeManagers(string sceneName)
    {
        if (sceneName == "playGame")
        {
            manager = GameObject.Find("GameManager");
            gameManager = manager.GetComponent<GameManager>();
            uIManager = manager.GetComponent<UIManager>();
            attackManager = manager.GetComponent<AttackManager>();
            effectManager = manager.GetComponent<EffectManager>();
        }
        else if (sceneName == "makeDeck")
        {
            manager = GameObject.Find("GameObject");
            dekeMakeUIManager = manager.GetComponent<DekeMakeUIManager>();
        }
    }

    private void InitializeVariables()
    {
        canAvoidAttack = false;
        canAttackTarget = true;
        elapsedTurns = 0;
    }

    void Update()
    {
        if (maxHp < hp)
        {
            hp = maxHp;
        }

    }

    public void P1SetUp(CardInf cardInf)
    {
        CardManager cardManager = null;
        if(GameObject.Find("P1CardManager") != null)
            cardManager = GameObject.Find("P1CardManager").GetComponent<CardManager>();
        SetUp(cardInf, PlayerID.Player1,cardManager);
    }

    public void P2SetUp(CardInf cardInf)
    {
        CardManager cardManager = null;
        if(GameObject.Find("P2CardManager") != null)
            cardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
        SetUp(cardInf, PlayerID.Player2, cardManager);
    }

    public void SetUp(CardInf cardInf, PlayerID owner, CardManager cardManager = null)
    {
        attackedList = new ObservableCollection<bool> { true };
        attackedList.CollectionChanged += OnAttackedListChanged;

        inf = cardInf;
        attackText.text = cardInf.attack.ToString();
        healthText.text = cardInf.hp.ToString();
        costText.text = cardInf.cost.ToString();
        cardImage.sprite = cardInf.cardSprite;
        sprite = cardInf.cardSprite;
        attack = cardInf.attack;
        hp = cardInf.hp;
        maxHp = cardInf.hp;
        cost = cardInf.cost;
        attackedList[0] = true;
        CardOwner = owner;

        if (SceneManager.GetActiveScene().name == "playGame")
        {
            if (CardOwner == PlayerID.Player1)
                player1CardManager = cardManager;
            else
                player2CardManager = cardManager;
        }

        if (CardOwner == PlayerID.Player2)
        {
            blindPanel.SetActive(true);
        }

        if (inf.cardType == CardType.Spel)
        {
            backColor.color = Color.blue;
            attackPanel.SetActive(false);
            defencePanel.SetActive(false);
        }
        else if (inf.cardType == CardType.Attack)
        {
            backColor.color = Color.HSVToRGB(0.83f, 1.0f, 1.0f);
        }

        baseColor = backColor.color;
    }

    private void OnAttackedListChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (attackedList.Count > 0)
        {
            Attacked = attackedList[attackedList.Count - 1];
        }
    }

    public void reflectAmount(int amount)
    {
        amountText.text = "×" + amount.ToString();
    }

    public async Task DestoryThis()
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        gameManager.nowDestory = true;
        if (CardOwner == PlayerID.Player1)
        {
            await DestroyCard(player1CardManager);
        }
        else if (CardOwner == PlayerID.Player2)
        {
            await DestroyCard(player2CardManager);
        }
        await effectManager.EffectAfterDie(this);
        Destroy(this.gameObject);
        gameManager.nowDestory = false;
    }

    private async Task DestroyCard(CardManager cardManager)
    {
        if (cardManager.AllFields.Contains(this))
        {
            cardManager.AllFields.Remove(this);
            if (this.inf.cardType == CardType.Defence)
            {
                cardManager.DefenceFields.Remove(this);
            }
            else if (this.inf.cardType == CardType.Attack)
            {
                cardManager.AttackFields.Remove(this);
            }
        }

        var cardLists = new List<List<Card>> {
        cardManager.CardsWithEffectOnField,
        cardManager.SpelEffectAfterSomeTurn,
        cardManager.CannotAttackMyDefenceCard,
        cardManager.CardsWithProtectEffectOnField,
        cardManager.CannotPlayDefenceCard,
        cardManager.EffectDuringAttacking
    };

        foreach (var list in cardLists)
        {
            if (list.Contains(this))
            {
                list.Remove(this);
            }
        }

        await Task.Yield();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (GameManager.defenceObject != null && !gameManager.restrictionClick && !gameManager.nowDestory && !gameManager.isDealing && GameManager.turnStatus == GameManager.TurnStatus.OnAttack)
            {
                gameManager.restrictionClick = true;
                StartCoroutine(OnPointerClickCoroutine());
            }
            else
            {
                if (SceneManager.GetActiveScene().name == "playGame")
                    uIManager.DetailPanelActive(inf);

                else if (SceneManager.GetActiveScene().name == "makeDeck")
                    dekeMakeUIManager.DetailPanelActive(inf);
            }
        }
    }

    private IEnumerator OnPointerClickCoroutine()
    {
        // GameManagerを取得
        if (GameManager.defenceObject != null)
        {
            // 非同期メソッドを実行し、その完了を待つ
            yield return StartCoroutine(attackManager.AttackCardCoroutine());
        }
    }
}
