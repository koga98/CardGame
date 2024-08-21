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

    public void reflectAmount(int amount)
    {
        amountText.text = "×" + amount.ToString();
    }

    public async Task DestoryThis()
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        gameManager.nowDestory = true;
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
        else if (EnemyAI.EAllFields.Contains(this))
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

        if (CardManager.P1CardsWithEffectOnField.Contains(this))
        {
            CardManager.P1CardsWithEffectOnField.Remove(this);
        }
        else if (CardManager.P1SpelEffectAfterSomeTurn.Contains(this))
        {
            CardManager.P1SpelEffectAfterSomeTurn.Remove(this);
        }
        else if (CardManager.P1CannotAttackMyDefenceCard.Contains(this))
        {
            CardManager.P1CannotAttackMyDefenceCard.Remove(this);
        }
        else if (CardManager.P1CardsWithProtectEffectOnField.Contains(this))
        {
            CardManager.P1CardsWithProtectEffectOnField.Remove(this);
        }
        else if (GameManager.p1CannotDrawEffectList.Contains(this))
        {
            GameManager.p1CannotDrawEffectList.Remove(this);
        }
        else if (CardManager.P1CannotPlayDefenceCard.Contains(this))
        {
            CardManager.P1CannotPlayDefenceCard.Remove(this);
        }

        if (CardManager.P2CardsWithEffectOnField.Contains(this))
        {
            CardManager.P2CardsWithEffectOnField.Remove(this);
        }
        else if (CardManager.P2SpelEffectAfterSomeTurn.Contains(this))
        {
            CardManager.P2SpelEffectAfterSomeTurn.Remove(this);
        }
        else if (CardManager.P2CannotAttackMyDefenceCard.Contains(this))
        {
            CardManager.P2CannotAttackMyDefenceCard.Remove(this);
        }
        else if (CardManager.P2CardsWithProtectEffectOnField.Contains(this))
        {
            CardManager.P2CardsWithProtectEffectOnField.Remove(this);
        }
        else if (GameManager.p2CannotDrawEffectList.Contains(this))
        {
            GameManager.p2CannotDrawEffectList.Remove(this);
        }
        else if (CardManager.P2CannotPlayDefenceCard.Contains(this))
        {
            CardManager.P2CannotPlayDefenceCard.Remove(this);
        }
        if (inf.effectInfs != null)
        {
            for (int i = 0; i < inf.effectInfs.Count; i++)
            {
                foreach (EffectInf.CardTrigger cardTrigger in inf.effectInfs[i].triggers)
                {
                    if (cardTrigger == EffectInf.CardTrigger.AfterDie)
                    {
                        if (inf.effectInfs[i] is ICardEffect cardEffect)
                        {
                            await cardEffect.Apply(new ApplyEffectEventArgs(this, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                            GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
                        }
                    }
                    if (cardTrigger == EffectInf.CardTrigger.FromPlayToDie)
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
        Destroy(this.gameObject);
        gameManager.nowDestory = false;
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
            if (GameManager.defenceObject != null && !restrictionClick && !gameManager.nowDestory && !gameManager.isDealing && GameManager.turnStatus == GameManager.TurnStatus.OnAttack)
            {
                restrictionClick = true;
                StartCoroutine(OnPointerClickCoroutine());
            }
            else
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
        }
    }

    private IEnumerator OnPointerClickCoroutine()
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
}
