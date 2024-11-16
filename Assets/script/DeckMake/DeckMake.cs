using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DeckMake : MonoBehaviour
{
    public AllCardInf deckInf;
    [SerializeField] private Text deckNumber;
    public GameObject cardPrehfab;
    public List<GameObject> pageObject;
    public Transform deckList;
    public Transform cardOption;
    public GameObject deckObject;
    public static int deckAmount;
    string filePath;
    public List<Card> cardLists;
    public GameObject nextButton;
    public GameObject preButton;
    Dictionary<int, int> counts = new Dictionary<int, int>();
    private int nowPage = 0;
    [SerializeField] private AllCardInf allCardInfList;
    public GameObject detailPanel;
    public int minId = 1000;
    public int maxId = 0;
    public bool restrictMoveCards = false;
    private const int CardsPerPage = 8;
    int maxPage = 0;

    // Start is called before the first frame update
    void Start()
    {
        maxPage = allCardInfList.allList.Count / CardsPerPage;
        maxPage += allCardInfList.allList.Count % CardsPerPage == 0 ? 0 : 1;
        UpdatePrePageButtonState();
        UpdateNextPageButtonState();
        InitializeCards();
        LoadCardData(DeckChiceButton.passNumber);
    }

    private void UpdatePrePageButtonState()
    {
        preButton.SetActive(nowPage > 0);
    }

    private void UpdateNextPageButtonState()
    {
        preButton.SetActive(nowPage < maxPage);
    }

    // Update is called once per frame
    void Update()
    {
        if (nowPage == 0)
            preButton.SetActive(false);


        if (Input.GetMouseButtonDown(0))
        {
            if (detailPanel.activeSelf)
                detailPanel.SetActive(false);
        }
    }

    private void InitializeCards()
    {
        cardLists = new List<Card>();
        //myObject
        for (int i = 0; i < allCardInfList.allList.Count; i++)
        {
            GameObject card = Instantiate(cardPrehfab, cardOption, false);
            card.GetComponent<Card>().P1SetUp(allCardInfList.allList[i]);
            card.transform.localScale = Vector3.one;
            pageObject.Add(card);
            if (i > 7)
                card.SetActive(false);

        }
    }

    private void LoadCardData(int passNumber)
    {
        filePath = Application.persistentDataPath + "/SaveData.json" + passNumber;
        if (File.Exists(filePath))
        {
            StreamReader streamReader;
            streamReader = new StreamReader(filePath);
            string data = streamReader.ReadToEnd();
            streamReader.Close();
            DeckDatabaseCollection collection = JsonUtility.FromJson<DeckDatabaseCollection>(data);
            //ロードしたのが何番目のデータなのかを検知して
            if (collection != null)
            {
                CardManager.DeckInf = collection.cardDataLists[0].idLists;
                deckAmount = collection.cardDataLists[0].idLists.Count;
                deckNumber.text = deckAmount.ToString() + "/40";

                foreach (int id in CardManager.DeckInf)
                {
                    if (counts.ContainsKey(id))
                        counts[id]++;
                    else
                        counts[id] = 1;
                }
                //copyCard
                foreach (KeyValuePair<int, int> entry in counts)
                {
                    CreateCard(entry.Key, entry.Value);
                }
            }
        }
        else
            deckAmount = 0;
    }

    public void DeckMakeMethod(int myButtonNumber)
    {
        int count = 0;
        deckInf.allList.Clear();
        List<GameObject> childObjects = GetAllChildren();
        foreach (GameObject child in childObjects)
        {
            Card card = child.GetComponent<Card>();
            ClickAdd click = child.GetComponent<ClickAdd>();
            for (int i = 0; i < click.amount; i++)
            {
                deckInf.allList.Add(card.inf);
            }
        }
        foreach (CardInf cardInf in deckInf.allList)
        {
            SaveButton.deckDatabase.idLists.Add(cardInf.Id);
            count++;
        }
        SaveButton.deckDatabaseCollection.cardDataLists.Add(SaveButton.deckDatabase);
    }

    private void CreateCard(int cardId, int count)
    {
        GameObject cardObj = Instantiate(cardPrehfab, deckList, false);
        var card = cardObj.GetComponent<Card>();
        card.P1SetUp(allCardInfList.allList[cardId]);
        card.GetComponent<ClickAdd>().myObject = pageObject[card.inf.Id];
        pageObject[card.inf.Id].GetComponent<ClickAdd>().copyObject = cardObj;
        cardObj.GetComponent<ClickAdd>().copyObject = cardObj;
        card.GetComponent<ClickAdd>().copyObject = cardObj;
        card.GetComponent<ClickAdd>().amount = count;
        card.reflectAmount(count);
        if (count == 3)
            card.GetComponent<ClickAdd>().myObject.GetComponent<Card>().backColor.color = Color.black;
        cardObj.transform.localScale = Vector3.one;
    }
    public void NextPage()
    {
        ChangePage(1);
    }

    public void PrePage()
    {
        ChangePage(-1);
    }

    private void ChangePage(int direction)
    {
        nowPage += direction;
        pageObject.ForEach(obj => obj.SetActive(false));
        pageObject.Clear();
        nextButton.SetActive(true);

        int startIdx = nowPage * CardsPerPage;
        for (int i = startIdx; i < startIdx + CardsPerPage; i++)
        {
            if (i < allCardInfList.allList.Count)
            {
                List<GameObject> objects = GetAllChildrenOption();
                objects[i].SetActive(true);
                if (objects[i].GetComponent<ClickAdd>().amount == 3)
                {
                    objects[i].GetComponent<Card>().backColor.color = Color.black;
                }
                pageObject.Add(objects[i]);
                if (i + 1 == allCardInfList.allList.Count)
                {
                    nextButton.SetActive(false);
                    break;
                }

            }
            else
            {
                nextButton.SetActive(false);
                break;
            }
        }
        UpdatePrePageButtonState();
        UpdateNextPageButtonState();
    }

    List<GameObject> GetAllChildren()
    {
        List<GameObject> children = new List<GameObject>();

        // 親オブジェクトのTransformコンポーネントを使用して子オブジェクトを取得
        foreach (Transform child in deckList)
        {
            children.Add(child.gameObject);
        }

        return children;
    }

    List<GameObject> GetAllChildrenOption()
    {
        List<GameObject> children = new List<GameObject>();

        // 親オブジェクトのTransformコンポーネントを使用して子オブジェクトを取得
        for (int i = 0; i < cardOption.childCount; i++)
        {
            Transform child = cardOption.GetChild(i);
            children.Add(child.gameObject);
        }
        return children;
    }

    public void GetCardIdBothSide()
    {
        int nowMinId = minId;
        int nowMaxId = maxId;
        foreach (Transform child in deckList)
        {
            if (86.0 <= child.position.x && child.position.x <= 248.0)
            {
                minId = child.gameObject.GetComponent<Card>().inf.Id;
            }
            else if (1711.0 <= child.position.x && child.position.x <= 1874.0)
            {
                maxId = child.gameObject.GetComponent<Card>().inf.Id;
            }

            minId = (minId == nowMinId) ? deckList.GetChild(0).gameObject.GetComponent<Card>().inf.Id : minId;
            maxId = (maxId == nowMaxId) ? deckList.GetChild(deckList.childCount - 1).gameObject.GetComponent<Card>().inf.Id : maxId;
        }
    }

    public int GetNumberofLeftOverCards()
    {
        int number = 0;
        foreach (Transform child in deckList)
        {
            if (-40 >= child.position.x)
            {
                number++;
            }
        }
        return number;
    }

    public int GetNumberofRightOverCards()
    {
        int number = 0;
        foreach (Transform child in deckList)
        {
            if (2000 <= child.position.x)
            {
                number++;
            }
        }
        return number;
    }

    public bool LayOutRight()
    {
        return GetNumberofLeftOverCards() > GetNumberofRightOverCards() && deckList.childCount > 11;
    }
}
