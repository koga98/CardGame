using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject detailPanel;
    public GameObject messagePanel;
    public GameObject ChoicePanel;
    public GameObject ChoiceCardPlace;
    public GameObject gameSetPanel;
    public GameObject checkPanel;
    public Text messageTexts;
    public List<Text> cardDetailText;
    public Text gameSetText;
    public Text phazeOperateButtonText;
    public GameObject phazeOperateButton;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DetailPanelInactive()
    {
        if (detailPanel.activeSelf)
        {
            detailPanel.SetActive(false);
        }
    }

    public void DetailPanelActive(CardInf inf)
    {
        detailPanel.SetActive(true);
        cardDetailText[0].text = inf.cardName;
        cardDetailText[1].text = inf.cost.ToString();
        cardDetailText[2].text = inf.attack.ToString();
        cardDetailText[3].text = inf.hp.ToString();
        cardDetailText[4].text = inf.longText;
        cardDetailText[5].text = inf.cardType.ToString();
    }

    public void ChangeMessageTexts(string text)
    {
        messageTexts.text = text;
    }

    public void ChangeGameSetText(string text)
    {
        gameSetText.text = text;
    }

    public void ChangePhazeOperateButtonText(string text)
    {
        phazeOperateButtonText.text = text;
    }
}