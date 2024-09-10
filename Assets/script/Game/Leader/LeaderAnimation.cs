using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class leaderAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Animator animator;
    CardManager player2CardManager;

    Card card;

    void Start()
    {
        card = GetComponent<Card>();
        player2CardManager = GameObject.Find("P2CardManager").GetComponent<CardManager>();
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
            if (player2CardManager.CardsWithProtectEffectOnField != null && player2CardManager.CardsWithProtectEffectOnField.Count != 0)
                return;

            if (!animator.GetBool("extendLeader") && GameManager.attackObject != null)
            {
                animator.SetBool("extendLeader", true);
                GameManager.defenceObject = cradObject;
            }
        }
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
