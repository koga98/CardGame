using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameManager;

public class TutorialManager : MonoBehaviour
{
    public UIManager uIManager;
    public GameManager gameManager;
    public bool buttonComplete = false;
    public bool attackComplete = false;
    [SerializeField]
    private CardDragAndDrop targetCard;

    [SerializeField]
    private CardManager p1CardManager;
    [SerializeField]
    private ManaManager manaManager;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }



    private IEnumerator WaitNextTutorialMessage()
    {
        bool skipTutorialMessage = false;
        float seconds = 0;
        while (!skipTutorialMessage && seconds <= 6)
        {
            seconds += Time.deltaTime;
            if (Input.GetMouseButtonDown(0))
            {
                skipTutorialMessage = true;
            }
            yield return null; // 毎フレーム一時停止
        }
    }

    private IEnumerator WaitNextTutorialMessageWhenNeedCardDrag()
    {
        bool skipTutorialMessage = false;
        float seconds = 0;
        while (!skipTutorialMessage && seconds <= 6 || targetCard.canDrag)
        {
            seconds += Time.deltaTime;
            if (Input.GetMouseButtonDown(0))
            {
                skipTutorialMessage = true;
            }
            if(skipTutorialMessage || seconds > 6){
                uIManager.tutorialPanel.SetActive(false);
            }
            yield return null; // 毎フレーム一時停止
        }
    }

    private IEnumerator WaitNextTutorialMessageWhenNeedButton()
    {
        bool skipTutorialMessage = false;
        float seconds = 0;
        while (!skipTutorialMessage && seconds <= 6 || !buttonComplete)
        {
            seconds += Time.deltaTime;
            if (Input.GetMouseButtonDown(0))
            {
                skipTutorialMessage = true;
            }
            if(skipTutorialMessage || seconds > 6){
                uIManager.tutorialPanel.SetActive(false);
            }
            
            yield return null; // 毎フレーム一時停止
        }
        uIManager.phazeOperateButton.SetActive(false);
    }

    private IEnumerator WaitNextTutorialMessageWhenNeedAttack()
    {
        bool skipTutorialMessage = false;
        float seconds = 0;
        while (!skipTutorialMessage && seconds <= 6 || !attackComplete)
        {
            seconds += Time.deltaTime;
            if (Input.GetMouseButtonDown(0))
            {
                skipTutorialMessage = true;
            }
            if(skipTutorialMessage || seconds > 6){
                uIManager.tutorialPanel.SetActive(false);
            }
            
            yield return null; // 毎フレーム一時停止
        }
        uIManager.phazeOperateButton.SetActive(false);
    }

    public IEnumerator ExplainTurn()
    {
        Vector3 position = new Vector3(206, 155, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        string text = "バトル開始時に、先攻後攻がランダムで決まります。今回はあなたが先行です。";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        uIManager.messagePanel.SetActive(false);
        StartCoroutine(ExplainDraw());
    }

    private IEnumerator ExplainDraw()
    {
        Vector3 position = new Vector3(608, -221, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        p1CardManager.drawCard();
        string text = "ターン開始時、カードを一枚デッキからドローします";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        uIManager.messagePanel.SetActive(false);
        StartCoroutine(ExplainManaRecover());
    }

    private IEnumerator ExplainManaRecover()
    {
        Vector3 position = new Vector3(-556, -162, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        TurnStartPhaze();
        string text = "また、マナの最大値が100増え、マナが全回復します";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        uIManager.messagePanel.SetActive(false);
        StartCoroutine(ExplainManaArrange());
    }

    private IEnumerator ExplainManaArrange()
    {
        Vector3 position = new Vector3(-556, -162, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        manaManager.P1MaxMana = 900;
        manaManager.P1_mana = 900;
        string text = "今回はチュートリアルなので、マナが900ある状態にします。";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        uIManager.messagePanel.SetActive(false);
        StartCoroutine(ExplainDeckAndHandInf());
    }

    private IEnumerator ExplainAttackReflesh()
    {
        Vector3 position = new Vector3(608, -221, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        TurnStartPhaze();
        string text = "また、攻撃済みのカードがある場合、再び攻撃できるようになります。";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        uIManager.messagePanel.SetActive(false);
        StartCoroutine(ExplainDeckAndHandInf());
    }

    private IEnumerator ExplainDeckAndHandInf()
    {
        Vector3 position = new Vector3(-581, -267, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        string text = "上から、デッキの枚数、手札の枚数を表示しています";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        StartCoroutine(ExplainP1_Mana());
    }

    private IEnumerator ExplainP1_Mana()
    {
        Vector3 position = new Vector3(-556, -162, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        string text = "左側が現在のマナ数、右側が最大マナ数を表します。カードをプレイすると、カードの左上に記載されているマナ数分消費します";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        StartCoroutine(ExplainMyLeader());
    }

    private IEnumerator ExplainMyLeader()
    {
        Vector3 position = new Vector3(-511, 155, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        string text = "自リーダーです。上がシールド、下がHPです。攻撃を受けるとき、シールドから減っていきます。シールドが0になってからHPは減ります";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        StartCoroutine(ExplainEnemyLeader());
    }

    private IEnumerator ExplainEnemyLeader()
    {
        Vector3 position = new Vector3(394, 418, 0);
        Vector3 rotation = new Vector3(0, 0, 0);
        string text = "それぞれ敵の情報を表示しています";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        StartCoroutine(ExplainmyHand());
    }

    private IEnumerator ExplainmyHand()
    {
        Vector3 position = new Vector3(241, -246, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        string text = "手札です。8枚まで所持できます。それを超えると手札に加わらず、デッキからも消えます";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        StartCoroutine(ExplainCardPropaties());
    }

    private IEnumerator ExplainCardPropaties()
    {
        Vector3 position = new Vector3(241, -246, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        string text = "カードには、前衛、後衛、スペルと三種類存在します。スペルは使い切りとなり、フィールドに出ることはありません";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        StartCoroutine(ExplainCardInf());
    }

    private IEnumerator ExplainCardInf()
    {
        Vector3 position = new Vector3(241, -246, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        string text = "左上が消費マナ、左下が攻撃力、右下がHPを表します";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        StartCoroutine(ExplainCardCanPlay());
    }

    private IEnumerator ExplainCardCanPlay()
    {
        Vector3 position = new Vector3(122, -250, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        turnStatus = TurnStatus.OnPlay;
        string text = "プレイ可能なカードは縁が緑になります。";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        StartCoroutine(ExplainCardVanguard());
    }

    private IEnumerator ExplainCardVanguard()
    {
        Vector3 position = new Vector3(122, -250, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        turnStatus = TurnStatus.OnPlay;
        ArrangeCandrag(1);
        string text = "まずは、左から二番目の前衛カードを使用してみましょう";
        yield return StartCoroutine(CompleteCardDrag(position, rotation, text));
        StartCoroutine(ExplainmyAttackField());
    }

    private IEnumerator ExplainmyAttackField()
    {
        Vector3 position = new Vector3(216, 122, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        
        string text = "前衛です。前衛カードをフィールドに出したとき、ここに配置されます";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        StartCoroutine(ExplainCardRearguard());
    }

    private IEnumerator ExplainCardRearguard()
    {
        Vector3 position = new Vector3(122, -250, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        ArrangeCandrag(3);
        string text = "次に、右から二番目の前衛カードを使用してみましょう";
        yield return StartCoroutine(CompleteCardDrag(position, rotation, text));
        StartCoroutine(ExplainmyDefenceField());
    }

    private IEnumerator ExplainmyDefenceField()
    {
        Vector3 position = new Vector3(224, -52, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        string text = "後衛です。後衛カードをフィールドに出したとき、ここに配置されます";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        StartCoroutine(ExplainCardSpel());
    }

    private IEnumerator ExplainCardSpel()
    {
        Vector3 position = new Vector3(122, -250, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        ArrangeCandrag(0);
        string text = "次に、一番左のスペルカードを使用してみましょう";
        yield return StartCoroutine(CompleteCardDrag(position, rotation, text));
        StartCoroutine(ExplainCardSpelAfterPlay());
    }

    private IEnumerator ExplainCardSpelAfterPlay()
    {
        Vector3 position = new Vector3(592, -171, 0);
        Vector3 rotation = new Vector3(0, 0, 0);
        string text = "スペルカードは使用後に消滅します";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        StartCoroutine(ExplainButton());
    }

    private IEnumerator ExplainButton()
    {
        Vector3 position = new Vector3(600, 6, 0);
        Vector3 rotation = new Vector3(0, 0, 0);
        uIManager.phazeOperateButton.SetActive(true);
        uIManager.ChangePhazeOperateButtonText("攻撃フェーズへ");
        buttonComplete = false;
        string text = "ボタンを押すと、攻撃フェーズに移ります";
        yield return StartCoroutine(CompleteButton(position, rotation, text));
        StartCoroutine(ExplainCannotAttack());
    }

    private IEnumerator ExplainCannotAttack()
    {
        Vector3 position = new Vector3(217, 124, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        string text = "出したカードは基本、そのターンに攻撃できません";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        StartCoroutine(ExplainAttack1());
    }

    private IEnumerator ExplainAttack1()
    {
        Vector3 position = new Vector3(217, 124, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        string text = "自カードをクリックし、攻撃対象を選択することで攻撃できます";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        StartCoroutine(ExplainAttack2());
    }

    private IEnumerator ExplainAttack2()
    {
        Vector3 position = new Vector3(217, 124, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        string text = "クリックするとカードが拡大します。拡大状態で相手のリーダーを選択しましょう";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        StartCoroutine(ExplainAttackCancel());
    }

    private IEnumerator ExplainAttackCancel()
    {
        Vector3 position = new Vector3(217, 124, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        string text = "なお、キャンセルしたい場合は拡大しているカードを選択しましょう。すると、元に戻り、キャンセルされた状態になります";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        StartCoroutine(ExplainAttackActive());
    }

    private IEnumerator ExplainAttackActive()
    {
        Vector3 position = new Vector3(217, 124, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        attackComplete = false;
        string text = "実際にやってみましょう";
        AttackActive();
        yield return StartCoroutine(CompleteAttack(position, rotation, text));
        StartCoroutine(ExplainEndButton());
    }

    private IEnumerator ExplainEndButton()
    {
        Vector3 position = new Vector3(600, 6, 0);
        Vector3 rotation = new Vector3(0, 0, 0);
        AttackInActive();
        buttonComplete = false;
        uIManager.phazeOperateButton.SetActive(true);
        uIManager.ChangePhazeOperateButtonText("ターンエンド");
        string text = "攻撃が終了したらボタンを押し、ターンエンドしましょう";
        yield return StartCoroutine(CompleteButton(position, rotation, text));
        StartCoroutine(ExplainFinish());
    }

    private IEnumerator ExplainFinish()
    {
        Vector3 position = new Vector3(217, 124, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        string text = "お疲れさまでした。あとは実戦で慣れましょう";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
        SceneManager.LoadScene("Title");
    }

    private IEnumerator ExplainChoicePanel()
    {
        Vector3 position = new Vector3(-557, 397, 0);
        Vector3 rotation = new Vector3(0, 180, 0);
        string text = "相手のカードを選択する効果を持つカードは一度ここに配置されます。キャンセルボタンで、キャンセルができ、対象外のカードを選択するとキャンセルされます";
        yield return StartCoroutine(PanelOperation(position, rotation, text));
    }

    private IEnumerator PanelOperation(Vector3 position, Vector3 rotation, string text)
    {
        uIManager.ChangeTutorialExplainer(position, rotation, text);
        yield return StartCoroutine(WaitNextTutorialMessage());
        uIManager.tutorialPanel.SetActive(false);
    }

    private IEnumerator CompleteCardDrag(Vector3 position, Vector3 rotation, string text)
    {
        uIManager.ChangeTutorialExplainer(position, rotation, text);
        yield return StartCoroutine(WaitNextTutorialMessageWhenNeedCardDrag());
    }

    private IEnumerator CompleteButton(Vector3 position, Vector3 rotation, string text)
    {
        uIManager.ChangeTutorialExplainer(position, rotation, text);
        yield return StartCoroutine(WaitNextTutorialMessageWhenNeedButton());
    }

    private IEnumerator CompleteAttack(Vector3 position, Vector3 rotation, string text)
    {
        uIManager.ChangeTutorialExplainer(position, rotation, text);
        yield return StartCoroutine(WaitNextTutorialMessageWhenNeedAttack());
    }

    private void TurnStartPhaze()
    {
        turnStatus = TurnStatus.OnTurnStart;
        gameManager.p1_turnElapsed++;
        manaManager.P1TurnStart();
    }

    private void AttackActive(){
        foreach (Card card in p1CardManager.AllFields)
                for (int c = 0; c < card.attackedList.Count; c++)
                    card.attackedList[c] = false;
                
        foreach (Card card in p1CardManager.AllFields)
            card.ActivePanel.SetActive(!card.Attacked); 
    }

    private void AttackInActive(){
        foreach (Card card in p1CardManager.AllFields)
                for (int c = 0; c < card.attackedList.Count; c++)
                    card.attackedList[c] = true;
                
        foreach (Card card in p1CardManager.AllFields)
            card.ActivePanel.SetActive(!card.Attacked); 
    }

    private void ArrangeCandrag(int index){
        foreach(Card card in p1CardManager.Hands){
            card.gameObject.GetComponent<CardDragAndDrop>().canDrag = false;
            card.ActivePanel.SetActive(false);
        }
        targetCard = p1CardManager.Hands[index].gameObject.GetComponent<CardDragAndDrop>();
        targetCard.canDrag = true;
        p1CardManager.Hands[index].ActivePanel.SetActive(true);
    }

    public void ArrangeCandrag(){
        foreach(Card card in p1CardManager.Hands){
            card.gameObject.GetComponent<CardDragAndDrop>().canDrag = false;
            card.ActivePanel.SetActive(false);
        }
    }
}
