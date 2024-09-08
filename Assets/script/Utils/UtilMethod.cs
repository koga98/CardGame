using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UtilMethod 
{
    public async Task DamageMethod(Card card,int damageAmount){
        card.hp -= damageAmount;
        card.healthText.text = card.hp.ToString();
        if(card.hp <= 0){
            await card.DestoryThis();
        }
    }

}
