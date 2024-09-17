using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
public class ClickAdd : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private DeckMake deckMake;
    public GameObject prefab;
    public GameObject copyObject;
    public GameObject myObject;
    public HorizontalLayoutGroup horizontalLayoutGroup;
    public int paddingLeft;
    public int amount = 0;
    [SerializeField] private bool isAdd = false;
    [SerializeField] private bool onCard = false;
    [SerializeField] private Text deckNumber;

    void Start()
    {
        InitializeVariables();
    }

    private void InitializeVariables()
    {
        deckMake = GameObject.Find("GameObject").GetComponent<DeckMake>();
        prefab = Resources.Load<GameObject>("DeckMakeCard");
        deckNumber = GameObject.Find("DeckNumber").GetComponent<Text>();
        horizontalLayoutGroup = deckMake.deckList.GetComponent<HorizontalLayoutGroup>();
        paddingLeft = horizontalLayoutGroup.padding.left;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            deckMake.GetCardIdBothSide();
            RightClickHandle();
        }
    }

    private void RightClickHandle()
    {
        if (isAdd == true && onCard == true)
        {
            if (copyObject == null)
            {
                AddToDeckList(gameObject);
            }
            else if (copyObject.GetComponent<ClickAdd>().amount <= 2)
            {
                MaxAddToDeckList(gameObject);
            }
        }
        if (isAdd == false && onCard == true)
        {
            if (amount == 1)
            {
                DestroyDeckListCard(copyObject);
            }
            else if (amount >= 2)
            {
                RemoveFromDeckList(gameObject, myObject);

            }
        }
    }

    public void AddToDeckList(GameObject baseObject)
    {
        copyObject = Instantiate(prefab);
        copyObject.GetComponent<ClickAdd>().myObject = baseObject;
        copyObject.GetComponent<ClickAdd>().copyObject = copyObject;
        Card copy = copyObject.GetComponent<Card>();
        copy.P1SetUp(baseObject.GetComponent<Card>().inf);
        copyObject.transform.SetParent(deckMake.deckList, false);
        copy.reflectAmount(copyObject.GetComponent<ClickAdd>().amount + 1);
        copyObject.GetComponent<ClickAdd>().amount++;
        baseObject.GetComponent<ClickAdd>().amount = copyObject.GetComponent<ClickAdd>().amount;
        ArrangeLayout(copy);
        SortChildren();

        DeckMake.deckAmount++;
        UpdateDeckNumber();
    }

    public void MaxAddToDeckList(GameObject baseObject)
    {
        Card copy = copyObject.GetComponent<Card>();
        copyObject.GetComponent<ClickAdd>().myObject = baseObject;
        copy.reflectAmount(copyObject.GetComponent<ClickAdd>().amount + 1);
        copyObject.GetComponent<ClickAdd>().amount++;
        baseObject.GetComponent<ClickAdd>().amount = copyObject.GetComponent<ClickAdd>().amount;
        ArrangeLayout(copy);
        DeckMake.deckAmount++;
        UpdateDeckNumber();
        if (copyObject.GetComponent<ClickAdd>().amount >= 3)
        {
            baseObject.GetComponent<Card>().backColor.color = Color.black;
        }
    }

    public void RemoveFromDeckList(GameObject decklistCard, GameObject cardOptionCard)
    {
        if (decklistCard.GetComponent<ClickAdd>().amount == 3)
        {
            cardOptionCard.GetComponent<Card>().backColor.color = cardOptionCard.GetComponent<Card>().baseColor;
        }
        decklistCard.GetComponent<ClickAdd>().amount--;
        cardOptionCard.GetComponent<ClickAdd>().amount = decklistCard.GetComponent<ClickAdd>().amount;
        decklistCard.GetComponent<Card>().reflectAmount(decklistCard.GetComponent<ClickAdd>().amount);
        DeckMake.deckAmount--;
        UpdateDeckNumber();
    }

    public void DestroyDeckListCard(GameObject destroyCard)
    {
        if (deckMake.LayOutRight())
        {
            horizontalLayoutGroup.childAlignment = TextAnchor.MiddleRight;
            horizontalLayoutGroup.padding = new RectOffset(paddingLeft - 165 * deckMake.GetNumberofLeftOverCards(), 0, 0, 0);
        }
        else
        {
            horizontalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
            horizontalLayoutGroup.padding = new RectOffset(paddingLeft, 0, 0, 0);
        }
        Destroy(destroyCard);
        DeckMake.deckAmount--;
        UpdateDeckNumber();
    }
    private void UpdateDeckNumber()
    {
        deckNumber.text = DeckMake.deckAmount + "/40";
    }

    private void ArrangeLayout(Card card)
    {
        if (deckMake.deckList.childCount > 12)
        {
            if (card.inf.Id > deckMake.maxId)
            {
                int arrangeCount = deckMake.deckList.childCount - 11;
                int result = paddingLeft - 170 * arrangeCount;
                horizontalLayoutGroup.padding = new RectOffset(result, 0, 0, 0);
                horizontalLayoutGroup.childAlignment = TextAnchor.MiddleRight;
            }
            else if (card.inf.Id < deckMake.minId)
            {
                horizontalLayoutGroup.padding = new RectOffset(paddingLeft, 0, 0, 0);
                horizontalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.tag == "Card" && transform.parent == deckMake.cardOption)
        {
            isAdd = true;
            onCard = true;
        }
        if (eventData.pointerCurrentRaycast.gameObject.tag == "Card" && transform.parent == deckMake.deckList)
        {
            isAdd = false;
            onCard = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isAdd = false;
        onCard = false;
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

    public void SortChildren()
    {
        List<Transform> children = new List<Transform>();

        // 親オブジェクトのTransformコンポーネントを使用して子オブジェクトを取得
        foreach (Transform child in deckMake.deckList)
        {
            children.Add(child);
        }
        children.Sort((card1, card2) => card1.GetComponent<Card>().inf.Id.CompareTo(card2.GetComponent<Card>().inf.Id));

        for (int i = 0; i < children.Count; i++)
        {
            children[i].SetSiblingIndex(i);
        }

    }

}
