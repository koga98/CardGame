using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameNamespace;
using UnityEngine;
using UnityEngine.UI;

public class ManaManager : MonoBehaviour
{
    CardManager player1CardManager;
    CardManager player2CardManager;
    public EffectManager effectManager;
    private int p1_mana;
    private int p2_mana;
    private int p1MaxMana;
    private int p2MaxMana;
    public Text P1_manaText;
    public Text P2_manaText;


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

    // Start is called before the first frame update
    void Start()
    {
        P1_mana = 0;
        P2_mana = 0;
        P1MaxMana = P1_mana;
        P2MaxMana = P2_mana;
        player1CardManager = GameObject.Find("P1CardManager").GetComponent<CardManager>();
        player2CardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (P1_mana > P1MaxMana)
        {
            P1_mana = P1MaxMana;
            P1_manaText.text = manaChange();
        }
    }

    public void P1TurnStart()
    {
        P1MaxMana += 100;
        P1_mana = P1MaxMana;
        P1_manaText.text = manaChange();
        
        if (player1CardManager.AllFields != null)
        {
            foreach (Card card in player1CardManager.AllFields)
            {
                for (int c = 0; c < card.attackedList.Count; c++)
                {
                    card.attackedList[c] = false;
                }
            }
        }
    }

    public void P2TurnStart()
    {
        P2MaxMana += 100;
        P2_mana = P2MaxMana;
        
        if (player2CardManager.AllFields != null)
        {
            foreach (Card card in player2CardManager.AllFields)
            {
                for (int c = 0; c < card.attackedList.Count; c++)
                {
                    card.attackedList[c] = false;
                }
            }
        }
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

    public IEnumerator OnP1ManaChanged()
    {
        if (player1CardManager.Hands != null)
        {
            yield return StartCoroutine(ConfirmCanPlayCard());
        }
    }

    private IEnumerator ConfirmCanPlayCard()
    {
        for (int i = player1CardManager.Hands.Count - 1; i >= 0; i--)
        {
            var card = player1CardManager.Hands[i];
            GameObject cardObject = card.gameObject;
            CardDragAndDrop dragCard = cardObject.GetComponent<CardDragAndDrop>();
            dragCard.canDrag = IsPlayableCard(card);
            yield return StartCoroutine(effectManager.OnBeginDragEffect(card).AsCoroutine());
            // カードのActivePanelの状態をcanDragプロパティに基づいて設定
            card.ActivePanel.SetActive(dragCard.canDrag);
        }
    }

    private bool IsPlayableCard(Card card)
    {
        bool isPlayable;
        if (card.cost > P1_mana)
        {
            isPlayable = false;
        }
        else
        {
            isPlayable = true;
        }
        // P1CannotPlayDefenceCardが存在し、カードが防御タイプの場合、ドラッグ不可に設定
        if (player1CardManager.CannotPlayDefenceCard.Count != 0 && card.inf.cardType == CardType.Defence)
        {
            isPlayable = false;
        }
        return isPlayable;
    }
    public void P1MaxManaIncrease(int increaseAmount){
        P1MaxMana += increaseAmount;
    }
    public void P2MaxManaIncrease(int increaseAmount){
        P2MaxMana += increaseAmount;
    }

    public void P1ManaHeal(int healAmount){
        P1_mana += healAmount;
        P1_manaText.text = manaChange();
    }

    public void P2ManaHeal(int healAmount){
        P2_mana += healAmount;
        P2_manaText.text = P2manaChange();
    }

    public void P1ManaDecrease(int decreaseAmount){
        if (decreaseAmount == 1)
        P1_mana /= 2;
        else
        P1_mana -= decreaseAmount;
    }

    public void P2ManaDecrease(int decreaseAmount){
        if (decreaseAmount == 1)
        P2_mana /= 2;
        else
        P2_mana -= decreaseAmount;
    }
}
