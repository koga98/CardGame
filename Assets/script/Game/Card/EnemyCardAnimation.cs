using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using GameNamespace;

public class EnemyCardAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Animator animator;
    public GameObject enemyHand;
    CardManager player2CardManager;
    Card card;

    void Start()
    {
        card = GetComponent<Card>();
        enemyHand = GameObject.Find("enemyHand ");
        player2CardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
    }

    void Update()
    {

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
        GameObject cardObject = GetCardObject(clickedObject);
        Card card = cardObject.GetComponent<Card>();

        // 攻撃可能かどうかの基本条件をチェック
        bool isEnemy = cardObject.tag == "Enemy" && cardObject.transform.parent != enemyHand.transform;
        bool isOnAttackTurn = GameManager.turnStatus == GameManager.TurnStatus.OnAttack;
        bool canAttackTarget = card.canAttackTarget;

        if (isEnemy && isOnAttackTurn && canAttackTarget)
        {
            // 防御カードの攻撃制限をチェック
            if (player2CardManager.CannotAttackMyDefenceCard.Count > 0 && card.inf.cardType == CardType.Defence)
            {
                return;
            }
            // プロテクト効果のあるカードがあるかチェック
            if (player2CardManager.CardsWithProtectEffectOnField != null && player2CardManager.CardsWithProtectEffectOnField.Count != 0)
            {
                bool found = false;
                foreach (Card protectedCard in player2CardManager.CardsWithProtectEffectOnField)
                {
                    if (protectedCard == card)
                    {
                        found = true;
                        break;
                    }
                }
                if(!found)
                return;
            }

            // 攻撃対象が既に攻撃していないかを確認
            if (!animator.GetBool("extendEnemy") && GameManager.attackObject != null)
            {
                Card attackingCard = GameManager.attackObject.GetComponent<Card>();
                if (!attackingCard.Attacked)
                {
                    animator.SetBool("extendEnemy", true);
                    GameManager.defenceObject = cardObject;
                }
            }
        }
    }

    public GameObject GetCardObject(GameObject clickedGameObject)
    {
        Transform current = clickedGameObject.transform;
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

    public void OnPointerExit(PointerEventData eventData)
    {
        if (animator.GetBool("extendEnemy"))
        {
            animator.SetBool("extendEnemy", false);
            GameManager.defenceObject = null;
        }
    }
}
