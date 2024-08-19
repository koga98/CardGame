using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

public class ApplyEffectEventArgs : EventArgs
{
    public Card Card { get; private set; }
    public  ObservableCollection<Card> PCards { get; private set; }
    public List<Card> PAttackCards { get; private set; }
    public List<Card> PDefenceCards { get; private set; }
    public ObservableCollection<Card> Cards { get; private set; }
    public List<Card> EAttackCards { get; private set; }
    public List<Card> EDefenceCards { get; private set; }
    public Card ChoiceCard;

    public ApplyEffectEventArgs(Card card, ObservableCollection<Card> cards,List<Card> eAttackCards ,List<Card> eDefenceCards,
                                 ObservableCollection<Card> pCards,List<Card> pAttackCards,List<Card> pDefenceCards,Card choiceCard = null)
    {
        Card = card;
        Cards = cards;
        EAttackCards =  eAttackCards;
        EDefenceCards = eDefenceCards;
        PCards = pCards;
        PAttackCards = pAttackCards;
        PDefenceCards = pDefenceCards;
        ChoiceCard = choiceCard;
    }
}

