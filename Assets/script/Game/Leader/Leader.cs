using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using GameNamespace;
using UnityEngine.EventSystems;
using System.Threading.Tasks;

public class Leader : MonoBehaviour, IPointerClickHandler
{
    private int hp = 30;
    private int protectShield = 2000;
    public double shieldDamageCutAmount = 0.0;
    public EffectManager effectManager;
    public EffectAnimationManager effectAnimationManager;
    public AttackManager attackManager;
    public AnimationClip shieldClip;
    public PlayerType playerType;
    public Sprite sprite;
    public Image leaderImage;
    public LeaderInf inf;
    public TextMeshProUGUI healthText;
    public Text protectShieldText;
    GameObject manager;
    GameManager gameManager;
    UIManager uIManager;

    public int Hp
    {
        get { return hp; }
        set
        {
            if (hp != value)
            {
                hp = value;
            }
            if (hp <= 0)
            {
                hp = 0;
                GameOver();
            }
        }
    }

    public int ProtectShield
    {
        get { return protectShield; }
        set
        {
            if (protectShield != value)
            {
                protectShield = value;
                if (protectShield < 0)
                {
                    protectShield = 0;
                }
                _ = UpdateShieldAsync();
            }

        }
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>

    void Start()
    {
        manager = GameObject.Find("GameManager");
        gameManager = manager.GetComponent<GameManager>();
        effectManager = manager.GetComponent<EffectManager>();
        effectAnimationManager = manager.GetComponent<EffectAnimationManager>();
        uIManager = manager.GetComponent<UIManager>();
        attackManager = manager.GetComponent<AttackManager>();
    }
    void Update()
    {

    }

    private async Task UpdateShieldAsync()
    {
        await effectAnimationManager.ShieldDamageAsync(this);
    }

    public void P1SetUp(LeaderInf leaderInf)
    {
        inf = leaderInf;
        healthText.text = leaderInf.hp.ToString();
        protectShieldText.text = leaderInf.protectShield.ToString();
        sprite = leaderInf.leaderSprite;
        leaderImage.sprite = sprite;
        Hp = leaderInf.hp;
        ProtectShield = leaderInf.protectShield;
        playerType = PlayerType.Player1;
    }
    public void P2SetUp(LeaderInf leaderInf)
    {
        inf = leaderInf;
        healthText.text = leaderInf.hp.ToString();
        protectShieldText.text = leaderInf.protectShield.ToString();
        sprite = leaderInf.leaderSprite;
        leaderImage.sprite = sprite;
        Hp = leaderInf.hp;
        ProtectShield = leaderInf.protectShield;
        playerType = PlayerType.Player2;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (GameManager.defenceObject != null)
            {
                StartCoroutine(attackManager.AttackLeaderCoroutine());
            }
        }
    }

    private void GameOver()
    {
        uIManager.gameSetPanel.SetActive(true);
        if (playerType == PlayerType.Player1)
        {
            uIManager.ChangeGameSetText("You Lose");
        }
        else if (playerType == PlayerType.Player2)
        {
            uIManager.ChangeGameSetText("You Win");
        }
    }

}
