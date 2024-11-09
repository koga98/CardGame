using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameNamespace;
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

    public bool JudgeActiveCard(Card card, int playerMana,CardManager cardManager){
        bool isPlayable;
        if (card.cost > playerMana)
        {
            isPlayable = false;
            return isPlayable;
        }
        else
        {
            isPlayable = true;
        }

        if (cardManager.AttackFields.Count >= 7 && card.inf.cardType == CardType.Attack 
        || cardManager.DefenceFields.Count >= 7 && card.inf.cardType == CardType.Defence)
        {
            isPlayable = false;
            return isPlayable;
        }
        else
        {
            isPlayable = true;
        }
        // P1CannotPlayDefenceCardが存在し、カードが防御タイプの場合、ドラッグ不可に設定
        if (cardManager.CannotPlayDefenceCard.Count != 0 && card.inf.cardType == CardType.Defence)
        {
            isPlayable = false;
            return isPlayable;
        }
        return isPlayable;
    }
}
