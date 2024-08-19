using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

[CreateAssetMenu(fileName = "New Monitaring ProtectShield", menuName = "Condition/MonitaringProtectShield")]
public class MonitaringProtectShield : ConditionEffectsInf
{
    public int threshold;
    public bool ApplyToMyself;
    private Leader leader;
    GameObject manager;
    GameManager gameManager;
    public override bool ApplyEffect(ApplyEffectEventArgs e)
    {
        manager = GameObject.Find("GameManager");
        gameManager = manager.GetComponent<GameManager>();
        if(e.Card.CardOwner == PlayerID.Player1){
            if(ApplyToMyself){
                leader = gameManager.myLeader.GetComponent<Leader>();
                return conditionMethod.ProtectShiledIsThresholdValue(threshold,leader);
            }else{
                leader = gameManager.enemyLeader.GetComponent<Leader>();
            return conditionMethod.ProtectShiledIsThresholdValue(threshold,leader);
            }
            
        }else{
            if(ApplyToMyself){
                leader = gameManager.enemyLeader.GetComponent<Leader>();
                return conditionMethod.ProtectShiledIsThresholdValue(threshold,leader);
            }else{
                leader = gameManager.myLeader.GetComponent<Leader>();
                return conditionMethod.ProtectShiledIsThresholdValue(threshold,leader);
            }
        }
        
    }
}
