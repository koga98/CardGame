using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameNamespace;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System;

public class EnemyAI : MonoBehaviour
{
    public CardManager player1CardManager;
    public CardManager player2CardManager;
    public GameObject enemyAttackField;
    public GameObject enemyDefenceField;
    public AttackManager attackManager;
    public GameObject leader;
    bool isLeader = false;
    bool continueMethod = true;
    // Start is called before the first frame update
    void Start()
    {
        CardManager p1Cardmanager = GameObject.Find("P1CardManager").GetComponent<CardManager>(); 
        CardManager p2Cardmanager = GameObject.Find("P2CardManager").GetComponent<CardManager>(); 
    }

    // Update is called once per frame
    void Update()
    {
    }

<<<<<<< HEAD
=======
    public void Instantiate()
    {
        int ObjCount = enemyAttackField.transform.childCount;
        for (int i = 0; i < ObjCount; i++)
        {
            Transform childTransform = enemyAttackField.transform.GetChild(i);
            GameObject childObject = childTransform.gameObject;
            Card childCard = childObject.GetComponent<Card>();
            AttackFields.Add(childCard);
            EAllFields.Add(childCard);
        }
        ObjCount = enemyDefenceField.transform.childCount;
        for (int i = 0; i < ObjCount; i++)
        {
            Transform childTransform = enemyDefenceField.transform.GetChild(i);
            GameObject childObject = childTransform.gameObject;
            Card childCard = childObject.GetComponent<Card>();
            DefenceFields.Add(childCard);
            EAllFields.Add(childCard);
        }

        for (int i = 0; i < enemyHand.transform.childCount; i++)
        {
            Transform childTransform = enemyHand.transform.GetChild(i);
            GameObject childObject = childTransform.gameObject;
            Card childCard = childObject.GetComponent<Card>();
            hands.Add(childCard);
        }
    }

    public void drawCard(int k, CardInf enemyDeckInf, GameObject canvas, GameObject enemyCardPrefab, GameManager gameManager)
    {
        Debug.Log(CardManager.p2CannotDrawEffectList.Count);
        if (CardManager.p2CannotDrawEffectList.Count == 0)
        {
            AudioManager.Instance.PlayDrawSound();
            Transform hand = canvas.transform.Find("enemyHand ");
            GameObject card = Instantiate(enemyCardPrefab);
            card.transform.SetParent(hand);
            card.GetComponent<Card>().P2SetUp(enemyDeckInf);
            card.transform.localScale = Vector3.one;
            hands.Add(card.GetComponent<Card>());
            gameManager.enemyDeckIndex++;
        }

    }

>>>>>>> 6b680c9b2337113d24102b3cbe88d67012e86f91
    public async Task PlayCard(ManaManager manaManager)
    {
        continueMethod = true;
        while (continueMethod)
        {
            Card playCard = null;
            foreach (Card hand in player2CardManager.Hands)
            {
                if (playCard == null)
                {
                    if (manaManager.P2_mana >= hand.cost)
                    {
                        await OnBeginDragEffect(hand);
                        playCard = PlayCardSelecte(hand);
                    }
                }
                else
                {
                    if (manaManager.P2_mana >= hand.cost && playCard.cost < hand.cost)
                    {
                        await OnBeginDragEffect(hand);
                        playCard = SetBetterCard(playCard, hand);
                    }
                }
            }
            if (playCard != null)
            {
                await PlaySelectedCard(playCard, manaManager);
            }
            else
            {
                continueMethod = false;
            }
        }
    }
    public async Task Attack()
    {
        await WaitUntilFalse(() => continueMethod);
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        gameManager.nowEnemyAttack = true;
        var attackFieldsCopy = player2CardManager.AttackFields?.ToList() ?? new List<Card>();
        var defenceFieldsCopy = player2CardManager.DefenceFields?.ToList() ?? new List<Card>();
        IEnumerable<Card> allFields = attackFieldsCopy.Concat(defenceFieldsCopy);

        // Convert the IEnumerable to a list for index-based access
        var allFieldsList = allFields.ToList();
        await AllCardAttackEnemy(allFieldsList,gameManager);
        gameManager.nowEnemyAttack = false;
    }

    private async Task OnBeginDragEffect(Card hand)
    {
        foreach (var effect in hand.inf.effectInfs)
        {
            for (int i = 0; i < effect.triggers.Count; i++)
            {
                if (effect.triggers[i] == EffectInf.CardTrigger.OnBeginDrag)
                {
                    if (effect is ICardEffect cardEffect)
                    {
                        await cardEffect.Apply(new ApplyEffectEventArgs(hand, player2CardManager.AllFields, player2CardManager.AttackFields, player2CardManager.DefenceFields
                        , player1CardManager.AllFields, player1CardManager.AttackFields, player1CardManager.DefenceFields));
                    }
                }
            }
        }
    }

