using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using GameNamespace;
using Unity.VisualScripting;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public EffectManager effectManager;
    private System.Random rng = new System.Random();
    public AllCardInf allCardInf;
    public Transform hand;
    public GameObject choiceCard;
    public GameObject cardPrefab;
    public int DeckIndex = 0;
    public static List<int> DeckInf = new List<int>();
    public static List<int> enemyDeckInf = new List<int>();
    public List<Card> Hands;
    public List<Card> AttackFields;
    public List<Card> DefenceFields;
    public ObservableCollection<Card> AllFields;
    //守護用のリスト
    public List<Card> CardsWithProtectEffectOnField;
    //アシストかーどをプレイできなくするのリスト
    public List<Card> CannotPlayDefenceCard;
    //フィールド上で効果を発揮するバフカードのリスト
    public List<Card> CardsWithEffectOnField;
    public List<Card> EffectDuringAttacking;
    //効果にターン経過を要するスペルカード用のリスト
    public List<Card> SpelEffectAfterSomeTurn;
    //フィールドにいる限り補助エリアを攻撃できなくさせるカードのリスト
    public List<Card> CannotAttackMyDefenceCard;
    public List<bool> CannotDrawEffectList;
    public PlayerType Owner;

    void Start()
    {
        EffectDuringAttacking = new List<Card>();
        CannotPlayDefenceCard = new List<Card>();
        CardsWithEffectOnField = new List<Card>();
        SpelEffectAfterSomeTurn = new List<Card>();
        CardsWithProtectEffectOnField = new List<Card>();
        CannotAttackMyDefenceCard = new List<Card>();
        CannotDrawEffectList = new List<bool>();
        AllFields = new ObservableCollection<Card>();
        AllFields.CollectionChanged += CollectionChanged;
        EnemyDeckCreate();
        Shuffle(DeckInf);
        Shuffle(enemyDeckInf);
        FirstHandSetUp();
    }

    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private void EnemyDeckCreate()
    {
        int x = allCardInf.allList.Count;  // 1からxまでの数
        int maxCount = 40;  // 合計で40回数をAddする
        int maxDuplicates = 3;  // 各数は最大3回まで重複可能

        Dictionary<int, int> counts = new Dictionary<int, int>();

        // 各数が3回までしか追加されないようにしながら、40回追加
        while (enemyDeckInf.Count < maxCount)
        {
            int num = rng.Next(0, x );  // 1からxまでのランダムな数
            Debug.Log(num);
            if (!counts.ContainsKey(num))
            {
                counts[num] = 0;
            }

            if (counts[num] < maxDuplicates)
            {
                enemyDeckInf.Add(num);
                counts[num]++;
            }
        }
        foreach (int num in enemyDeckInf)
        {
            Console.Write(num + " ");
        }
    }

    private async void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            await effectManager.EffectWhenCollectionChanged(CardsWithEffectOnField, e);
        }
    }

    public void drawCard()
    {
        if (CannotDrawEffectList.Count == 0)
        {
            AudioManager.Instance.PlayDrawSound();
            if (Owner == PlayerType.Player2)
            {
                GameObject card = Instantiate(cardPrefab);
                card.transform.SetParent(hand);
                card.GetComponent<Card>().P2SetUp(allCardInf.allList[enemyDeckInf[DeckIndex]]);
                card.transform.localScale = Vector3.one;
                Hands.Add(card.GetComponent<Card>());
                DeckIndex++;
            }
            else if (Owner == PlayerType.Player1)
            {
                GameObject card = Instantiate(cardPrefab);
                card.transform.SetParent(hand);
                card.GetComponent<Card>().P1SetUp(allCardInf.allList[DeckInf[DeckIndex]]);
                card.transform.localScale = Vector3.one;
                Hands.Add(card.GetComponent<Card>());
                DeckIndex++;
            }
        }
    }

    public void FirstHandSetUp()
    {
        if (Owner == PlayerType.Player2)
        {
            for (DeckIndex = 0; DeckIndex < 5; DeckIndex++)
            {
                GameObject card = Instantiate(cardPrefab, hand, false);
                card.GetComponent<Card>().P2SetUp(allCardInf.allList[enemyDeckInf[DeckIndex]]);
                Hands.Add(card.GetComponent<Card>());
                card.transform.localScale = Vector3.one;
            }
        }
        else if (Owner == PlayerType.Player1)
        {
            for (DeckIndex = 0; DeckIndex < 5; DeckIndex++)
            {
                GameObject card = Instantiate(cardPrefab, hand, false);
                card.GetComponent<Card>().P1SetUp(allCardInf.allList[DeckInf[DeckIndex]]);
                Hands.Add(card.GetComponent<Card>());
                card.transform.localScale = Vector3.one;
            }

        }
    }
}
