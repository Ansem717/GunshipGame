//---------------------------------------------------------
// file:	A_MoveToZone.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	Action to combine Move, Rotate, and possibly Flip when moving a card to a zone.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

public class A_MoveToZone : ActionInterface {

    public enum MoveType { RANDOM }

    public MoveType moveType;
    public Zone zone;
    public Action callback;

    public A_MoveToZone(Zone zone, Action callback, float _duration = 0.5f) : base(_duration: _duration) {
        name = "MoveToZone";
        this.zone = zone;
        this.callback = callback;
    }

    public override bool Init() {

        //Get the card's actual size (from 9-slice SpriteRenderer)
        Vector2 cardSize = cardReference.GetCardSize();

        //Get valid placement bounds that account for card size
        Dictionary<Zone.BoundaryRanges, float> placementBounds = zone.GetValidCardPlacementBounds(cardSize);

        //establish a move action to put the card into a random spot in the zone
        cardReference.AddAction(Helpers.BuildActionWithRandomFloats(
            (r) => new A_MoveToVector(
                destination: new Vector3(r["x"], r["y"], 0),
                _easing: EaseType.EaseOut,
                _duration: duration
            ),
            new RandomEntry("x", placementBounds[Zone.BoundaryRanges.MinX], placementBounds[Zone.BoundaryRanges.MaxX]),
            new RandomEntry("y", placementBounds[Zone.BoundaryRanges.MinY], placementBounds[Zone.BoundaryRanges.MaxY])
        ));

        //create flip action if card is heading to player zone and is facedown
        if (zone.IsVisibleZone && cardReference.IsFaceDown) {
            cardReference.AddAction(new A_Flip(
                isFlippingDown: false,
                _easing: EaseType.EaseInOut,
                _duration: duration
            ));
        }

        //create a random rotate action to face approximately the correct direction with a bit of spin after it
        ActionInterface RotateWithSpin = Helpers.BuildActionWithRandomFloats(
            (r) => new A_Rotate(
                targetAngle: r["angle"],
                _easing: EaseType.EaseOut,
                _duration: duration * r["duration_multiplier"]
            ),
            new RandomEntry("angle", zone.transform.localEulerAngles.z - 20, zone.transform.localEulerAngles.z + 20),
            new RandomEntry("duration_multiplier", 1.25f, 2f)
        );

        cardReference.AddAction(RotateWithSpin);

        return true;
    }

    public override void IUpdate(float dt) {
        if (GetProgress() >= 1.0f) State = ActionState.Done;
    }

    public override void Exit() {
        callback();
    }
}