    private Card PlayCardSelecte(Card hand)
    {
        bool ClearCardEffectConditon = hand.gameObject.GetComponent<CardDragAndDrop>().canDrag;
        bool NoRestrictPlayDefenceCard = player2CardManager.CannotPlayDefenceCard.Count == 0;
        bool IsNotDefenceCard = player2CardManager.CannotPlayDefenceCard.Count != 0 && hand.inf.cardType != CardType.Defence;
        bool canPlayDefenceCard;
        if (NoRestrictPlayDefenceCard || IsNotDefenceCard)
            canPlayDefenceCard = true;
        else
            canPlayDefenceCard = false;

        if (canPlayDefenceCard && ClearCardEffectConditon)
            return hand;
        else
            return null;
    }

    private Card SetBetterCard(Card playCard, Card hand)
    {
        bool ClearCardEffectConditon = hand.gameObject.GetComponent<CardDragAndDrop>().canDrag;
        bool NoRestrictPlayDefenceCard = player2CardManager.CannotPlayDefenceCard.Count == 0;
        bool IsNotDefenceCard = player2CardManager.CannotPlayDefenceCard.Count != 0 && hand.inf.cardType != CardType.Defence;
        bool canPlayDefenceCard;
        if (NoRestrictPlayDefenceCard || IsNotDefenceCard)
            canPlayDefenceCard = true;
        else
            canPlayDefenceCard = false;

        if (canPlayDefenceCard && ClearCardEffectConditon)
            return hand;
        else
            return playCard;
    }

    private async Task PlaySelectedCard(Card playCard, ManaManager manaManager)
    {
        playCard.blindPanel.SetActive(false);
        player2CardManager.Hands.Remove(playCard);
        if (playCard.inf.cardType == CardType.Attack)
        {
            await AttackMethod(playCard);
        }
        else if (playCard.inf.cardType == CardType.Defence)
        {
            await DefenceMethod(playCard);
        }
        else if (playCard.inf.cardType == CardType.Spel)
        {
            await SpelMethod(playCard);
        }
        manaManager.P2_mana -= playCard.cost;
        AudioManager.Instance.PlayPlayCardSound();
        if (manaManager.P2_mana == 0)
        {
            continueMethod = false;
        }
    }

    private async Task AttackMethod(Card card)
    {
        card.transform.SetParent(enemyAttackField.transform, false);
        player2CardManager.AttackFields.Add(card);
        player2CardManager.AllFields.Add(card);
        if (card.inf.effectInfs.Count != 0)
        {
            await ProcessCardEffects(card);
        }
    }

    private async Task SpelMethod(Card card)
    {
        await ProcessCardEffects(card);
        if (card.transform.parent == GameObject.Find("SpelPanel").transform)
        {
        }
        else
        {
            Destroy(card.gameObject);
        }
    }

    private async Task DefenceMethod(Card card)
    {
        card.transform.SetParent(enemyDefenceField.transform, false);
        player2CardManager.AllFields.Add(card);
        player2CardManager.DefenceFields.Add(card);
        await ProcessCardEffects(card);

    }

    private async Task AllCardAttackEnemy(List<Card> allFieldsList,GameManager gameManager)
    {
        for (int i = 0; i < allFieldsList.Count; i++)
        {
            Card filed = allFieldsList[i];

            if (!filed.Attacked)
            {
                GameManager.attackObject = filed.gameObject;
                GameManager.defenceObject = SelectDefenceObject(filed, player1CardManager.AttackFields, player1CardManager.DefenceFields);

                // If the attack clip is not null, proceed with attack animation
                if (filed.inf.attackClip != null)
                {
                    if (isLeader)
                    {
                        await attackManager.AttackLeader();
                    }
                    else
                    {
                        await attackManager.AttackCard();
                    }
                }
            }
        }
    }

    private async Task WaitUntilFalse(Func<bool> condition)
{
    // conditionがfalseになるまでループ
    while (condition())
    {
        await Task.Yield(); // 次のフレームまで待機
    }
}

    // Helper method to select the appropriate defence object
    private GameObject SelectDefenceObject(Card filed, List<Card> pAttackCard, List<Card> pDefenceCard)
    {
        int count = pAttackCard.Count;
        int defenceCount = pDefenceCard.Count;

        GameObject defenceObject = null;
        isLeader = true; // Default to leader

        if (count == 0 && defenceCount == 0)
        {
            return leader;
        }

        if (player1CardManager.CardsWithProtectEffectOnField.Count > 0)
        {
            foreach (Card target in player1CardManager.CardsWithProtectEffectOnField)
            {
                if (defenceObject == null || defenceObject.GetComponent<Card>().attack < target.attack)
                {
                    defenceObject = target.gameObject;
                }
            }
            isLeader = false;
        }
        else
        {
            defenceObject = FindBestTarget(pAttackCard, filed) ?? defenceObject;
            defenceObject = FindBestTarget(pDefenceCard, filed) ?? defenceObject;
        }
        if (defenceObject != null)
        {
            isLeader = false;
        }

        return defenceObject ?? leader;
    }

