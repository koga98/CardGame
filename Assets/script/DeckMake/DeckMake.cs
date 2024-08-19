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
    Card saveCard;
    string filePath;
    public List<Card> cardLists;
    public GameObject nextButton;
    public GameObject preButton;
    Dictionary<int, int> counts = new Dictionary<int, int>();
    private int nowPage = 0;
    [SerializeField] private AllCardInf allCardInfList;
    public GameObject detailPanel;
    public List<Text> DetailText;
    public GameObject confirmPanel;

    // Start is called before the first frame update
    void Start()
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
        switch (DeckChiceButton.passNumber)
        {
            case 1:
                filePath = Application.persistentDataPath + "/" + "SaveData.json1";
                break;
            case 2:
                filePath = Application.persistentDataPath + "/" + "SaveData.json2";
                break;
            case 3:
                filePath = Application.persistentDataPath + "/" + "SaveData.json3";
                break;
            case 4:
                filePath = Application.persistentDataPath + "/" + "SaveData.json1";
                break;
        }

        if (File.Exists(filePath))
        {
            
            StreamReader streamReader;
            streamReader = new StreamReader(filePath);
            string data = streamReader.ReadToEnd();
            streamReader.Close();
            DeckDatabaseCollection collection = JsonUtility.FromJson<DeckDatabaseCollection>(data);
            //ロードしたのが何番目のデータなのかを検知して
            GameManager.myDeckInf = collection.cardDataLists[0].idLists;
            deckAmount = collection.cardDataLists[0].idLists.Count;
            deckNumber.text = deckAmount.ToString() + "/40";

            foreach (int id in GameManager.myDeckInf)
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
                GameObject saveData = Instantiate(cardPrehfab, deckList, false);
                saveCard = saveData.GetComponent<Card>();
                saveCard.P1SetUp(allCardInfList.allList[entry.Key]);
                saveCard.GetComponent<ClickAdd>().myObject = pageObject[saveCard.inf.Id];
                pageObject[saveCard.inf.Id].GetComponent<ClickAdd>().copyObject = saveData;
                saveCard.GetComponent<ClickAdd>().copyObject = saveData;
                saveCard.GetComponent<ClickAdd>().amount = entry.Value;
                saveCard.reflectAmount(entry.Value);
                saveData.transform.localScale = Vector3.one;
            }
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (nowPage == 0)
        {
            preButton.SetActive(false);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (detailPanel.activeSelf)
            {
                detailPanel.SetActive(false);
            }

        }
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
    public void NextPage()
    {
        nowPage++;
        for (int field = pageObject.Count - 1; field >= 0; field--)
        {
            pageObject[field].SetActive(false);
        }
        pageObject.Clear();
        int tmp = nowPage * 8;
        for (int next = nowPage * 8; next < (tmp + 8); next++)
        {
            if (allCardInfList.allList.Count > next)
            {
                List<GameObject> objects = GetAllChildrenOption();
                objects[next].SetActive(true);
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
        for (int field = pageObject.Count - 1; field >= 0; field--)
        {
            pageObject[field].SetActive(false);
        }
        pageObject.Clear();
        int tmp = nowPage * 8;
        for (int pre = nowPage * 8; pre < (tmp + 8); pre++)
        {
            List<GameObject> objects = GetAllChildrenOption();
            objects[pre].SetActive(true);
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

    List<GameObject> SortChildren()
    {
        List<GameObject> children = new List<GameObject>();

        // 親オブジェクトのTransformコンポーネントを使用して子オブジェクトを取得
        foreach (Transform child in deckList)
        {
            children.Add(child.gameObject);
        }
        children.Sort((card1, card2) => card1.GetComponent<Card>().inf.Id.CompareTo(card2.GetComponent<Card>().inf.Id));
        return children;
    }
}
