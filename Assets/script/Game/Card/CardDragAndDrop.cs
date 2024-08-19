using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using GameNamespace;
using System.Threading.Tasks;

public class CardDragAndDrop : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    Vector3 touchPosition;
    public Transform defaultParent;
    public Transform choicePanel;
    public GameObject canvas;
    GameManager gameManager;
    public GameObject myHand;
    public GameObject myAttackField;
    public GameObject myDefenceField;
    public Transform deckList;
    public Transform spelPanel;
    public Transform cardOption;
    Card card;
    GameObject clickedObject;
    public bool completeChoice = false;
    public static bool OnCoroutine = false;
    public static bool OnButtonCoroutine = false;
    public bool canDrag = true;

    float defaultZPosition = 0;
    string currentSceneName;

    int siblingIndex = 0;
    Animator cardAnimator;

    void Start()
    {
        canDrag = true;
        if (SceneManager.GetActiveScene().name == "playGame")
        {
            GameObject manager = GameObject.Find("GameManager");
            gameManager = manager.GetComponent<GameManager>();
            choicePanel = GameObject.Find("ChoicePosition").transform;
            spelPanel = GameObject.Find("SpelPanel").transform;
        }


        completeChoice = false;
        OnCoroutine = false;
        currentSceneName = SceneManager.GetActiveScene().name;
        card = gameObject.GetComponent<Card>();
    }
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "makeDeck" && gameObject.transform.position.y > 310)
        {
            if (Input.GetMouseButtonDown(0))
            {
                touchPosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 direction = Input.mousePosition - touchPosition;
                touchPosition = Input.mousePosition;
                if (direction != Vector3.zero)
                {
                    MoveCards(direction);
                }
            }

        }
    }

    private void MoveCards(Vector3 direction)
    {
        gameObject.transform.localPosition += new Vector3(direction.x * 0.9f, 0, 0);

    }

    public void OnDrag(PointerEventData eventData)
    {
        //条件分岐でカードがmyHandかmyFieldかを判定する
        if (SceneManager.GetActiveScene().name == "playGame" && GameManager.turnStatus == GameManager.TurnStatus.OnPlay && canDrag)
        {
            Vector3 newPos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
            (RectTransform)transform.parent, // 親のRectTransform
            eventData.position, // ドラッグ中のスクリーン座標
            eventData.pressEventCamera, // イベントを処理するカメラ
            out newPos); // ワールド座標を取得

            // Z軸の値を維持または設定する
            newPos.z = transform.position.z;

            // 新しい位置を設定
            transform.position = newPos;
        }
        else if (!canDrag)
        {


        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(OnPointerClickCoroutine(eventData));
    }

    public IEnumerator OnPointerClickCoroutine(PointerEventData eventData)
    {
        if (OnCoroutine)
        {
            clickedObject = GetCardObject(eventData.pointerCurrentRaycast.gameObject);
            if (clickedObject.tag == "Enemy")
            {
                Card clickedObjectCard = clickedObject.GetComponent<Card>();

                Debug.Log(gameManager.choiceCard.GetComponent<CardDragAndDrop>().completeChoice);
                gameManager.choiceCard.GetComponent<CardDragAndDrop>().completeChoice = true;

                yield return StartCoroutine(ApplyEffectCoroutine(gameManager.choiceCard.GetComponent<Card>().inf.effectInfs[0],
                                new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                                GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields, clickedObjectCard)));
            }
            else
            {
                OnCoroutine = false;
            }
        }

        if (OnButtonCoroutine)
        {
            clickedObject = GetCardObject(eventData.pointerCurrentRaycast.gameObject);
            if (clickedObject.tag == "Card")
            {
                yield break; // コルーチンを終了
            }
            else
            {
                OnButtonCoroutine = false;
            }
        }
    }

    public void canDragPanel(GameObject clickedObject)
    {
        if (clickedObject.transform.parent == myHand.transform)
        {
            gameManager.someTexts.text = "このカードはプレイできません";
            gameManager.panel.SetActive(true);
            StartCoroutine(WaitForClosePanel());
            canDrag = false;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // プレイシーンとターンステータスのチェック
        if (SceneManager.GetActiveScene().name != "playGame" || GameManager.turnStatus != GameManager.TurnStatus.OnPlay)
            return;

        InitializeSceneObjects();

        clickedObject = GetCardObject(eventData.pointerCurrentRaycast.gameObject);
        ApplyCardEffects();
        if (clickedObject == null || !CanDragCard(clickedObject, eventData))
            return;

        StartDrag();
    }

    private void InitializeSceneObjects()
    {
        currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName.Equals("playGame"))
        {
            myHand = GameObject.Find("myHand");
            myAttackField = GameObject.Find("myAttackField");
            myDefenceField = GameObject.Find("myDefenceField");
        }
        else if (currentSceneName.Equals("makeDeck"))
        {
            deckList = GameObject.Find("deckPanel").transform;
            cardOption = GameObject.Find("allCards").transform;
        }
    }

    private bool CanDragCard(GameObject cardObject, PointerEventData eventData)
    {
        var cardComponent = cardObject.GetComponent<Card>();

        if(!canDrag){
            canDragPanel(cardObject);
            return false;
        }

        // マナコストチェック
        if (gameManager.P1_mana < cardComponent.cost)
        {
            canDragPanel(cardObject);
            return false;
        }

        // 防御カードプレイ制限チェック
        if (CardManager.P1CannotPlayDefenceCard.Count != 0 && cardComponent.inf.cardType == CardType.Defence)
        {
            canDragPanel(cardObject);
            return false;
        }

        // カードの位置チェック
        if (eventData.position.y > 180)
        {
            canDragPanel(cardObject);
            return false;
        }

        canDrag = true;
        return true;
    }

    private void ApplyCardEffects()
    {
        foreach (var effect in card.inf.effectInfs)
        {
            foreach (var trigger in effect.triggers)
            {
                if (trigger == EffectInf.CardTrigger.OnBeginDrag)
                {
                    if (effect is ICardEffect cardEffect)
                    {
                        cardEffect.Apply(new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                                                                  GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
                    }
                }
                    
            }
        }
    }

    private void StartDrag()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 worldPosition = rectTransform.TransformPoint(rectTransform.rect.center);
        worldPosition.z = defaultZPosition;
        siblingIndex = transform.GetSiblingIndex();

        canvas = GameObject.Find("Canvas");
        defaultParent = canvas.transform;

        transform.SetParent(defaultParent, false);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (SceneManager.GetActiveScene().name == "playGame" && GameManager.turnStatus == GameManager.TurnStatus.OnPlay && canDrag)
        {
            if (currentSceneName.Equals("playGame"))
            {
                clickedObject = GetCardObject(eventData.pointerCurrentRaycast.gameObject);
                card = clickedObject.GetComponent<Card>();
                if (eventData.position.y > 180)
                {
                    AudioManager.Instance.PlayPlayCardSound();
                    
                    gameManager.P1_mana -= card.cost;
                    gameManager.P1_manaText.text = gameManager.manaChange();
                    if (card.inf.cardType == CardType.Defence)
                    {
                        StartCoroutine(DefenceMethod());
                    }
                    else if (card.inf.cardType == CardType.Attack)
                    {
                        StartCoroutine(AttackMethod());
                    }
                    else if (card.inf.cardType == CardType.Spel)
                    {
                        StartCoroutine(SpelMethod());
                    }
                    GameManager.PHands.Remove(card);
                    card.ActivePanel.SetActive(false);
                    canDrag = false;
                    //ここでアクティブパネルの操作をする
                    StartCoroutine(OnP1ManaChanged());
                }
                else
                {
                    transform.SetParent(myHand.transform, false);
                    transform.SetSiblingIndex(siblingIndex);
                }
            }
            else if (currentSceneName.Equals("makeDeck"))
            {
                if (eventData.position.y > 310)
                {
                    transform.SetParent(deckList, false);
                }
                else
                {
                    transform.SetParent(cardOption, false);
                    transform.SetSiblingIndex(siblingIndex);
                }
            }
            gameManager.detailPanel.SetActive(false);
        }
        

    }

    private IEnumerator OnP1ManaChanged()
    {
        foreach (var card in GameManager.PHands)
        {
            GameObject cardObject = card.gameObject;
            if (card.cost > gameManager.P1_mana)
            {
                cardObject.GetComponent<CardDragAndDrop>().canDrag = false;
            }
            else
            {
                cardObject.GetComponent<CardDragAndDrop>().canDrag = true;
            }
            if (CardManager.P1CannotPlayDefenceCard.Count != 0 && card.inf.cardType == CardType.Defence)
            {
                cardObject.GetComponent<CardDragAndDrop>().canDrag = false;
            }
            if (card.transform.position.y > 180)
            {
                cardObject.GetComponent<CardDragAndDrop>().canDrag = false;
            }
            foreach (var effect in card.inf.effectInfs)
            {
                for (int i = 0; i < effect.triggers.Count; i++)
                {
                    if (effect.triggers[i] == EffectInf.CardTrigger.OnBeginDrag)
                    {
                        if (effect is ICardEffect cardEffect)
                        {
                            ApplyEffectEventArgs args = new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields
                            , GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields);
                            yield return StartCoroutine(ApplyEffectCoroutine(cardEffect, args));
                        }
                    }
                }
            }
            card.ActivePanel.SetActive(cardObject.GetComponent<CardDragAndDrop>().canDrag);
        }
    }

    public IEnumerator ProcessCardEffectsCoroutine(Card card)
    {
        if (card.inf.cardType == CardType.Spel)
        {
            card.transform.SetParent(spelPanel, false);
        }
        // ランダム効果カードの特別な処理
        if (card.inf is RandomCardInf randomCardInf)
        {
            int randomValue = UnityEngine.Random.Range(0, randomCardInf.effectInfs.Count);
            yield return StartCoroutine(ApplyEffectCoroutine(randomCardInf.effectInfs[randomValue],
                new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields)));
            Destroy(card.gameObject);
            yield break; // これ以上の効果は処理しない
        }

        if (card.inf.effectInfs[0].triggers[0] == EffectInf.CardTrigger.OnPlay)
        {
            clickedObject.transform.SetParent(choicePanel, false);
            gameManager.choiceCard = clickedObject;
            StartCoroutine(WaitForChoiceCoroutine(card));
            yield break;
        }

        foreach (var effect in card.inf.effectInfs)
        {
            foreach (var trigger in effect.triggers)
            {
                switch (trigger)
                {
                    case EffectInf.CardTrigger.ButtonOperetion:
                        if (effect is ICardEffect buttonEffect)
                        {
                            yield return StartCoroutine(ApplyEffectCoroutine(buttonEffect,
                                new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                                GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields)));
                        }
                        OnButtonCoroutine = true;
                        StartCoroutine(WaitForButtonCoroutine(card));
                        break;

                    case EffectInf.CardTrigger.SpelEffectSomeTurn:
                        CardManager.P1SpelEffectAfterSomeTurn.Add(card);
                        card.transform.SetParent(spelPanel, false);
                        break;

                    case EffectInf.CardTrigger.AfterPlay:
                        if (effect is ICardEffect afterPlayEffect)
                        {
                            yield return StartCoroutine(ApplyEffectCoroutine(afterPlayEffect,
                                new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                                GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields)));
                        }
                        break;

                    case EffectInf.CardTrigger.FromPlayToDie:
                        if (effect is ICardEffect fromPlayToDieEffect)
                        {
                            yield return StartCoroutine(ApplyEffectCoroutine(fromPlayToDieEffect,
                                new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                                GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields)));
                        }
                        break;

                    case EffectInf.CardTrigger.OnFieldOnAfterPlay:
                        if (effect is ICardEffect onFieldEffect)
                        {
                            CardManager.P1CardsWithEffectOnField.Add(card);
                        }
                        break;

                    case EffectInf.CardTrigger.AfterPlayAndProtectShieldDecrease:
                        if (effect is ICardEffect afterPlayAndProtectShieldDecrease)
                        {
                            yield return StartCoroutine(ApplyEffectCoroutine(afterPlayAndProtectShieldDecrease,
                                new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                                GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields)));
                            CardManager.P1CardsWithEffectOnField.Add(card);
                        }
                        break;

                    default:
                        break;
                }
            }
        }
    }

    public IEnumerator SpelMethod()
    {
        yield return StartCoroutine(ProcessCardEffectsCoroutine(card));
        if (OnButtonCoroutine || transform.parent == spelPanel)
        {
        }else{
            Destroy(card.gameObject);
        }
        

    }

    public IEnumerator AttackMethod()
    {
        if (clickedObject.transform.parent != choicePanel.transform)
        {
            transform.SetParent(myAttackField.transform, false);
        }
        if (card.inf.effectInfs.Count != 0)
        {
            yield return StartCoroutine(ProcessCardEffectsCoroutine(card));
        }
        
        GameManager.PAttackFields.Add(card);
        GameManager.PAllFields.Add(card);
    }

    public IEnumerator DefenceMethod()
    {
        if (clickedObject.transform.parent != choicePanel.transform)
        {
            transform.SetParent(myDefenceField.transform, false);
        }
        yield return StartCoroutine(ProcessCardEffectsCoroutine(card));
        GameManager.PDefenceFields.Add(card);
        GameManager.PAllFields.Add(card);
    }

    private IEnumerator AfterChoiceEffectCoroutine(Card card)
    {
        OnCoroutine = false;
        foreach (var effect in card.inf.effectInfs)
        {
            foreach (var trigger in effect.triggers)
            {
                switch (trigger)
                {
                    case EffectInf.CardTrigger.ButtonOperetion:

                        if (effect is ICardEffect buttonEffect)
                        {
                            ApplyEffectEventArgs args = new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                                GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields);
                            yield return StartCoroutine(ApplyEffectCoroutine(buttonEffect, args));
                        }
                        OnButtonCoroutine = true;
                        yield return StartCoroutine(WaitForButtonCoroutine(card));
                        break;

                    case EffectInf.CardTrigger.SpelEffectSomeTurn:
                        CardManager.P1SpelEffectAfterSomeTurn.Add(card);
                        card.transform.SetParent(spelPanel, false);
                        break;

                    case EffectInf.CardTrigger.AfterPlay:
                        if (effect is ICardEffect afterPlayEffect)
                        {
                            ApplyEffectEventArgs args = new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                                GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields);
                            yield return StartCoroutine(ApplyEffectCoroutine(afterPlayEffect, args));
                        }
                        break;

                    case EffectInf.CardTrigger.FromPlayToDie:
                        if (effect is ICardEffect fromPlayToDieEffect)
                        {
                            ApplyEffectEventArgs args = new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                                GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields);
                            yield return StartCoroutine(ApplyEffectCoroutine(fromPlayToDieEffect, args));
                        }
                        break;

                    case EffectInf.CardTrigger.OnFieldOnAfterPlay:
                        if (effect is ICardEffect onFieldEffect)
                        {
                            CardManager.P1CardsWithEffectOnField.Add(card);
                        }
                        break;

                    case EffectInf.CardTrigger.AfterPlayAndProtectShieldDecrease:
                        if (effect is ICardEffect afterPlayAndProtectShieldDecrease)
                        {
                            ApplyEffectEventArgs args = new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields,
                                GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields);
                            yield return StartCoroutine(ApplyEffectCoroutine(afterPlayAndProtectShieldDecrease, args));
                            CardManager.P1CardsWithEffectOnField.Add(card);
                        }
                        break;

                    default:
                        break;
                }
            }
        }
    }

    public void ApplyOnFieldEffect()
    {
        foreach (var effectCard in CardManager.P1CardsWithEffectOnField)
        {
            foreach (var effect in effectCard.inf.effectInfs)
            {
                for (int i = 0; i < effect.triggers.Count; i++)
                {
                    if (effect.triggers[i] == EffectInf.CardTrigger.OnFieldOnAfterPlay)
                    {
                        if (effect is ICardEffect cardEffect)
                        {
                            cardEffect.Apply(new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields
                            , GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
                        }
                    }
                }
            }
        }
    }

    private IEnumerator WaitForClosePanel()
    {
        yield return new WaitForSeconds(0.5f);
        gameManager.panel.SetActive(false);

    }

    private IEnumerator WaitForChoiceCoroutine(Card card)
    {
        if (EnemyAI.EAllFields.Count == 0)
        {
            OnCoroutine = false;
        }
        else
        {
            OnCoroutine = true;
        }
        while (OnCoroutine)
        {
            yield return new WaitUntil(() => completeChoice);
            gameObject.transform.SetParent(myAttackField.transform, false);
            GameManager.PAttackFields.Add(card);
            yield return StartCoroutine(AfterChoiceEffectCoroutine(gameManager.choiceCard.GetComponent<Card>()));
        }
        yield return new WaitUntil(() => !OnCoroutine);
        transform.SetParent(myAttackField.transform, false);

    }

    private IEnumerator WaitForButtonCoroutine(Card card)
    {
        while (OnButtonCoroutine)
        {
            // `GameManager.completeButtonChoice` が true になるまで待機
            yield return new WaitUntil(() => GameManager.completeButtonChoice);

            foreach (var effect in card.inf.effectInfs)
            {
                for (int i = 0; i < effect.triggers.Count; i++)
                {
                    if (effect.triggers[i] == EffectInf.CardTrigger.StopButtonOperetion)
                    {
                        if (effect is ICardEffect cardEffect)
                        {
                            // 非同期処理を待機するためにコルーチンに変更
                            yield return ApplyEffectAsync(cardEffect, card);
                        }
                    }
                }
            }

            GameManager.completeButtonChoice = false;

            if (card.inf.cardType == CardType.Spel)
            {
                Destroy(card.gameObject);
            }
        }
    }

    // コルーチンを呼び出すためのラッパーメソッド
    private IEnumerator ApplyEffectAsync(ICardEffect cardEffect, Card card)
    {
        Task applyEffectTask = cardEffect.Apply(new ApplyEffectEventArgs(card, EnemyAI.EAllFields, EnemyAI.AttackFields, EnemyAI.DefenceFields
            , GameManager.PAllFields, GameManager.PAttackFields, GameManager.PDefenceFields));
        yield return new WaitUntil(() => applyEffectTask.IsCompleted);
    }

    private IEnumerator ApplyEffectCoroutine(ICardEffect cardEffect, ApplyEffectEventArgs args)
    {
        Task applyEffectTask = cardEffect.Apply(args);
        // Task の完了を待機
        yield return new WaitUntil(() => applyEffectTask.IsCompleted);
    }

    public GameObject GetCardObject(GameObject clickedGameObject)
    {
        Transform current = clickedGameObject.transform;
        while (current != null)
        {
            if (current.gameObject.GetComponent<Card>() != null)
            {
                return current.gameObject;
            }
            current = current.parent;
        }
        return null;
    }
}