    private GameObject FindBestTarget(List<Card> cards, Card filed)
    {
        GameObject bestTarget = null;
        for (int i = 0; i < cards.Count; i++)
        {
            Card childCard = cards[i];
            if (player1CardManager.CannotAttackMyDefenceCard.Count != 0 && childCard.inf.cardType == CardType.Defence)
            {
                continue;
            }
            if (childCard.attack < filed.hp && childCard.canAttackTarget)
            {
                if (bestTarget == null || bestTarget.GetComponent<Card>().attack < childCard.attack)
                {
                    bestTarget = childCard.gameObject;
                }
            }
        }

        return bestTarget;
    }

    private async Task ProcessCardEffects(Card card)
    {
        // ランダム効果カードの特別な処理
        if (card.inf is RandomCardInf randomCardInf)
        {
            int randomValue = UnityEngine.Random.Range(0, randomCardInf.effectInfs.Count);
            await randomCardInf.effectInfs[randomValue].Apply(new ApplyEffectEventArgs(card, player2CardManager.AllFields, player2CardManager.AttackFields, player2CardManager.DefenceFields,
                player1CardManager.AllFields, player1CardManager.AttackFields, player1CardManager.DefenceFields));
            return; // これ以上の効果は処理しない
        }

        if (card.inf.effectInfs[0].triggers[0] == EffectInf.CardTrigger.OnPlay)
        {
            if (player1CardManager.AllFields.Count != 0)
            {
                int random = UnityEngine.Random.Range(0, player1CardManager.AllFields.Count);
                if (card.inf.effectInfs[0] is ICardEffect onPlay)
                {
                    await onPlay.Apply(new ApplyEffectEventArgs(player1CardManager.AllFields[random], player2CardManager.AllFields, player2CardManager.AttackFields, player2CardManager.DefenceFields,
                        player1CardManager.AllFields, player1CardManager.AttackFields, player1CardManager.DefenceFields, player1CardManager.AllFields[random]));
                }
            }
        }

        foreach (var effect in card.inf.effectInfs)
        {
            foreach (var trigger in effect.triggers)
            {
                switch (trigger)
                {
                    case EffectInf.CardTrigger.ButtonOperetion:
                        //これも選択するものを決める
                        if (effect is CheckTopCard buttonEffect)
                        {
                            GameObject manager = GameObject.Find("GameManager");
                            GameManager gameManager = manager.GetComponent<GameManager>();
                            int randomValue = UnityEngine.Random.Range(0, 2);
                            if (randomValue == 0)
                            {
                                CardManager.enemyDeckInf.Add(CardManager.enemyDeckInf[player2CardManager.DeckIndex]);
                                CardManager.enemyDeckInf.RemoveAt(player2CardManager.DeckIndex);
                            }

                        }
                        break;

                    case EffectInf.CardTrigger.SpelEffectSomeTurn:
                        player2CardManager.SpelEffectAfterSomeTurn.Add(card);
                        card.transform.SetParent(GameObject.Find("SpelPanel").transform, false);
                        break;

                    case EffectInf.CardTrigger.AfterPlay:
                        if (effect is ICardEffect afterPlayEffect)
                        {
                            await afterPlayEffect.Apply(new ApplyEffectEventArgs(card, player2CardManager.AllFields, player2CardManager.AttackFields, player2CardManager.DefenceFields,
                                player1CardManager.AllFields, player1CardManager.AttackFields, player1CardManager.DefenceFields));
                        }
                        break;

                    case EffectInf.CardTrigger.FromPlayToDie:
                        if (effect is ICardEffect fromPlayToDieEffect)
                        {
                            await fromPlayToDieEffect.Apply(new ApplyEffectEventArgs(card, player2CardManager.AllFields, player2CardManager.AttackFields, player2CardManager.DefenceFields,
                                player1CardManager.AllFields, player1CardManager.AttackFields, player1CardManager.DefenceFields));
                        }
                        break;

                    case EffectInf.CardTrigger.OnFieldOnAfterPlay:
                        if (effect is ICardEffect onFieldEffect)
                        {
                            player2CardManager.CardsWithEffectOnField.Add(card);
                        }
                        break;

                    case EffectInf.CardTrigger.AfterPlayAndProtectShieldDecrease:
                        if (effect is ICardEffect afterPlayAndProtectShieldDecrease)
                        {
                            await afterPlayAndProtectShieldDecrease.Apply(new ApplyEffectEventArgs(card, player2CardManager.AllFields, player2CardManager.AttackFields, player2CardManager.DefenceFields,
                                player1CardManager.AllFields, player1CardManager.AttackFields, player1CardManager.DefenceFields));
                            player2CardManager.CardsWithEffectOnField.Add(card);
                        }
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
