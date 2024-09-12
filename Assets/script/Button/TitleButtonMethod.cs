using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleButtonMethod : MonoBehaviour
{
    public GameObject buttonPrefab;
    public GameObject nextAction;
    public GameObject buttonParent;
    public int buttonNumber;
    int buttonDealCoount;

    void Start()
    {
        buttonDealCoount = 0;
    }
    public void MakeDeckButton()
    {
        if (buttonDealCoount == 0)
        {
            if (nextAction != null)
            {
                nextAction.SetActive(true);
            }

            if (buttonParent != null)
            {
                buttonParent.SetActive(true);
            }

            for (int i = 0; i < 4; i++)
            {
                string filePath = Application.persistentDataPath + "/" + "SaveData.json" + i.ToString();
                if (File.Exists(filePath))
                {
                    try
                    {
                        using (StreamReader streamReader = new StreamReader(filePath))
                        {
                            string data = streamReader.ReadToEnd();

                            if (!string.IsNullOrEmpty(data))
                            {
                                DeckDatabaseCollection deckDatabaseCollection = JsonUtility.FromJson<DeckDatabaseCollection>(data);
                                if (deckDatabaseCollection.cardDataLists[0].idLists.Count == 40)
                                {
                                    GameObject newButton = Instantiate(buttonPrefab, buttonParent.transform);
                                    newButton.GetComponentInChildren<Text>().text = "デッキ" + i.ToString();
                                    newButton.GetComponent<TitleButtonMethod>().buttonNumber = i;
                                }
                            }
                            // ロードしたのが何番目のデータなのかを検知して

                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("データの読み込みに失敗しました: " + e.Message);
                    }
                }
            }
        }

        // Textコンポーネントの設定（親オブジェクトかその子オブジェクトにあるかを確認）
        Text nextActionText = nextAction.GetComponentInChildren<Text>();
        buttonDealCoount++;
        if (nextActionText != null)
        {
            if (buttonParent.transform.childCount == 0)
            {
                nextActionText.text = "作成したデッキがありません";
                StartCoroutine(WaitForClosePanel());
            }
            else
            {
                nextActionText.text = "デッキを選択";

            }
        }
        else
        {
            Debug.LogError("nextActionオブジェクトにTextコンポーネントが見つかりませんでした。");
        }
    }

    private IEnumerator WaitForClosePanel()
    {
        // 任意の遅延時間を設定
        yield return new WaitForSeconds(2f);
        // パネルを閉じる処理などを追加
        nextAction.SetActive(false);
        buttonParent.SetActive(false);
    }
    public void ScenChange()
    {
        string filePath = "";
        if (buttonNumber == 1)
        {
            filePath = Application.persistentDataPath + "/" + "SaveData.json1";
        }
        else if (buttonNumber == 2)
        {
            filePath = Application.persistentDataPath + "/" + "SaveData.json2";
        }
        else if (buttonNumber == 3)
        {
            filePath = Application.persistentDataPath + "/" + "SaveData.json3";
        }
        else if (buttonNumber == 4)
        {
            filePath = Application.persistentDataPath + "/" + "SaveData.json4";
        }
        if (File.Exists(filePath))
        {
            AudioManager.Instance.ButtonSound();
            StreamReader streamReader;
            streamReader = new StreamReader(filePath);
            string data = streamReader.ReadToEnd();
            streamReader.Close();
            DeckDatabaseCollection deckDatabaseCollection = JsonUtility.FromJson<DeckDatabaseCollection>(data);
            //ロードしたのが何番目のデータなのかを検知して
            CardManager.DeckInf = deckDatabaseCollection.cardDataLists[0].idLists;
            SceneManager.LoadScene("playGame");
        }
    }

    public void ChoiceDeckNumber()
    {
        AudioManager.Instance.ButtonSound();
        SceneManager.LoadScene("ChoiceDeckNumber");
    }
}
