//---------------------------------------------------------
// file:	A_TakeTrick.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	An action to take all cards in the play area and put them to the discard.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;

public class A_TakeTrick : ActionInterface {

    private Zone playArea;
    private Zone discardPile;

    private float internalDelay;
    private float internalElapsed;

    private bool taking;

    public A_TakeTrick() : base(_duration: 0.167f, _blocking: true) {
        //duration is treated as "duration per card"
        name = "TakeTrick";
        playArea = GameObject.Find("PlayArea").GetComponent<Zone>();
        discardPile = GameObject.Find("DiscardPile").GetComponent<Zone>();
    }

    public override bool Init() {
        internalDelay = duration; //provided duration value is actually duration PER CARD
        duration *= playArea.Count(); //so the actual duration is multiplied by each card
        internalElapsed = 0f;
        taking = true;
        return true;
    }

    public override void IUpdate(float dt) {
        if (taking == false) {
            State = ActionState.Done;
            return;
        }

        if (playArea.Empty()) return; //wait for taking to be complete

        internalElapsed += dt;
        if (internalElapsed > internalDelay) {
            internalElapsed = 0f;

            CardInstance card = playArea.DEBUG_RemoveFirstCardFromZone();
            card.AddAction(new A_MoveToZone(discardPile, () => Callback(card)));
        }
    }

    private void Callback(CardInstance card) {
        discardPile.DEBUG_AddCardToZone(card);
        if (playArea.Empty()) taking = false;
    }

    public override void Exit() {}
}
