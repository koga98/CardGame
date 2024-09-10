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
    CardManager player1CardManager;
    CardManager player2CardManager;
    EffectManager effectManager;
    UIManager uIManager;
    AttackManager attackManager;
    public bool canAvoidAttack;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI costText;
    public Image cardImage;
    public Image backColor;
    public Sprite sprite;
    public int elapsedTurns;
    public PlayerID CardOwner;
    GameObject manager;
    GameManager gameManager;
    //攻撃対象になるかを決めるためのもの
    public bool canAttackTarget = true;
    public bool effectApplied = false;
    public event Action<List<Card>> OnMultipleCardEffect;
    public CardInf inf;
    public int attack;
    public int hp;
    public int maxHp;
    public int cost;
    //過去形のattackedです
    public ObservableCollection<bool> attackedList;
    private bool attacked = false;
    public bool attackPre = false;
    private DeckMake deckMake;

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

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "playGame")
        {
            manager = GameObject.Find("GameManager");
            gameManager = manager.GetComponent<GameManager>();
            uIManager = manager.GetComponent<UIManager>();
            attackManager = manager.GetComponent<AttackManager>();
            effectManager = manager.GetComponent<EffectManager>();
        }
        else if (SceneManager.GetActiveScene().name == "makeDeck")
        {
            manager = GameObject.Find("GameObject");
            uIManager = manager.GetComponent<UIManager>();
            deckMake = manager.GetComponent<DeckMake>();
        }

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

    public void MultipleCardEffect(List<Card> cards)
    {
        OnMultipleCardEffect?.Invoke(cards);
    }

    public void P1SetUp(CardInf cardInf)
    {
        attackedList = new ObservableCollection<bool>
        {
            true
        };
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
        CardOwner = PlayerID.Player1;
        if (SceneManager.GetActiveScene().name == "playGame")
        {
            player1CardManager = GameObject.Find("P1CardManager").GetComponent<CardManager>();
        }
        if (inf.cardType == CardType.Spel)
        {

            backColor.color = Color.blue;

            attackPanel.SetActive(false);
            defencePanel.SetActive(false);
        }
    }

    public void P2SetUp(CardInf cardInf)
    {
        attackedList = new ObservableCollection<bool>
        {
            true
        };
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
        CardOwner = PlayerID.Player2;
        if (SceneManager.GetActiveScene().name == "playGame")
        {
            player2CardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
        }
        blindPanel.SetActive(true);
        if (inf.cardType == CardType.Spel)
        {
            backColor.color = Color.blue;
            attackPanel.SetActive(false);
            defencePanel.SetActive(false);
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
            await P1CardDestory();
        }
        else if (CardOwner == PlayerID.Player2)
        {
            await P2CardDestory();
        }
        Debug.Log("終わり");
        await effectManager.EffectAfterDie(this);
        Destroy(this.gameObject);
        gameManager.nowDestory = false;
    }

    private async Task P1CardDestory()
    {
        if (player1CardManager.AllFields.Contains(this))
        {
            player1CardManager.AllFields.Remove(this);
            if (this.inf.cardType == CardType.Defence)
            {
                player1CardManager.DefenceFields.Remove(this);
            }
            else if (this.inf.cardType == CardType.Attack)
            {
                player1CardManager.AttackFields.Remove(this);
            }
        }
        var cardLists = new List<List<Card>>
    {
    player1CardManager.CardsWithEffectOnField,
    player1CardManager.SpelEffectAfterSomeTurn,
    player1CardManager.CannotAttackMyDefenceCard,
    player1CardManager.CardsWithProtectEffectOnField,
    player1CardManager.CannotPlayDefenceCard,
    player1CardManager.EffectDuringAttacking
    };
        foreach (var list in cardLists)
        {
            if (list.Contains(this))
            {
                list.Remove(this);
            }
        }
        if (player1CardManager.CannotDrawEffectList.Contains(this))
        {
            player1CardManager.CannotDrawEffectList.Remove(this);
        }
        await Task.Yield();

    }

    private async Task P2CardDestory()
    {
        if (player2CardManager.AllFields.Contains(this))
        {
            player2CardManager.AllFields.Remove(this);
            if (this.inf.cardType == CardType.Defence)
            {
                player2CardManager.DefenceFields.Remove(this);
            }
            else if (this.inf.cardType == CardType.Attack)
            {
                player2CardManager.AttackFields.Remove(this);
            }
        }
        var cardLists = new List<List<Card>>
    {
    player2CardManager.CardsWithEffectOnField,
    player2CardManager.SpelEffectAfterSomeTurn,
    player2CardManager.CannotAttackMyDefenceCard,
    player2CardManager.CardsWithProtectEffectOnField,
    player2CardManager.CannotPlayDefenceCard,
    player2CardManager.EffectDuringAttacking
    };
        foreach (var list in cardLists)
        {
            if (list.Contains(this))
            {
                list.Remove(this);
            }
        }

        if (player2CardManager.CannotDrawEffectList.Contains(this))
        {
            player2CardManager.CannotDrawEffectList.Remove(this);
        }
        await Task.Yield();
    }

    private void OnAttackedListChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (attackedList.Count > 0)
        {
            Attacked = attackedList[attackedList.Count - 1];
        }
    }

    private void OnChangeAttacked()
    {
        if (GameManager.turnStatus == GameManager.TurnStatus.OnAttack && CardOwner == PlayerID.Player1)
        {
            ActivePanel.SetActive(!attacked);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (GameManager.defenceObject != null && !gameManager.restrictionClick && !gameManager.nowDestory && !gameManager.isDealing && GameManager.turnStatus == GameManager.TurnStatus.OnAttack)
            {
                gameManager.restrictionClick = true;
                Debug.Log("発生1");
                StartCoroutine(OnPointerClickCoroutine());
                
            }
            else
            {

                if (SceneManager.GetActiveScene().name == "playGame")
                {
                    uIManager.DetailPanelActive(inf);
                }
                else if (SceneManager.GetActiveScene().name == "makeDeck")
                {
                    uIManager.DetailPanelActive(inf);
                }
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
        Debug.Log("発生2");
    }
}
