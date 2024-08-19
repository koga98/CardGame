using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeckChiceButton : MonoBehaviour
{
    public int deckNumber;
    public static int passNumber;

    public void ChoiceDeckNumber(){
        AudioManager.Instance.ButtonSound();
        passNumber = deckNumber;
        SceneManager.LoadScene("makeDeck");
    }

    public void BackPrePage(){
        AudioManager.Instance.ButtonSound();
        if(SceneManager.GetActiveScene().name == "makeDeck"){
            SceneManager.LoadScene("ChoiceDeckNumber");
        }else if(SceneManager.GetActiveScene().name == "ChoiceDeckNumber"){
            SceneManager.LoadScene("Title");
        }
        
    }


}
