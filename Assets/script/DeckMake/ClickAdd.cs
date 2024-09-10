using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
public class ClickAdd : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Transform deckList;
    public Transform cardOption;
    private string currentSceneName;
    private GameObject prefab;
    public GameObject copyObject;
    public GameObject myObject;
   
    public int amount = 0;
    [SerializeField] private bool isAdd = false;
    [SerializeField] private bool onCard = false;
    public int siblingIndex = 0;

    [SerializeField] private Text deckNumber;

    void Start()
    {

        currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName.Equals("makeDeck"))
        {
            prefab = Resources.Load<GameObject>("cardDesign");
            deckList = GameObject.Find("deckPanel").transform;
            cardOption = GameObject.Find("allCards").transform;
            deckNumber = GameObject.Find("DeckNumber").GetComponent<Text>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            RightClickHandle();
        }
    }

    private void RightClickHandle()
    {
        if (isAdd == true && onCard == true)
        {
            if (copyObject == null)
            {
                AddToDeckList();
            }
            else if (copyObject.GetComponent<ClickAdd>().amount <= 2)
            {
                MaxAddToDeckList();

            }
            siblingIndex = deckList.childCount - 1;
            deckNumber.text = DeckMake.deckAmount + "/40";
        }
        if (isAdd == false && onCard == true)
        {
            if (amount == 1)
            {
                Destroy(copyObject);
            }
            else if (amount >= 2)
            {
                RemoveFromDeckList();

            }
            siblingIndex = deckList.childCount - 1;
            DeckMake.deckAmount--;
            deckNumber.text = (DeckMake.deckAmount) + "/40";
        }
    }

    private void AddToDeckList()
    {
        copyObject = Instantiate(prefab, deckList, false);
        copyObject.GetComponent<ClickAdd>().myObject = gameObject;
        copyObject.GetComponent<ClickAdd>().copyObject = copyObject;
        Card copy = copyObject.GetComponent<Card>();
        copy.P1SetUp(gameObject.GetComponent<Card>().inf);
        copy.reflectAmount(copyObject.GetComponent<ClickAdd>().amount + 1);
        copyObject.GetComponent<ClickAdd>().amount++;
        SortChildren();
        DeckMake.deckAmount++;
    }

    private void MaxAddToDeckList()
    {
        Card copy = copyObject.GetComponent<Card>();
        copy.reflectAmount(copyObject.GetComponent<ClickAdd>().amount + 1);
        copyObject.GetComponent<ClickAdd>().amount++;
        DeckMake.deckAmount++;
        if (copyObject.GetComponent<ClickAdd>().amount >= 3)
        {
            gameObject.GetComponent<CardAnimation>().CanOperate();
        }
    }

    private void RemoveFromDeckList()
    {
        if (amount == 3)
        {
            myObject.GetComponent<CardAnimation>().CannotOperate();
        }
        amount--;
        gameObject.GetComponent<Card>().reflectAmount(amount);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentSceneName.Equals("makeDeck"))
        {
            if (eventData.pointerCurrentRaycast.gameObject.tag == "Card" && transform.parent == cardOption)
            {
                isAdd = true;
                onCard = true;
            }
            if (eventData.pointerCurrentRaycast.gameObject.tag == "Card" && transform.parent == deckList)
            {
                isAdd = false;
                onCard = true;
            }
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentSceneName.Equals("makeDeck"))
        {
            isAdd = false;
            onCard = false;
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

    public void SortChildren()
    {
        List<Transform> children = new List<Transform>();

        // 親オブジェクトのTransformコンポーネントを使用して子オブジェクトを取得
        foreach (Transform child in deckList)
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
