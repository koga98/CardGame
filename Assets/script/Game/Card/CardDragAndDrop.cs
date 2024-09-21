using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using GameNamespace;

public class CardDragAndDrop : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
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
    public Transform spelPanel;
    Card card;
    [SerializeField] GameObject clickedObject;
    public bool completeChoice = false;
    public bool cancelChoice = false;
    public static bool OnCoroutine = false;
    public static bool OnButtonCoroutine = false;
    public bool canDrag = false;
    string currentSceneName;

    int siblingIndex = 0;

    void Start()
    {
        InitializeManagers();
        InitializeVariables();
        InitializeSceneObjects();
    }
    private void InitializeManagers()
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

    private void InitializeVariables()
    {
        cancelChoice = false;
        completeChoice = false;
        OnCoroutine = false;
        currentSceneName = SceneManager.GetActiveScene().name;
        card = gameObject.GetComponent<Card>();
    }


    void Update()
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(OnPointerClickCoroutine(eventData));
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
                yield return StartCoroutine(effectManager.PlayCardChoiceEffect(clickedObjectCard));
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
                yield break;

            else
                OnButtonCoroutine = false;
        }
    }

    private GameObject GetCardObject(GameObject clickedGameObject)
    {
        Transform current = clickedGameObject.transform;
        while (current != null)
        {
            if (current.gameObject.GetComponent<Card>() != null)
                return current.gameObject;
            current = current.parent;
        }
        return null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        clickedObject = GetCardObject(eventData.pointerCurrentRaycast.gameObject);
        // プレイシーンとターンステータスのチェック
        if (SceneManager.GetActiveScene().name == "playGame")
        {
            if (GameManager.turnStatus != GameManager.TurnStatus.OnPlay || clickedObject.tag == "Enemy" || gameManager.isPlayerTurnProcessing)
            {
                canDrag = false;
                return;
            }
            StartCoroutine(effectManager.BeforeCardDrag(card).AsCoroutine());
            if (clickedObject == null || !CanDragCard(clickedObject, eventData))
                return;
        }
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

    private IEnumerator WaitForClosePanel()
    {
        yield return new WaitForSeconds(0.5f);
        uIManager.messagePanel.SetActive(false);
    }

    //Canvas直下に移動
    private void StartDrag()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 worldPosition = rectTransform.TransformPoint(rectTransform.rect.center);
        worldPosition.z = 0;
        siblingIndex = transform.GetSiblingIndex();
        canvas = GameObject.Find("Canvas");
        defaultParent = canvas.transform;
        transform.SetParent(defaultParent, false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (GameManager.turnStatus == GameManager.TurnStatus.OnPlay && canDrag)
        {
            DragCard(eventData);
        }
        else if (!canDrag || clickedObject.tag == "Enemy")
            return;
    }

    private void DragCard(PointerEventData eventData)
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

    public void OnEndDrag(PointerEventData eventData)
    {
        if (GameManager.turnStatus == GameManager.TurnStatus.OnPlay && canDrag)
        {
            if (currentSceneName.Equals("playGame"))
                HandlePlayGame(eventData);
        }
    }

    private void HandlePlayGame(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject == null)
        {
            CancelPlayCard();
        }
        else if (eventData.position.y > 180)
        {
            clickedObject = GetCardObject(eventData.pointerCurrentRaycast.gameObject);
            card = clickedObject.GetComponent<Card>();
            StartCoroutine(PlayCard());
        }
        else
        {
            CancelPlayCard();
        }

    }

    private void CancelPlayCard()
    {
        transform.SetParent(myHand.transform, false);
        transform.SetSiblingIndex(siblingIndex);
    }

    private IEnumerator PlayCard()
    {
        gameManager.isDealing = true;
        AudioManager.Instance.PlayPlayCardSound();
        if (card.inf.effectInfs == null || card.inf.effectInfs.Count == 0)
        {
            manaManager.P1ManaDecrease(card.cost);
            ChangePlayCardTransform();
            AddCardOnFields();
            player1CardManager.Hands.Remove(card);
            card.ActivePanel.SetActive(false);
            canDrag = false;
            //ここでアクティブパネルの操作をする
            StartCoroutine(manaManager.OnP1ManaChanged());
            gameManager.isDealing = false;
            yield break;
        }
        else
        {
            if (player2CardManager.AllFields.Count != 0)
                yield return StartCoroutine(ExecuteChoiceCardEffect(card));
            if (!cancelChoice)
            {
                manaManager.P1ManaDecrease(card.cost);
                ChangePlayCardTransform();
                AddCardOnFields();
                player1CardManager.Hands.Remove(card);
                card.ActivePanel.SetActive(false);
                canDrag = false;
                //ここでアクティブパネルの操作をする
                StartCoroutine(manaManager.OnP1ManaChanged());
            }
        }

    }

    private IEnumerator ExecuteChoiceCardEffect(Card card)
    {
        if (card.inf.effectInfs[0].triggers[0] == EffectInf.CardTrigger.OnPlay)
        {
            uIManager.ChoicePanel.SetActive(true);
            clickedObject.transform.SetParent(uIManager.ChoiceCardPlace.transform, false);
            player1CardManager.choiceCard = clickedObject;
            yield return StartCoroutine(WaitForChoiceCoroutine());
        }
    }

    private IEnumerator WaitForChoiceCoroutine()
    {
        if (player2CardManager.AllFields.Count == 0)
            OnCoroutine = false;
        else
            OnCoroutine = true;

        yield return new WaitUntil(() => completeChoice);
        if (OnCoroutine)
        {
            cancelChoice = false;
            gameObject.transform.SetParent(myAttackField.transform, false);
            uIManager.ChoicePanel.SetActive(false);
        }
        else
        {
            completeChoice = false;
            cancelChoice = true;
            transform.SetParent(myHand.transform, false);
            transform.SetSiblingIndex(siblingIndex);
            uIManager.ChoicePanel.SetActive(false);
        }
    }

    private void ChangePlayCardTransform()
    {
        uIManager.DetailPanelActive(card.inf);
        if (card.inf.cardType == CardType.Defence)
            StartCoroutine(PlayDefenceCard());
        else if (card.inf.cardType == CardType.Attack)
            StartCoroutine(PlayAttackCard());
        else if (card.inf.cardType == CardType.Spel)
            StartCoroutine(PlaySpelCard());
    }
    private IEnumerator PlayDefenceCard()
    {
        if (clickedObject.transform.parent != uIManager.ChoiceCardPlace.transform)
            transform.SetParent(myDefenceField.transform, false);
        yield return StartCoroutine(ProcessPlayCardEffectsCoroutine(card));
        uIManager.DetailPanelInactive();
    }

    private IEnumerator PlayAttackCard()
    {
        if (clickedObject.transform.parent != uIManager.ChoiceCardPlace.transform)
            transform.SetParent(myAttackField.transform, false);
        if (card.inf.effectInfs.Count != 0)
            yield return StartCoroutine(ProcessPlayCardEffectsCoroutine(card));
        uIManager.DetailPanelInactive();
    }
    private IEnumerator PlaySpelCard()
    {
        card.transform.SetParent(uIManager.temporarySpelPanel.transform, false);
        yield return StartCoroutine(ProcessPlayCardEffectsCoroutine(card));
        if (card.transform.parent != GameObject.Find("SpelPanel").transform)
            yield return StartCoroutine(card.DestoryThis().AsCoroutine());
        uIManager.DetailPanelInactive();
    }

    private IEnumerator ProcessPlayCardEffectsCoroutine(Card card)
    {
        AudioManager.Instance.voiceSound(card.inf.voice);

        if (card.inf is RandomCardInf randomCardInf)
        {
            yield return StartCoroutine(effectManager.PlayCardRandomEffect(randomCardInf, card).AsCoroutine());
            gameManager.isDealing = false;
            yield break;
        }
        if (card.inf.effectInfs != null && card.inf.effectInfs.Count != 0)
            foreach (var effect in card.inf.effectInfs)
                yield return StartCoroutine(ExecuteAllPlayCardEffect(card, effect));
    }

    private IEnumerator ExecuteAllPlayCardEffect(Card card, EffectInf effect)
    {

        foreach (var trigger in effect.triggers)
            yield return StartCoroutine(ExecuteEachPlayCardEffect(card, effect, trigger));

    }

    private IEnumerator ExecuteEachPlayCardEffect(Card card, EffectInf effect, EffectInf.CardTrigger trigger)
    {
        switch (trigger)
        {
            case EffectInf.CardTrigger.ButtonOperetion:
                yield return StartCoroutine(effectManager.PlayCardButtonEffect(card, effect));
                break;

            case EffectInf.CardTrigger.SpelEffectSomeTurn:
                PlayCardEffectSpelNeedsSomeTurn(card);
                break;

            case EffectInf.CardTrigger.AfterPlay:
            case EffectInf.CardTrigger.ProtectShieldDecrease:
                yield return StartCoroutine(effectManager.PlayCardEffect(card, effect));
                break;

            case EffectInf.CardTrigger.OnFieldOnAfterPlay:
                player1CardManager.CardsWithEffectOnField.Add(card);
                break;

            default:
                break;
        }
        gameManager.isDealing = false;
    }

    private void PlayCardEffectSpelNeedsSomeTurn(Card card)
    {
        player1CardManager.SpelEffectAfterSomeTurn.Add(card);
        MoveCardToSpelPanel(card);
    }

    //スペルが効果起動時に画面から消すため
    private void MoveCardToSpelPanel(Card card)
    {
        card.transform.SetParent(spelPanel, false);
    }

    private void AddCardOnFields()
    {
        if (gameObject.activeSelf)
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

    }
}
