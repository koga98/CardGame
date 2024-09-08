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
using System.Linq;

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
    public bool restrictionClick;
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
        }
        else if (SceneManager.GetActiveScene().name == "makeDeck")
        {
            manager = GameObject.Find("GameObject");
            deckMake = manager.GetComponent<DeckMake>();
        }

        canAvoidAttack = false;
        canAttackTarget = true;
        restrictionClick = false;
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
        blindPanel.SetActive(true);
        if (inf.cardType == CardType.Spel)
        {
            backColor.color = Color.blue;
            attackPanel.SetActive(false);
            defencePanel.SetActive(false);
        }
    }

    public async Task DestoryThis()
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        gameManager.nowDestory = true;
        P1CardDestory();
        P2CardDestory();
        await EffectAfterDie();
        Destroy(this.gameObject);
        gameManager.nowDestory = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (GameManager.defenceObject != null && !restrictionClick && !gameManager.nowDestory && !gameManager.isDealing && GameManager.turnStatus == GameManager.TurnStatus.OnAttack)
            {
                restrictionClick = true;
                StartCoroutine(AttackCoroutine());
            }
            else
            {
                ShowCardDetail();
            }
        }
    }

    public void reflectAmount(int amount)
    {
        amountText.text = "×" + amount.ToString();
    }

    private async Task EffectAfterDie(){
        if (inf.effectInfs != null)
        {
            for (int i = 0; i < inf.effectInfs.Count; i++)
            {
                foreach (EffectInf.CardTrigger cardTrigger in inf.effectInfs[i].triggers)
                {
                    if (cardTrigger == EffectInf.CardTrigger.AfterDie || cardTrigger == EffectInf.CardTrigger.FromPlayToDie)
                    {
                        if (inf.effectInfs[i] is ICardEffect cardEffect)
                        {
                            await cardEffect.Apply(new ApplyEffectEventArgs(this, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                            GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
                        }
                    }
                }
            }
        }
    }

    private void OnAttackedListChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (attackedList.Count > 0)
        {
            Attacked = attackedList[attackedList.Count - 1];
        }
    }

    private void P1CardDestory()
    {
        if (GameManager.PAllFields.Contains(this))
        {
            GameManager.PAllFields.Remove(this);
            if (this.inf.cardType == CardType.Defence)
            {
                GameManager.PDefenceFields.Remove(this);
            }
            else if (this.inf.cardType == CardType.Attack)
            {
                GameManager.PAttackFields.Remove(this);
            }
        }
        var cardLists = new List<List<Card>>
    {
    CardManager.P1CardsWithEffectOnField,
    CardManager.P1SpelEffectAfterSomeTurn,
    CardManager.P1CannotAttackMyDefenceCard,
    CardManager.P1CardsWithProtectEffectOnField,
    CardManager.P1CannotPlayDefenceCard,
    CardManager.P1EffectDuringAttacking
    };
        foreach (var list in cardLists)
        {
            if (list.Contains(this))
            {
                list.Remove(this);
            }
        }
        if (CardManager.p1CannotDrawEffectList.Contains(this))
        {
            CardManager.p1CannotDrawEffectList.Remove(this);
        }
    }

    private void P2CardDestory()
    {
        if (EnemyAI.EAllFields.Contains(this))
        {
            EnemyAI.EAllFields.Remove(this);
            if (this.inf.cardType == CardType.Defence)
            {
                EnemyAI.DefenceFields.Remove(this);
            }
            else if (this.inf.cardType == CardType.Attack)
            {
                EnemyAI.AttackFields.Remove(this);
            }
        }
        var cardLists = new List<List<Card>>
    {
    CardManager.P2CardsWithEffectOnField,
    CardManager.P2SpelEffectAfterSomeTurn,
    CardManager.P2CannotAttackMyDefenceCard,
    CardManager.P2CardsWithProtectEffectOnField,
    CardManager.P2CannotPlayDefenceCard,
    CardManager.P2EffectDuringAttacking
    };
        foreach (var list in cardLists)
        {
            if (list.Contains(this))
            {
                list.Remove(this);
            }
        }

        if (CardManager.p2CannotDrawEffectList.Contains(this))
        {
            CardManager.p2CannotDrawEffectList.Remove(this);
        }

    }

    private IEnumerator AttackCoroutine()
    {
        // GameManagerを取得
        if (GameManager.defenceObject != null)
        {
            GameObject manager = GameObject.Find("GameManager");
            GameManager gameManager = manager.GetComponent<GameManager>();

            // 非同期メソッドを実行し、その完了を待つ
            yield return gameManager.AttackCard().AsCoroutine();
            restrictionClick = false;
        }
    }

    private void ShowCardDetail()
    {
        if (SceneManager.GetActiveScene().name == "playGame")
        {
            gameManager.detailPanel.SetActive(true);
            gameManager.cardDetailText[0].text = inf.cardName;
            gameManager.cardDetailText[1].text = inf.cost.ToString();
            gameManager.cardDetailText[2].text = inf.attack.ToString();
            gameManager.cardDetailText[3].text = inf.hp.ToString();
            gameManager.cardDetailText[4].text = inf.longText;
            gameManager.cardDetailText[5].text = inf.cardType.ToString();
        }
        else if (SceneManager.GetActiveScene().name == "makeDeck")
        {
            deckMake.detailPanel.SetActive(true);
            deckMake.DetailText[0].text = inf.cardName;
            deckMake.DetailText[1].text = inf.cost.ToString();
            deckMake.DetailText[2].text = inf.attack.ToString();
            deckMake.DetailText[3].text = inf.hp.ToString();
            deckMake.DetailText[4].text = inf.longText;
            deckMake.DetailText[5].text = inf.cardType.ToString();
        }
    }

    private void OnChangeAttacked()
    {
        if (GameManager.turnStatus == GameManager.TurnStatus.OnAttack && CardOwner == PlayerID.Player1)
        {
            ActivePanel.SetActive(!attacked);
        }
    }
}
