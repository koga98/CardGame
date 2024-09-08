using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject messagePanel;
    public GameObject ChoicePanel;
    public GameObject ChoiceCardPlace;
    public GameObject gameSetPanel;
    public GameObject checkPanel;
    public Text someTexts;
    public List<Text> cardDetailText;
    public Text gameSetText;
    [SerializeField] private Text phazeOperateButtonText;
    [SerializeField] private GameObject phazeOperateButton;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckPanelActive(){
        checkPanel.SetActive(true);
    }

    public void CheckPanelInActive(){
        checkPanel.SetActive(false);
    }

    public void GameSetPanelActive(){
        gameSetPanel.SetActive(true);
    }

    public void ChangeGameSetText(string text){
        gameSetText.text = text;
    }

    public void MessagePanelActive(){
        messagePanel.SetActive(true);
    }

    public void MessagePanelInActive(){
        messagePanel.SetActive(false);
    }

    public void ChoicePanelActive(){
        ChoicePanel.SetActive(true);
    }

    public void ChoicePanelInActive(){
        ChoicePanel.SetActive(false);
    }

    public void PhazeOperateButtonActive(){
        phazeOperateButton.SetActive(true);
    }

    public void PhazeOperateButtonInActive(){
        phazeOperateButton.SetActive(false);
    }

    public void ChangePhazeOperateButtonText(String text){
        phazeOperateButtonText.text = text;
    }
}
