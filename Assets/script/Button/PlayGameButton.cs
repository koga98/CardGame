using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayGameButton : MonoBehaviour
{
    public GameManager gameManager;
    public UIManager uIManager;
    public void topButtonMethod()
    {
        AudioManager.Instance.ButtonSound();
        GameManager.completeButtonChoice = true;
    }

    public async void nextPlay()
    {
        if(gameManager.isGameOver)
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

    public void SettingPanelActive(){
        AudioManager.Instance.ButtonSound();
        uIManager.SettingPanel.SetActive(true);
    }
    public void SettingPanelInActive(){
        AudioManager.Instance.ButtonSound();
        uIManager.SettingPanel.SetActive(false);
    }

    public void SoundSliderPanelActive(){
        AudioManager.Instance.ButtonSound();
        uIManager.SoundSliderPanel.SetActive(true);
    }

    public void SoundSliderPanelInActive(){
        AudioManager.Instance.ButtonSound();
        uIManager.SoundSliderPanel.SetActive(false);
    }

    public void Retry()
    {
        AudioManager.Instance.ButtonSound();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Retire(){
        AudioManager.Instance.ButtonSound();
        uIManager.SettingPanel.SetActive(false);
        gameManager.myLeader.GetComponent<Leader>().Hp = 0;
    }

    public void ReturnTitle(){
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

    public void CancelChoice(){
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

}
