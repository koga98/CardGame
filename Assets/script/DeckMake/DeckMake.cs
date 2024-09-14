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

    // Start is called before the first frame update
    void Start()
    {
        InitializeCards();
        LoadCardData(DeckChiceButton.passNumber);

    }

    // Update is called once per frame
    void Update()
    {
        if (nowPage == 0)
            preButton.SetActive(false);


        if (Input.GetMouseButtonDown(0))
        {
            if (detailPanel.activeSelf)
            {
                detailPanel.SetActive(false);
            }
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
            {
                card.SetActive(false);
            }

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
                    {
                        counts[id]++;
                    }
                    else
                    {
                        counts[id] = 1;
                    }
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
        card.GetComponent<ClickAdd>().copyObject = cardObj;
        card.GetComponent<ClickAdd>().amount = count;
        card.reflectAmount(count);
        if (count == 3)
        {
            card.GetComponent<ClickAdd>().myObject.GetComponent<Card>().backColor.color = Color.black;
        }
        cardObj.transform.localScale = Vector3.one;
    }
    public void NextPage()
    {
        nowPage++;
        pageObject.ForEach(obj => obj.SetActive(false));
        pageObject.Clear();
        int tmp = nowPage * 8;
        for (int next = nowPage * 8; next < (tmp + 8); next++)
        {
            if (allCardInfList.allList.Count > next)
            {
                List<GameObject> objects = GetAllChildrenOption();
                objects[next].SetActive(true);
                if (objects[next].GetComponent<ClickAdd>().amount == 3)
                {
                    objects[next].GetComponent<Card>().backColor.color = Color.black;
                }
                pageObject.Add(objects[next]);
            }
            else
            {
                nextButton.SetActive(false);
            }
        }
        preButton.SetActive(true);
    }

    public void PrePage()
    {
        nowPage--;
        pageObject.ForEach(obj => obj.SetActive(false));
        pageObject.Clear();
        int tmp = nowPage * 8;
        for (int pre = nowPage * 8; pre < (tmp + 8); pre++)
        {
            List<GameObject> objects = GetAllChildrenOption();
            objects[pre].SetActive(true);
            if (objects[pre].GetComponent<ClickAdd>().amount == 3)
            {
                objects[pre].GetComponent<Card>().backColor.color = Color.black;
            }
            pageObject.Add(objects[pre]);
        }
        nextButton.SetActive(true);
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
        }
    }
}
