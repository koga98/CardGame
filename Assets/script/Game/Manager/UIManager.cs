using System;
using System.Collections;
using System.Collections.Generic;
using GameNamespace;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject HpPanel;
    public GameObject AttackPanel;
    public GameObject detailPanel;
    public GameObject messagePanel;
    public GameObject ChoicePanel;
    public GameObject ChoiceCardPlace;
    public GameObject gameSetPanel;
    public GameObject checkPanel;
    public GameObject SettingPanel;
    public GameObject SoundSliderPanel;
    public GameObject temporarySpelPanel;
    public Text messageTexts;
    public List<Text> cardDetailText;
    public Text gameSetText;
    public List<Text> deckNumber;
    public List<Text> handNumber;
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
        cardDetailText[5].text = (inf.cardType == CardType.Defence) ? "後衛": (inf.cardType == CardType.Attack) ? "前衛":"スペル";
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

    public void ChangeDeckNumber(int playerNumber,int restDeck){
        string Deck = "    ×    " + restDeck;
        deckNumber[playerNumber].text = Deck;
    }

    public void  ChangeHandNumber(int playerNumber,int nowHand){
        string Hand = "    ×    " + nowHand;
        handNumber[playerNumber].text = Hand;
    }
}