using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public bool IsPlayerTurn { get; private set; }
    [SerializeField]
    private GameManager gameManager;
    [SerializeField]
    private UIManager uIManager;
    [SerializeField]
    private TutorialManager tutorialManager;
    [SerializeField]
    private ManaManager manaManager;
    

    void Start()
    {
       
    }

    public void TutorialTurnOrder()
    {
        IsPlayerTurn = true;
        string turnOrder = IsPlayerTurn ? "あなたは先攻" : "あなたは後攻";
        uIManager.messagePanel.SetActive(true);
        uIManager.ChangeMessageTexts(turnOrder);
        StartCoroutine(tutorialManager.ExplainTurn());
    }

    public void DecideTurnOrder()
    {
        System.Random random = new System.Random();
        bool isP1First = random.Next(2) == 0;
        IsPlayerTurn = isP1First;
        manaManager.ManaArrangeOnFirstSecond(isP1First);
        string turnOrder = IsPlayerTurn ? "あなたは先攻" : "あなたは後攻";
        uIManager.messagePanel.SetActive(true);
        uIManager.ChangeMessageTexts(turnOrder);
        StartCoroutine(HideMessagePanelAfterDelay(2.0f));
    }
    private IEnumerator HideMessagePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        uIManager.messagePanel.SetActive(false);
    }

    public void ToggleTurn() => IsPlayerTurn = !IsPlayerTurn;
}
