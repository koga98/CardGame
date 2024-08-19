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
    public GameObject enemyAttackField;
    public GameObject enemyDefenceField;
    public GameObject enemyHand;
    public static List<Card> hands; // AIの手札
    public static List<Card> AttackFields;
    public static List<Card> DefenceFields;
    public static ObservableCollection<Card> EAllFields; // AIのフィールド
    public GameObject leader;
    bool isLeader = false;
    bool continueMethod = true;
    // Start is called before the first frame update
    void Start()
    {
        hands = new List<Card>();
        AttackFields = new List<Card>();
        DefenceFields = new List<Card>();
        EAllFields = new ObservableCollection<Card>();
        EAllFields.CollectionChanged += CollectionChangedAsync;
    }

    // Update is called once per frame
    void Update()
    {
    }

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
        if (GameManager.p2CannotDrawEffectList.Count == 0)
        {
            AudioManager.Instance.PlayDrawSound();
            Transform hand = canvas.transform.Find("enemyHand ");
            GameObject card = Instantiate(enemyCardPrefab);
            card.transform.SetParent(hand);
            card.GetComponent<Card>().P2SetUp(enemyDeckInf);
            card.transform.localScale = Vector3.one;
            hands.Add(card.GetComponent<Card>());
            gameManager.k++;
        }

    }

    public async Task PlayCard(GameManager gameManager)
    {
        continueMethod = true;
        while (continueMethod)
        {
            Card playCard = null;
            foreach (Card hand in hands)
            {
                if (playCard == null)
                {
                    Debug.Log(gameManager.P2_mana);
                    Debug.Log(hand.cost);
                    if (gameManager.P2_mana >= hand.cost)
                    {
                        if (CardManager.P2CannotPlayDefenceCard.Count == 0)
                        {
                            playCard = hand;
                        }
                        else if (CardManager.P2CannotPlayDefenceCard.Count != 0 && hand.inf.cardType != CardType.Defence)
                        {
                            playCard = hand;
                        }
                        foreach (var effect in hand.inf.effectInfs)
                        {
                            for (int i = 0; i < effect.triggers.Count; i++)
                            {
                                if (effect.triggers[i] == EffectInf.CardTrigger.OnBeginDrag)
                                {
                                    if (effect is ICardEffect cardEffect)
                                    {
                                        await cardEffect.Apply(new ApplyEffectEventArgs(hand, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields
                                        , GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
                                    }
                                }
                            }
                            if (hand.gameObject.GetComponent<CardDragAndDrop>().canDrag)
                            {
                                playCard = hand;
                            }
                            else
                            {
                                playCard = null;
                            }
                        }

                    }
                }
                else
                {
                    if (gameManager.P2_mana >= hand.cost && playCard.cost < hand.cost)
                    {
                        if (CardManager.P2CannotPlayDefenceCard.Count == 0)
                        {
                            playCard = hand;
                        }
                        else if (CardManager.P2CannotPlayDefenceCard.Count != 0 && hand.inf.cardType != CardType.Defence)
                        {
                            playCard = hand;
                        }
                        foreach (var effect in hand.inf.effectInfs)
                        {
                            for (int i = 0; i < effect.triggers.Count; i++)
                            {
                                if (effect.triggers[i] == EffectInf.CardTrigger.OnBeginDrag)
                                {
                                    if (effect is ICardEffect cardEffect)
                                    {
                                        await cardEffect.Apply(new ApplyEffectEventArgs(hand, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields
                                        , GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
                                    }
                                }
                            }
                        }
                        if (hand.gameObject.GetComponent<CardDragAndDrop>().canDrag)
                        {
                            playCard = hand;
                        }
                    }
                }

            }
            if (playCard != null)
            {
                GameObject card = playCard.gameObject;
                if (card != null)
                {
                    hands.Remove(playCard);
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
                    gameManager.P2_mana -= playCard.cost;
                    AudioManager.Instance.PlayPlayCardSound();
                    if (gameManager.P2_mana == 0)
                    {
                        continueMethod = false;
                    }
                }
            }
            else
            {
                continueMethod = false;
            }
        }
        Debug.Log("PlayCardメソッド内終了");
    }

    public async Task AttackMethod(Card card)
    {
        card.transform.SetParent(enemyAttackField.transform, false);
        AttackFields.Add(card);
        EAllFields.Add(card);
        if (card.inf.effectInfs.Count != 0)
        {
            await ProcessCardEffects(card);
        }
        Debug.Log("AttackMethod終了");
    }

    public async Task SpelMethod(Card card)
    {
        await ProcessCardEffects(card);
    }

    public async Task DefenceMethod(Card card)
    {
        card.transform.SetParent(enemyDefenceField.transform, false);
        EAllFields.Add(card);
        DefenceFields.Add(card);
        await ProcessCardEffects(card);

    }

    public async Task Attack()
    {
        await WaitUntilFalse(() => continueMethod);
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();

        // Combine both attack and defense fields
        //IEnumerable<Card> allFields = (AttackFields ?? Enumerable.Empty<Card>()).Concat(DefenceFields ?? Enumerable.Empty<Card>());
        var attackFieldsCopy = AttackFields?.ToList() ?? new List<Card>();
        var defenceFieldsCopy = DefenceFields?.ToList() ?? new List<Card>();
        IEnumerable<Card> allFields = attackFieldsCopy.Concat(defenceFieldsCopy);

        // Convert the IEnumerable to a list for index-based access
        var allFieldsList = allFields.ToList();
        Debug.Log(allFieldsList.Count);

        for (int i = 0; i < allFieldsList.Count; i++)
        {
            Card filed = allFieldsList[i];

            if (!filed.Attacked)
            {
                GameManager.attackObject = filed.gameObject;
                GameManager.defenceObject = SelectDefenceObject(filed, GameManager.PAttackFields, GameManager.PDefenceFields);
                Debug.Log(GameManager.defenceObject);

                // If the attack clip is not null, proceed with attack animation
                if (filed.inf.attackClip != null)
                {
                    if (isLeader)
                    {
                        await GameManager.defenceObject.GetComponent<Leader>().AttackedLeaderAsync();
                    }
                    else
                    {
                        await gameManager.AttackCard();
                        Debug.Log(i + "終了");
                    }
                }
            }
        }
    }

    private async Task WaitUntilFalse(Func<bool> condition)
    {
        while (condition())
        {
            await Task.Yield(); // Unityエンジンのフレームワーク上で実行するために次のフレームを待機
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

        if (CardManager.P1CardsWithProtectEffectOnField.Count > 0)
        {
            foreach (Card target in CardManager.P1CardsWithProtectEffectOnField)
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

    // Helper method to find the best target in a given field
    //オブジェクトが破壊されていた時にメソッド処理中に呼び出され、それを代入してしまっている
    private GameObject FindBestTarget(List<Card> cards, Card filed)
    {
        GameObject bestTarget = null;
        for (int i = 0; i < cards.Count; i++)
        {
            Card childCard = cards[i];
            if (CardManager.P1CannotAttackMyDefenceCard.Count != 0 && childCard.inf.cardType == CardType.Defence)
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

    public async Task ProcessCardEffects(Card card)
    {
        // ランダム効果カードの特別な処理
        if (card.inf is RandomCardInf randomCardInf)
        {
            int randomValue = UnityEngine.Random.Range(0, randomCardInf.effectInfs.Count);
            await randomCardInf.effectInfs[randomValue].Apply(new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
            return; // これ以上の効果は処理しない
        }

        if (card.inf.effectInfs[0].triggers[0] == EffectInf.CardTrigger.OnPlay)
        {
            if (GameManager.PAllFields.Count != 0)
            {
                int random = UnityEngine.Random.Range(0, GameManager.PAllFields.Count);
                if (card.inf.effectInfs[0] is ICardEffect onPlay)
                {
                    await onPlay.Apply(new ApplyEffectEventArgs(GameManager.PAllFields[random], EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                        GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields, GameManager.PAllFields[random]));
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
                                GameManager.enemyDeckInf.Add(GameManager.enemyDeckInf[gameManager.k]);
                                GameManager.enemyDeckInf.RemoveAt(gameManager.k);
                            }

                        }
                        break;

                    case EffectInf.CardTrigger.SpelEffectSomeTurn:
                        CardManager.P2SpelEffectAfterSomeTurn.Add(card);
                        card.transform.SetParent(GameObject.Find("SpelPanel").transform, false);
                        break;

                    case EffectInf.CardTrigger.AfterPlay:
                        if (effect is ICardEffect afterPlayEffect)
                        {
                            await afterPlayEffect.Apply(new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                                GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
                        }
                        break;

                    case EffectInf.CardTrigger.FromPlayToDie:
                        if (effect is ICardEffect fromPlayToDieEffect)
                        {
                            await fromPlayToDieEffect.Apply(new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                                GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
                        }
                        break;

                    case EffectInf.CardTrigger.OnFieldOnAfterPlay:
                        if (effect is ICardEffect onFieldEffect)
                        {
                            CardManager.P2CardsWithEffectOnField.Add(card);
                        }
                        break;

                    case EffectInf.CardTrigger.AfterPlayAndProtectShieldDecrease:
                        if (effect is ICardEffect afterPlayAndProtectShieldDecrease)
                        {
                            await afterPlayAndProtectShieldDecrease.Apply(new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                                GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
                            CardManager.P2CardsWithEffectOnField.Add(card);
                        }
                        break;

                    default:
                        break;
                }
            }
        }
        Debug.Log("ProcessCardEffects終了");
    }
    private static async void CollectionChangedAsync(object sender, NotifyCollectionChangedEventArgs e)
    {

        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            if (CardManager.P2CardsWithEffectOnField.Count != 0)
            {
                Card card = (Card)e.NewItems[0];
                foreach (var effectCard in CardManager.P2CardsWithEffectOnField)
                {
                    foreach (var effect in effectCard.inf.effectInfs)
                    {
                        for (int i = 0; i < effect.triggers.Count; i++)
                        {
                            if (effect.triggers[i] == EffectInf.CardTrigger.OnFieldOnAfterPlay)
                            {
                                if (effect is ICardEffect cardEffect)
                                {
                                    await cardEffect.Apply(new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                                     GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));

                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
