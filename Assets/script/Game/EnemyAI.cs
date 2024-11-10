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
    private UtilMethod utilMethod = new UtilMethod();
    public CardManager player1CardManager;
    public CardManager player2CardManager;
    public UIManager uIManager;
    public EffectManager effectManager;
    public GameObject enemyAttackField;
    public GameObject enemyDefenceField;
    public AttackManager attackManager;
    public GameManager gameManager;
    public GameObject leader;
    bool isLeader = false;
    bool continueMethod = true;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    public async Task PlayCard(ManaManager manaManager)
    {
        continueMethod = true;
        while (continueMethod)
        {
            Card playCard = null;
            foreach (Card hand in player2CardManager.Hands)
            {
                if (playCard == null || playCard.cost < hand.cost)
                {
                    Card temp = await PlayCardSelecte(hand, manaManager);
                    if(temp != null)
                    playCard = temp;
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

    private async Task<Card> PlayCardSelecte(Card hand, ManaManager manaManager)
    {
        hand.gameObject.GetComponent<CardDragAndDrop>().canDrag = utilMethod.JudgeActiveCard(hand, manaManager.P2_mana, player2CardManager);
        await effectManager.BeforeCardDrag(hand);
        if (hand.gameObject.GetComponent<CardDragAndDrop>().canDrag)
            return hand;
        else
            return null;
    }

    private async Task PlaySelectedCard(Card playCard, ManaManager manaManager)
    {
        await WaitUntilFalse(() => gameManager.nowCollectionChanging);
        playCard.blindPanel.SetActive(false);
        player2CardManager.Hands.Remove(playCard);
        uIManager.ChangeDeckNumber(1, 40 - player2CardManager.DeckIndex);
        uIManager.ChangeHandNumber(1, player2CardManager.Hands.Count);
        if (playCard.inf.cardType == CardType.Attack)
        {
            await PlayAttackCard(playCard);
        }
        else if (playCard.inf.cardType == CardType.Defence)
        {
            await PlayDefenceCard(playCard);
        }
        else if (playCard.inf.cardType == CardType.Spel)
        {
            await PlaySpelCard(playCard);
        }
        manaManager.P2_mana -= playCard.cost;
        AudioManager.Instance.PlayPlayCardSound();

        if (manaManager.P2_mana == 0)
        {
            continueMethod = false;
        }
    }

    private async Task PlayAttackCard(Card card)
    {
        card.transform.SetParent(enemyAttackField.transform, false);
        foreach (var effect in card.inf.effectInfs)
        {
            if (card.inf is RandomCardInf randomCardInf)
            {
                await effectManager.PlayCardRandomEffect(randomCardInf, card);
                break;
            }
            await ExecuteAllPlayCardEffect(card, effect);
        }
        player2CardManager.AttackFields.Add(card);
        player2CardManager.AllFields.Add(card);
    }

    private async Task PlaySpelCard(Card card)
    {
        foreach (var effect in card.inf.effectInfs)
        {
            if (card.inf is RandomCardInf randomCardInf)
            {
                await effectManager.PlayCardRandomEffect(randomCardInf, card);
                break;
            }
            await ExecuteAllPlayCardEffect(card, effect);
        }

        if (card.transform.parent != GameObject.Find("SpelPanel").transform)
            await card.DestoryThis();

    }

    private async Task PlayDefenceCard(Card card)
    {
        card.transform.SetParent(enemyDefenceField.transform, false);
        foreach (var effect in card.inf.effectInfs)
        {
            if (card.inf is RandomCardInf randomCardInf)
            {
                await effectManager.PlayCardRandomEffect(randomCardInf, card);
                break;
            }
            await ExecuteAllPlayCardEffect(card, effect);
        }
        if (card.gameObject.activeSelf)
        {
            player2CardManager.AllFields.Add(card);
            player2CardManager.DefenceFields.Add(card);
        }
    }

    public async Task Attack()
    {
        await WaitUntilFalse(() => continueMethod);
        gameManager.nowEnemyAttack = true;
        var attackFieldsCopy = player2CardManager.AttackFields?.ToList() ?? new List<Card>();
        var defenceFieldsCopy = player2CardManager.DefenceFields?.ToList() ?? new List<Card>();
        IEnumerable<Card> allFields = attackFieldsCopy.Concat(defenceFieldsCopy);

        // Convert the IEnumerable to a list for index-based access
        var allFieldsList = allFields.ToList();
        await AllCardAttackEnemy(allFieldsList, gameManager);
        gameManager.nowEnemyAttack = false;
    }

    private async Task WaitUntilFalse(Func<bool> condition)
    {
        while (condition())
            await Task.Yield(); // 次のフレームまで待機

    }

    private async Task AllCardAttackEnemy(List<Card> allFieldsList, GameManager gameManager)
    {
        for (int i = 0; i < allFieldsList.Count; i++)
        {
            if (gameManager.isGameOver)
                return;

            Card filed = allFieldsList[i];

            if (filed == null || filed.gameObject == null)
            {
                continue;
            }

            if (!filed.Attacked)
            {
                if (filed.attack == 0)
                    continue;

                GameManager.attackObject = filed.gameObject;
                GameManager.defenceObject = SelectDefenceObject(filed, player1CardManager.AttackFields, player1CardManager.DefenceFields);

                // If the attack clip is not null, proceed with attack animation
                if (filed.inf.attackClip != null)
                {
                    if (isLeader)
                        await attackManager.AttackLeader();

                    else
                        await attackManager.AttackCard();
                }
            }
        }
    }

    private GameObject SelectDefenceObject(Card filed, List<Card> pAttackCard, List<Card> pDefenceCard)
    {
        int count = pAttackCard.Count;
        int defenceCount = pDefenceCard.Count;

        GameObject defenceObject = null;
        isLeader = true; // Default to leader

        if (count == 0 && defenceCount == 0 || leader.GetComponent<Leader>().Hp < filed.attack)
            return leader;

        if (player1CardManager.CardsWithProtectEffectOnField.Count > 0)
        {
            foreach (Card target in player1CardManager.CardsWithProtectEffectOnField)
            {
                if (defenceObject == null || defenceObject.GetComponent<Card>().attack < target.attack)
                    defenceObject = target.gameObject;
            }
            isLeader = false;
        }
        else
        {
            defenceObject = FindBestTarget(pAttackCard, filed) ?? defenceObject;
            defenceObject = FindBestTarget(pDefenceCard, filed) ?? defenceObject;
        }
        if (defenceObject != null)
            isLeader = false;

        return defenceObject ?? leader;
    }

    private GameObject FindBestTarget(List<Card> cards, Card filed)
    {
        GameObject bestTarget = null;
        int score = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            Card childCard = cards[i];
            if (player1CardManager.CannotAttackMyDefenceCard.Count != 0 && childCard.inf.cardType == CardType.Defence || !childCard.canAttackTarget)
                continue;

            int newScore = 0;
            if (childCard.attack > filed.attack + 300 && childCard.hp <= filed.attack)
                newScore = (bestTarget == null) ? 70 : 90;
            else if (childCard.attack < filed.hp && childCard.canAttackTarget)
                newScore = (bestTarget == null) ? 50 : 60;
            else if (childCard.attack > filed.attack && childCard.hp <= filed.attack)
                newScore = (bestTarget == null) ? 30 : 40;

            if (newScore > score)
            {
                score = newScore;
                bestTarget = childCard.gameObject;
            }
        }
        return bestTarget;
    }

    private async Task ExecuteAllPlayCardEffect(Card card, EffectInf effect)
    {
        foreach (var trigger in effect.triggers)
            await ExecuteEachPlayCardEffect(card, effect, trigger);
    }

    private async Task ExecuteEachPlayCardEffect(Card card, EffectInf effect, EffectInf.CardTrigger trigger)
    {
        switch (trigger)
        {
            case EffectInf.CardTrigger.ButtonOperetion:
                AIButtonOperetion();
                break;
            case EffectInf.CardTrigger.OnPlay:
                if (player1CardManager.AllFields.Count != 0)
                {
                    int random = UnityEngine.Random.Range(0, player1CardManager.AllFields.Count);
                    await effectManager.PlayCardChoiceEffect(effect, player1CardManager.AllFields[random]);
                }
                break;

            case EffectInf.CardTrigger.SpelEffectSomeTurn:
                PlayCardEffectSpelNeedsSomeTurn(card);
                break;

            case EffectInf.CardTrigger.AfterPlay:
            case EffectInf.CardTrigger.ProtectShieldDecrease:
                await effectManager.AIPlayCardEffect(card, effect);
                break;

            case EffectInf.CardTrigger.OnFieldOnAfterPlay:
                player2CardManager.CardsWithEffectOnField.Add(card);
                break;

            default:
                break;
        }
    }

    private void AIButtonOperetion()
    {
        int randomValue = UnityEngine.Random.Range(0, 2);
        if (randomValue == 0)
        {
            CardManager.enemyDeckInf.Add(CardManager.enemyDeckInf[player2CardManager.DeckIndex]);
            CardManager.enemyDeckInf.RemoveAt(player2CardManager.DeckIndex);
        }
    }

    private void PlayCardEffectSpelNeedsSomeTurn(Card card)
    {
        player2CardManager.SpelEffectAfterSomeTurn.Add(card);
        card.transform.SetParent(GameObject.Find("SpelPanel").transform, false);
    }
}
