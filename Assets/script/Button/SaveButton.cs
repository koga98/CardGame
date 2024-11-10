using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveButton : MonoBehaviour
{
    DeckMake deckMake;
    DekeMakeUIManager dekeMakeUIManager;
    public static DeckDatabase deckDatabase;
    public static DeckDatabaseCollection deckDatabaseCollection;
    string filePath;
    public int myButtonNumber;

    void Awake()
    {
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
                filePath = Application.persistentDataPath + "/" + "SaveData.json4";
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
        dekeMakeUIManager = manager.GetComponent<DekeMakeUIManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SaveButtonAction()
    {
        AudioManager.Instance.ButtonSound();
        deckMake.DeckMakeMethod(myButtonNumber);
        Save();
        dekeMakeUIManager.SaveButtonUIAction();
    }

    public void NextPageButton()
    {
        AudioManager.Instance.ButtonSound();
        deckMake.NextPage();
    }

    public void PrePageButton()
    {
        AudioManager.Instance.ButtonSound();
        deckMake.PrePage();
    }
    //デッキ選択に戻るボタン
    public void BackPreSceneButton()
    {
        dekeMakeUIManager.BackPreScene();
    }

    //いいえの場合
    public void BackToTitleButton()
    {
        AudioManager.Instance.ButtonSound();
        SceneManager.LoadScene("Title");
    }
    //戻る場合
    public void Back()
    {
        AudioManager.Instance.ButtonSound();
        SceneManager.LoadScene("ChoiceDeckNumber");
    }
    //保存して戻る場合
    public void BackAndSave()
    {
        AudioManager.Instance.ButtonSound();
        deckMake.DeckMakeMethod(myButtonNumber);
        Save();
        SceneManager.LoadScene("ChoiceDeckNumber");
    }
    //×ボタン
    public void closePanelButton()
    {
        AudioManager.Instance.ButtonSound();
        dekeMakeUIManager.closePanel();
    }

    public void OpenHowToDoIt()
    {
        AudioManager.Instance.ButtonSound();
        dekeMakeUIManager.OpenHowToDoItPanel();
    }

    public void CloseHowToDoIt()
    {
        AudioManager.Instance.ButtonSound();
        dekeMakeUIManager.CloseHowToDoItPanel();
    }

    private void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(deckDatabaseCollection);
            using (StreamWriter streamWriter = new StreamWriter(filePath))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
        }
        catch (IOException e)
        {
            Debug.LogError($"Failed to save data: {e.Message}");
        }
    }

    public void ContinueButton()
    {
        dekeMakeUIManager.Continue();
    }

    public void LoadButton()
    {
        if (File.Exists(filePath) && DeckMake.deckAmount == 40)
        {
            StreamReader streamReader = new StreamReader(filePath);
            string data = streamReader.ReadToEnd();
            streamReader.Close();
            deckDatabaseCollection = JsonUtility.FromJson<DeckDatabaseCollection>(data);

            if (deckDatabaseCollection != null && deckDatabaseCollection.cardDataLists.Count > 0)
            {
                CardManager.DeckInf = deckDatabaseCollection.cardDataLists[0].idLists;
                SceneManager.LoadScene("playGame");
            }
            else
                Debug.LogError("Failed to load deck data.");
        }
    }
}
