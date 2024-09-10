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
    public GameObject canvas;
    EffectManager effectManager;
    GameManager gameManager;
    ManaManager manaManager;
    UIManager uIManager;
    public CardManager player1CardManager;
    public CardManager player2CardManager;
    public GameObject myHand;
    public GameObject myAttackField;
    public GameObject myDefenceField;
    public Transform deckList;
    public Transform spelPanel;
    public Transform cardOption;
    Card card;
    [SerializeField] GameObject clickedObject;
    public bool completeChoice = false;
    public bool cancelChoice = false;
    public static bool OnCoroutine = false;
    public static bool OnButtonCoroutine = false;
    public bool canDrag = false;

    float defaultZPosition = 0;
    string currentSceneName;

    int siblingIndex = 0;

    void Start()
    {
        InitializeManagers();
        InitializeVariables();
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
    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(OnPointerClickCoroutine(eventData));
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        clickedObject = GetCardObject(eventData.pointerCurrentRaycast.gameObject);
        // プレイシーンとターンステータスのチェック
        if (SceneManager.GetActiveScene().name != "playGame" || GameManager.turnStatus != GameManager.TurnStatus.OnPlay || clickedObject.tag == "Enemy" || gameManager.isPlayerTurnProcessing)
        {
            canDrag = false;
            return;
        }

        InitializeSceneObjects();
        StartCoroutine(effectManager.OnBeginDragEffect(card).AsCoroutine());
        if (clickedObject == null || !CanDragCard(clickedObject, eventData))
            return;

        StartDrag();
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
        else if (!canDrag || clickedObject.tag == "Enemy")
        {
            return;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (SceneManager.GetActiveScene().name == "playGame" && GameManager.turnStatus == GameManager.TurnStatus.OnPlay && canDrag)
        {
            if (currentSceneName.Equals("playGame"))
            {
                HandlePlayGame(eventData);
            }
            else if (currentSceneName.Equals("makeDeck"))
            {
                HandleDekeMake(eventData);
            }
            uIManager.detailPanel.SetActive(false);
        }
    }
    private void InitializeManagers()
    {
        if (SceneManager.GetActiveScene().name == "playGame")
        {
            GameObject manager = GameObject.Find("GameManager");
            gameManager = manager.GetComponent<GameManager>();
            manaManager = manager.GetComponent<ManaManager>();
            effectManager = manager.GetComponent<EffectManager>();
            uIManager = manager.GetComponent<UIManager>();
            player1CardManager = GameObject.Find("P1CardManager").GetComponent<CardManager>();
            player2CardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
            spelPanel = GameObject.Find("SpelPanel").transform;
        }
    }

    private void InitializeVariables()
    {
        cancelChoice = false;
        completeChoice = false;
        OnCoroutine = false;
        currentSceneName = SceneManager.GetActiveScene().name;
        card = gameObject.GetComponent<Card>();
    }
    private void MoveCards(Vector3 direction)
    {
        gameObject.transform.localPosition += new Vector3(direction.x * 0.9f, 0, 0);

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

        if (!canDrag)
        {
            ShowCannotDragMessage(cardObject);
            return false;
        }

        // マナコストチェック
        if (manaManager.P1_mana < cardComponent.cost)
        {
            ShowCannotDragMessage(cardObject);
            return false;
        }

        // 防御カードプレイ制限チェック
        if (player1CardManager.CannotPlayDefenceCard.Count != 0 && cardComponent.inf.cardType == CardType.Defence)
        {
            ShowCannotDragMessage(cardObject);
            return false;
        }

        // カードの位置チェック
        if (card.transform.position.y > 180)
        {
            ShowCannotDragMessage(cardObject);
            return false;
        }

        canDrag = true;
        return true;
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

    private void HandlePlayGame(PointerEventData eventData)
    {
        clickedObject = GetCardObject(eventData.pointerCurrentRaycast.gameObject);
        card = clickedObject.GetComponent<Card>();
        if (eventData.position.y > 180)
        {
            StartCoroutine(PlayCard());
        }
        else
        {
            CancelPlayCard();
        }
    }

    private void HandleDekeMake(PointerEventData eventData)
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

    private IEnumerator PlayCard()
    {
        AudioManager.Instance.PlayPlayCardSound();
        yield return StartCoroutine(ExecuteChoiceCardEffect(card));
        if (!cancelChoice)
        {
            manaManager.P1ManaDecrease(card.cost);
            ExecuteCardEffect();
            AddCardOnFields();
            player1CardManager.Hands.Remove(card);
            card.ActivePanel.SetActive(false);
            canDrag = false;
            //ここでアクティブパネルの操作をする
            StartCoroutine(manaManager.OnP1ManaChanged());
        }

    }

    private void AddCardOnFields()
    {
        if (card.inf.cardType == CardType.Defence)
        {
            player1CardManager.DefenceFields.Add(card);
            player1CardManager.AllFields.Add(card);
        }
        else if (card.inf.cardType == CardType.Attack)
        {
            player1CardManager.AttackFields.Add(card);
            player1CardManager.AllFields.Add(card);
        }

    }

    private IEnumerator ExecuteChoiceCardEffect(Card card)
    {
        if (card.inf.effectInfs[0].triggers[0] == EffectInf.CardTrigger.OnPlay)
        {
            uIManager.ChoiceCardPlace.SetActive(true);
            clickedObject.transform.SetParent(uIManager.ChoicePanel.transform, false);
            player1CardManager.choiceCard = clickedObject;
            yield return StartCoroutine(WaitForChoiceCoroutine());
        }
    }

    private void ExecuteCardEffect()
    {
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
    }

    private void CancelPlayCard()
    {
        transform.SetParent(myHand.transform, false);
        transform.SetSiblingIndex(siblingIndex);
    }

    private IEnumerator ProcessCardEffectsCoroutine(Card card)
    {
        if (card.inf.cardType == CardType.Spel)
        {
            MoveCardToSpelPanel(card);
        }

        AudioManager.Instance.voiceSound(card.inf.voice);

        if (card.inf is RandomCardInf randomCardInf)
        {
            yield return effectManager.HandleRandomCardEffect(randomCardInf, card);
            yield break;
        }

        foreach (var effect in card.inf.effectInfs)
        {
            yield return ProcessEffectTriggers(card, effect);
        }
    }

    private IEnumerator OnPointerClickCoroutine(PointerEventData eventData)
    {
        if (OnCoroutine)
        {
            clickedObject = GetCardObject(eventData.pointerCurrentRaycast.gameObject);
            if (clickedObject.tag == "Enemy")
            {
                Card clickedObjectCard = clickedObject.GetComponent<Card>();
                player1CardManager.choiceCard.GetComponent<CardDragAndDrop>().completeChoice = true;
                yield return StartCoroutine(effectManager.ApplyEffectCoroutine(player1CardManager.choiceCard.GetComponent<Card>().inf.effectInfs[0],
                                new ApplyEffectEventArgs(card, player2CardManager.AllFields, player2CardManager.AttackFields, player2CardManager.DefenceFields,
                                player1CardManager.AllFields, player1CardManager.AttackFields, player1CardManager.DefenceFields, clickedObjectCard)));
            }
            else
            {
                OnCoroutine = false;
                player1CardManager.choiceCard.GetComponent<CardDragAndDrop>().completeChoice = true;
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

    private void MoveCardToSpelPanel(Card card)
    {
        card.transform.SetParent(spelPanel, false);
    }

    private IEnumerator ProcessEffectTriggers(Card card, EffectInf effect)
    {
        foreach (var trigger in effect.triggers)
        {
            yield return HandleTrigger(card, effect, trigger);
        }
    }

    private IEnumerator HandleTrigger(Card card, EffectInf effect, EffectInf.CardTrigger trigger)
    {
        switch (trigger)
        {
            case EffectInf.CardTrigger.ButtonOperetion:
                yield return effectManager.HandleButtonOperation(card, effect);
                break;

            case EffectInf.CardTrigger.SpelEffectSomeTurn:
                HandleSpelEffectSomeTurn(card);
                break;

            case EffectInf.CardTrigger.AfterPlay:
            case EffectInf.CardTrigger.FromPlayToDie:
            case EffectInf.CardTrigger.AfterPlayAndProtectShieldDecrease:
                yield return effectManager.HandleEffect(card, effect);
                break;

            case EffectInf.CardTrigger.OnFieldOnAfterPlay:
                player1CardManager.CardsWithEffectOnField.Add(card);
                break;

            default:
                break;
        }
    }
    private void HandleSpelEffectSomeTurn(Card card)
    {
        player1CardManager.SpelEffectAfterSomeTurn.Add(card);
        MoveCardToSpelPanel(card);
    }

    private IEnumerator SpelMethod()
    {
        yield return StartCoroutine(ProcessCardEffectsCoroutine(card));
        if (OnButtonCoroutine || transform.parent == spelPanel)
        {
        }
        else
        {
            Destroy(card.gameObject);
        }
    }

    private IEnumerator AttackMethod()
    {
        if (clickedObject.transform.parent != uIManager.ChoiceCardPlace.transform)
        {
            transform.SetParent(myAttackField.transform, false);
        }
        if (card.inf.effectInfs.Count != 0)
        {
            yield return StartCoroutine(ProcessCardEffectsCoroutine(card));
        }
    }

    private IEnumerator DefenceMethod()
    {
        if (clickedObject.transform.parent != uIManager.ChoiceCardPlace.transform)
        {
            transform.SetParent(myDefenceField.transform, false);
        }
        yield return StartCoroutine(ProcessCardEffectsCoroutine(card));
    }
    private IEnumerator WaitForClosePanel()
    {
        yield return new WaitForSeconds(0.5f);
        uIManager.messagePanel.SetActive(false);
    }

    public IEnumerator WaitForChoiceCoroutine()
    {
        if (player2CardManager.AllFields.Count == 0)
        {
            OnCoroutine = false;
        }
        else
        {
            OnCoroutine = true;
        }
        yield return new WaitUntil(() => completeChoice);
        if (OnCoroutine)
        {
            cancelChoice = false;
            gameObject.transform.SetParent(myAttackField.transform, false);
            uIManager.ChoiceCardPlace.SetActive(false);
            player1CardManager.AttackFields.Add(card);
        }
        else
        {
            completeChoice = false;
            cancelChoice = true;
            transform.SetParent(myHand.transform, false);
            uIManager.ChoiceCardPlace.SetActive(false);
        }
    }

    private void ShowCannotDragMessage(GameObject clickedObject)
    {
        if (clickedObject.transform.parent == myHand.transform)
        {
            uIManager.ChangeMessageTexts("このカードはプレイできません");
            uIManager.messagePanel.SetActive(true);
            StartCoroutine(WaitForClosePanel());
            canDrag = false;
        }
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
