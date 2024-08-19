using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using GameNamespace;

public class EnemyCardAnimation : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public Animator animator;
    public GameObject enemyHand;
    Card card;

    void Start()
    {
        card = GetComponent<Card>();
        enemyHand = GameObject.Find("enemyHand ");
    }

    void Update()
    {

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
        GameObject cradObject = GetCardObject(clickedObject);
        Card card = cradObject.GetComponent<Card>();
        if (cradObject.tag == "Enemy" && cradObject.transform.parent != enemyHand.transform &&
         GameManager.turnStatus == GameManager.TurnStatus.OnAttack && card.canAttackTarget)
        {
            if (CardManager.P2CannotAttackMyDefenceCard.Count != 0 && card.inf.cardType == CardType.Defence)
            {
                return;
            }
            if (CardManager.P2CardsWithProtectEffectOnField != null)
            {
                foreach (Card target in CardManager.P2CardsWithProtectEffectOnField)
                {
                    if (target != card)
                    {
                        return;
                    }
                }

            }
            if (!animator.GetBool("extendEnemy") && GameManager.attackObject != null)
            {
                if (GameManager.attackObject.GetComponent<Card>().Attacked)
                {
                    return;
                }
                animator.SetBool("extendEnemy", true);
                GameManager.defenceObject = cradObject;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }
    public void OnPointerUp(PointerEventData eventData)
    {


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
