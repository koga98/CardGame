using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class leaderAnimation : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public Animator animator;

    Card card;

    void Start()
    {
        card = GetComponent<Card>();
    }

    void Update()
    {

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.tag == "enemyLeader")
        {
            GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
            GameObject cradObject = GetCardObject(clickedObject);
            if (!animator.GetBool("extendLeader") && GameManager.attackObject != null)
            {
                animator.SetBool("extendLeader", true);
                Debug.Log(animator.GetBool("extendLeader"));
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
            if (current.gameObject.GetComponent<Leader>() != null)
            {
                return current.gameObject;
            }
            current = current.parent;
        }
        return null;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (animator.GetBool("extendLeader"))
        {
            animator.SetBool("extendLeader", false);
            GameManager.defenceObject = null;
        }
    }
}
