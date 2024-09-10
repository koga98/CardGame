using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
public class UtilMethod
{
    public async Task DamageMethod(Card card, int damageAmount)
    {
        // ダメージを与える
        card.hp -= damageAmount;
        card.healthText.text = card.hp.ToString();

        // HPが0以下ならカードを破壊
        if (card.hp <= 0)
        {
            await card.DestoryThis(); // コルーチンからタスクに変更
        }
    }
}