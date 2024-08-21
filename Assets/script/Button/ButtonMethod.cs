using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonMethod : MonoBehaviour
{
    public void topButtonMethod()
    {
        AudioManager.Instance.ButtonSound();
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        GameManager.completeButtonChoice = true;
    }

    public void underButtonMethod()
    {
        AudioManager.Instance.ButtonSound();
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        GameManager.myDeckInf.Add(GameManager.myDeckInf[gameManager.myDeckIndex]);
        GameManager.myDeckInf.RemoveAt(gameManager.myDeckIndex);
        GameManager.completeButtonChoice = true;
    }

    public void P2underButtonMethod()
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager gameManager = manager.GetComponent<GameManager>();
        gameManager.enemyDeckInf.Add(gameManager.enemyDeckInf[gameManager.enemyDeckIndex]);
        gameManager.enemyDeckInf.RemoveAt(gameManager.enemyDeckIndex);
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


}
