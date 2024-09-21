using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CardAnimation : MonoBehaviour, IPointerDownHandler
{
    public Animator animator;
    Card attackCard;
    CardDragAndDrop dragAndDrop;



    public void OnPointerDown(PointerEventData eventData)
    {
        GameObject clickedObject = GetCardObject(eventData.pointerCurrentRaycast.gameObject);
        attackCard = clickedObject.GetComponent<Card>();
        dragAndDrop = clickedObject.GetComponent<CardDragAndDrop>();
        if (SceneManager.GetActiveScene().name == "playGame")
        {
            bool isOnField = clickedObject.transform.parent == dragAndDrop.myAttackField.transform || clickedObject.transform.parent == dragAndDrop.myDefenceField.transform;
            if (isOnField && GameManager.turnStatus == GameManager.TurnStatus.OnAttack)
            {
                if (clickedObject.tag == "Card")
                {
                    if (!animator.GetBool("extendMy") && GameManager.attackObject == null && !attackCard.Attacked)
                    {
                        AttackPrepareAnim(clickedObject);
                    }
                    else if (animator.GetBool("extendMy"))
                    {
                        CancelAttackPrepareAnim();
                    }
                }
            }
        }
    }

    private void AttackPrepareAnim(GameObject cardObject)
    {
        animator.SetBool("extendMy", true);
        GameManager.attackObject = cardObject;
        attackCard.attackPre = true;
    }

    public void CancelAttackPrepareAnim()
    {
        if (GameManager.attackObject != null)
        {
            attackCard = GameManager.attackObject.GetComponent<Card>();
            animator.SetBool("extendMy", false);
            GameManager.attackObject = null;
            attackCard.attackPre = false;
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
}
