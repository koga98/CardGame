using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DekeMakeUIManager : MonoBehaviour
{
    public GameObject SavePanel;
    public GameObject BackConfirmPanel;
    public GameObject BattleConfirmPanel;
    public GameObject ConfirmContinuePanel;
    public GameObject detailPanel;
    public List<Text> cardDetailText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
    //セーブボタン
    public void SaveButtonUIAction()
    {
        SavePanel.SetActive(false);
        if (DeckMake.deckAmount == 40)
        {
            BattleConfirmPanel.SetActive(true);
        }else{
            ConfirmContinuePanel.SetActive(true);
        }
    }

    public void BackPreScene()
    {
        AudioManager.Instance.ButtonSound();
        BackConfirmPanel.SetActive(true);
        SavePanel.SetActive(false);
    }

    public void closePanel()
    {
        AudioManager.Instance.ButtonSound();
        if (BackConfirmPanel.activeSelf)
        {
            BackConfirmPanel.SetActive(false);
            SavePanel.SetActive(true);
        }
    }

    public void Continue(){
        SavePanel.SetActive(true);
        ConfirmContinuePanel.SetActive(false);
    }

    
}
