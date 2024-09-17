using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckMakeDragAndDrop : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    DeckMake deckMake;
    public ClickAdd clickAdd;
    Vector3 touchPosition;
    GameObject originalCard;
    public GameObject canvas;
    public Transform defaultParent;
    bool isDeckListCard = false;
    public bool canDrag = false;
    public HorizontalLayoutGroup horizontalLayoutGroup;
    int siblingIndex = 0;
    private const float AddBorderLine = 310.0f;
    private const float RemoveBorderLine = 620.0f;
    // Start is called before the first frame update
    void Start()
    {
        deckMake = GameObject.Find("GameObject").GetComponent<DeckMake>();
        horizontalLayoutGroup = deckMake.deckList.GetComponent<HorizontalLayoutGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.transform.position.y > 310)
        {
            if (Input.GetMouseButtonDown(0))
                touchPosition = Input.mousePosition;

            if (Input.GetMouseButton(0))
            {
                Vector3 direction = Input.mousePosition - touchPosition;
                touchPosition = Input.mousePosition;
                if (direction != Vector3.zero && !deckMake.restrictMoveCards)
                    MoveCards(direction);
            }
        }
    }

    private void MoveCards(Vector3 direction)
    {
        gameObject.transform.localPosition += new Vector3(direction.x * 0.9f, 0, 0);

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        deckMake.GetCardIdBothSide();

        isDeckListCard = gameObject.transform.parent == deckMake.deckList;
        canDrag = clickAdd.amount < 3 || isDeckListCard;
        if (!canDrag)
            return;

        InstantiateCopyCard();
        StartDrag();
    }

    private void InstantiateCopyCard()
    {
        siblingIndex = transform.GetSiblingIndex();
        Transform parentTransform = gameObject.transform.parent == deckMake.deckList ? deckMake.deckList : deckMake.cardOption;

        originalCard = Instantiate(clickAdd.prefab, parentTransform, false);
        originalCard.GetComponent<ClickAdd>().amount = clickAdd.amount;
        originalCard.GetComponent<ClickAdd>().copyObject = clickAdd.copyObject;
        originalCard.GetComponent<ClickAdd>().myObject = clickAdd.myObject;

        originalCard.GetComponent<Card>().P1SetUp(gameObject.GetComponent<Card>().inf);
        originalCard.transform.SetSiblingIndex(siblingIndex);
        deckMake.restrictMoveCards = true;
    }

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
        if (canDrag)
            DragCard(eventData);
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
        if (canDrag)
        {
            if (!isDeckListCard)
                CardAddDeckList(eventData);
            else
                RemoveCardDeckList(eventData);
        }
    }

    private void CardAddDeckList(PointerEventData eventData)
    {
        if (eventData.position.y > AddBorderLine)
        {
            if (clickAdd.copyObject == null)
                clickAdd.AddToDeckList(originalCard);
            else if (clickAdd.copyObject.GetComponent<ClickAdd>().amount <= 2)
                clickAdd.MaxAddToDeckList(originalCard);
        }
        deckMake.pageObject.Remove(gameObject);
        deckMake.pageObject.Insert(siblingIndex % deckMake.pageObject.Count, originalCard);
        originalCard.GetComponent<ClickAdd>().copyObject = clickAdd.copyObject;
        deckMake.restrictMoveCards = false;
        Destroy(gameObject);
    }

    private void RemoveCardDeckList(PointerEventData eventData)
    {
        if (eventData.position.y <= RemoveBorderLine)
        {
            if (originalCard.GetComponent<ClickAdd>().amount == 1)
                clickAdd.DestroyDeckListCard(originalCard);
            else if (originalCard.GetComponent<ClickAdd>().amount >= 2)
                clickAdd.RemoveFromDeckList(originalCard, originalCard.GetComponent<ClickAdd>().myObject);
        }
        originalCard.GetComponent<ClickAdd>().copyObject = originalCard;
        originalCard.GetComponent<ClickAdd>().myObject.GetComponent<ClickAdd>().copyObject = originalCard.GetComponent<ClickAdd>().copyObject;
        deckMake.restrictMoveCards = false;
        Destroy(gameObject);
    }
}
