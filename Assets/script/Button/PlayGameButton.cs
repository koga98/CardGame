using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayGameButton : MonoBehaviour
{
    public GameManager gameManager;
    public UIManager uIManager;
    public Slider bgmSlider;
    public Slider SeSlider;
    public Slider voiceSlider;
    string filePath;
    public void topButtonMethod()
    {
        AudioManager.Instance.ButtonSound();
        GameManager.completeButtonChoice = true;
    }

    public async void nextPlay()
    {
        if (gameManager.isGameOver)
            return;
        AudioManager.Instance.TurnEndButtonSound();
        if (GameManager.turnStatus == GameManager.TurnStatus.OnPlay)
        {
            uIManager.phazeOperateButton.SetActive(false);
            await WaitUntilFalse(() => gameManager.isDealing);
            gameManager.AttackPhaze();
            uIManager.phazeOperateButton.SetActive(true);
        }
        else if (GameManager.turnStatus == GameManager.TurnStatus.OnAttack)
        {
            uIManager.phazeOperateButton.SetActive(false);
            await WaitUntilFalse(() => gameManager.isDealing);
            await gameManager.TurnEndPhaze();
        }
    }

    public void underButtonMethod()
    {
        AudioManager.Instance.ButtonSound();
        CardManager cardManager = GameObject.Find("P1CardManager").GetComponent<CardManager>();
        CardManager.DeckInf.Add(CardManager.DeckInf[cardManager.DeckIndex]);
        CardManager.DeckInf.RemoveAt(cardManager.DeckIndex);
        GameManager.completeButtonChoice = true;
    }

    public void P2underButtonMethod()
    {
        AudioManager.Instance.ButtonSound();
        CardManager cardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
        CardManager.enemyDeckInf.Add(CardManager.enemyDeckInf[cardManager.DeckIndex]);
        CardManager.enemyDeckInf.RemoveAt(cardManager.DeckIndex);
        GameManager.completeButtonChoice = true;
    }

    public void SettingPanelActive()
    {
        AudioManager.Instance.ButtonSound();
        uIManager.SettingPanel.SetActive(true);
    }
    public void SettingPanelInActive()
    {
        AudioManager.Instance.ButtonSound();
        
        uIManager.SettingPanel.SetActive(false);
    }

    public void SoundSliderPanelActive()
    {
        AudioManager.Instance.ButtonSound();
        LoadButton();
        uIManager.SoundSliderPanel.SetActive(true);
    }

    public void SoundSliderPanelInActive()
    {
        AudioManager.Instance.ButtonSound();
        Save();
        uIManager.SoundSliderPanel.SetActive(false);
    }

    public void Retry()
    {
        AudioManager.Instance.ButtonSound();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Retire()
    {
        AudioManager.Instance.ButtonSound();
        uIManager.SettingPanel.SetActive(false);
        gameManager.myLeader.GetComponent<Leader>().Hp = 0;
    }

    public void ReturnTitle()
    {
        AudioManager.Instance.ButtonSound();
        SceneManager.LoadScene("Title");
    }

    public void EndGame()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif

    }

    public GameObject GetCardObject(GameObject buttonGameObject)
    {
        Transform current = buttonGameObject.transform;
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

    public void CancelChoice()
    {
        GameObject manager = GameObject.Find("P1CardManager");
        CardManager cardManager = manager.GetComponent<CardManager>();
        CardDragAndDrop.OnCoroutine = false;
        cardManager.choiceCard.GetComponent<CardDragAndDrop>().completeChoice = true;
        cardManager.choiceCard.GetComponent<CardDragAndDrop>().cancelChoice = true;
    }

    public async Task WaitUntilFalse(Func<bool> condition)
    {
        while (condition())
        {
            await Task.Yield();
        }
    }

    private void Save()
    {
        filePath = Application.persistentDataPath + "/"  + "SoundData.json";
        TitleGamemanager.soundDataBase.soundSetting.Clear();
        TitleGamemanager.soundDataBase.soundSetting.Add(bgmSlider.value);
        TitleGamemanager.soundDataBase.soundSetting.Add(SeSlider.value);
        TitleGamemanager.soundDataBase.soundSetting.Add(voiceSlider.value);
        try
        {
            string json = JsonUtility.ToJson(TitleGamemanager.soundDataBase);
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

    public void LoadButton()
    {
        filePath = Application.persistentDataPath + "/"  + "SoundData.json";
        if (File.Exists(filePath))
        {
            StreamReader streamReader = new StreamReader(filePath);
            string data = streamReader.ReadToEnd();
            streamReader.Close();
            TitleGamemanager.soundDataBase = JsonUtility.FromJson<SoundDataBase>(data);

            if (TitleGamemanager.soundDataBase != null && TitleGamemanager.soundDataBase.soundSetting.Count > 0)
            {
                bgmSlider.value = TitleGamemanager.soundDataBase.soundSetting[0];
                SeSlider.value = TitleGamemanager.soundDataBase.soundSetting[1];
                voiceSlider.value = TitleGamemanager.soundDataBase.soundSetting[2];
                AudioManager.Instance.audioSource.volume = bgmSlider.value;
                AudioManager.Instance.SEaudioSource.volume = SeSlider.value;
                AudioManager.Instance.voiceSource.volume = voiceSlider.value;
            }
            else
                Debug.LogError("Failed to load deck data.");
        }
    }

}
