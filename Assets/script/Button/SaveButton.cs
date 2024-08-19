using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveButton : MonoBehaviour
{
    DeckMake deckMake;
    public GameObject confirmPanel;
    public GameObject LoadButton;
    public static DeckDatabase  deckDatabase;
    public static DeckDatabaseCollection  deckDatabaseCollection;
    string filePath;
    public int myButtonNumber;

    void Awake()
    {
        switch(DeckChiceButton.passNumber){
            case 1:
            filePath=Application.persistentDataPath+"/" +"SaveData.json1";
            break;
            case 2:
            filePath=Application.persistentDataPath+"/" +"SaveData.json2";
            break;
            case 3:
            filePath=Application.persistentDataPath+"/" +"SaveData.json3";
            break;
            case 4:
            filePath=Application.persistentDataPath+"/" +"SaveData.json1";
            break;
        }
        deckDatabase = new DeckDatabase
        {
            idLists = new List<int>()
        };

        deckDatabaseCollection = new DeckDatabaseCollection();
    }
    
    void Start()
    {
        GameObject manager = GameObject.Find("GameObject");
        deckMake = manager.GetComponent<DeckMake>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ButtonAction(){
        AudioManager.Instance.ButtonSound();
        deckMake.DeckMakeMethod(myButtonNumber);
        Save();
        LoadButton.SetActive(true);
    }

    public void NextPageButton(){
        AudioManager.Instance.ButtonSound();
        deckMake.NextPage();
    }

    public void PrePageButton(){
        AudioManager.Instance.ButtonSound();
        deckMake.PrePage();
    }

    public void BackPrePage(){
        AudioManager.Instance.ButtonSound();
        confirmPanel.SetActive(true);
    }

    public void Back(){
        AudioManager.Instance.ButtonSound();
        SceneManager.LoadScene("ChoiceDeckNumber");
    }

    public void BackAndSave(){
        AudioManager.Instance.ButtonSound();
        deckMake.DeckMakeMethod(myButtonNumber);
        Save();
        SceneManager.LoadScene("ChoiceDeckNumber");
    }

    public void closePanel(){
        AudioManager.Instance.ButtonSound();
        if(confirmPanel.activeSelf){
            confirmPanel.SetActive(false);
        }
    }

    public void Save()
    {
        //textSaveDataの変数名とその内容をjsonファイルに合うようなstringに変更する
        string json=JsonUtility.ToJson(deckDatabaseCollection);
        //引数にアクセスするようなStremWriterをインスタンス化する
        StreamWriter streamWriter=new StreamWriter(filePath);
        //
        streamWriter.Write(json);
        streamWriter.Flush();
        streamWriter.Close();
    }

    public void Load(){
        if (File.Exists(filePath))
        {
            StreamReader streamReader;
            streamReader = new StreamReader(filePath);
            string data = streamReader.ReadToEnd();
            streamReader.Close();
            deckDatabaseCollection = JsonUtility.FromJson<DeckDatabaseCollection>(data);
            //ロードしたのが何番目のデータなのかを検知して
            GameManager.myDeckInf = deckDatabaseCollection.cardDataLists[0].idLists;
            GameManager.enemyDeckInf = deckDatabaseCollection.cardDataLists[0].idLists;
            SceneManager.LoadScene("playGame");
        }
        
    }
}
