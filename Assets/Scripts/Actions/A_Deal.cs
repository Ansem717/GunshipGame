//---------------------------------------------------------
// file:	A_Deal.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	Action provided to sample deal from the deck to 4 hands of players.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class A_Deal : ActionInterface {

    private Deck deck;
    private int zoneID;
    private int dealCount;

    private List<Hand> hands;
    private Dictionary<Hand, bool> completedHands;

    private float delayPerCard = 0.3f;
    private float initialDelay;

    public A_Deal(Deck deck, int dealCount = 10, float initialDelay = 0.6f) : base(_delay: initialDelay) {
        name = "Deal";
        this.deck = deck;
        this.dealCount = dealCount;
        this.initialDelay = initialDelay;
        zoneID = 0;
        completedHands = new();
        hands = Object.FindObjectsByType<Hand>(FindObjectsSortMode.None).ToList();
    }

    public override bool Init() {
        zoneID = 0;
        return true;
    }

    public override void IUpdate(float dt) {
        if (completedHands.Count == hands.Count) {
            State = ActionState.Done;
            return;
        }

        Hand currentHand = hands[zoneID];

        if (currentHand.Count() < dealCount) {
            if (Helpers.DrawCard(deck, currentHand) == null) {
                State = ActionState.Done;
                return;
            }
        }

        if (currentHand.Count() >= dealCount) {
            completedHands[currentHand] = true;
        }

        zoneID++;
        if (zoneID >= hands.Count) zoneID = 0;

        delay = delayPerCard;
        State = ActionState.Waiting; //nice, this is a clean way to reset delay after each trigger
    }

    public override void Exit() {    }

    public override float GetEstimatedDuration() {
        int total = dealCount * hands.Count; //Total number of cards to be dealt
        return initialDelay                  // initial delay
               + delayPerCard * (total - 1)  // delay per card except last card
               + 0.5f * total                //movement per card, including last card
        ;
    }
}
