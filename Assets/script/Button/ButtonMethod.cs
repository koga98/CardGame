using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonMethod : MonoBehaviour
{
    public void topButtonMethod()
    {
        AudioManager.Instance.ButtonSound();
        GameManager.completeButtonChoice = true;
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
        CardManager cardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
        CardManager.enemyDeckInf.Add(CardManager.enemyDeckInf[cardManager.DeckIndex]);
        CardManager.enemyDeckInf.RemoveAt(cardManager.DeckIndex);
        GameManager.completeButtonChoice = true;
    }

    public void Retry()
    {
        AudioManager.Instance.ButtonSound();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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


}
