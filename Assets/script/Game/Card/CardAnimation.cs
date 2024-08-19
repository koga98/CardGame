using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CardAnimation : MonoBehaviour, IPointerDownHandler
{
    public Animator animator;
    Card attackCard;

    public void OnPointerDown(PointerEventData eventData)
    {
        GameObject clickedObject = GetCardObject(eventData.pointerCurrentRaycast.gameObject);
        if (SceneManager.GetActiveScene().name == "playGame")
        {
            if (eventData.position.y > 222.24 && GameManager.turnStatus == GameManager.TurnStatus.OnAttack)
            {
                if (eventData.pointerCurrentRaycast.gameObject.tag == "Card")
                {
                    GameObject cradObject = GetCardObject(clickedObject);
                    attackCard = cradObject.GetComponent<Card>();
                    if (!animator.GetBool("extendMy") && GameManager.attackObject == null && !attackCard.Attacked)
                    {
                        animator.SetBool("extendMy", true);
                        GameManager.attackObject = cradObject;
                        attackCard.attackPre = true;
                    }
                    else if (animator.GetBool("extendMy"))
                    {
                        if (GameManager.attackObject != null)
                        {
                            attackCard = GameManager.attackObject.GetComponent<Card>();
                        }
                        animator.SetBool("extendMy", false);
                        GameManager.attackObject = null;
                        attackCard.attackPre = false;
                    }

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

    public void CannotOperate()
    {
        animator.SetBool("canOperate", false);
    }

    public void CanOperate()
    {
        animator.SetBool("canOperate", true);
    }
}
