//---------------------------------------------------------
// file:	A_Flip.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	An action used to simulate flipping a card.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;

public class A_Flip : A_LerpVector3 {

    public bool IsFlippingDown;

    public A_Flip(bool isFlippingDown, float? _speed = null, EaseType? _easing = null, float? _duration = null, float? _delay = null, bool? _blocking = null)
        : base(Vector3.zero, _speed, _easing, _duration, _delay, _blocking) {
        name = "Flip";
        IsFlippingDown = isFlippingDown;
    }

    public override bool Init() {
        Vector3 curr = GetCurrent();

        if ((IsFlippingDown && curr.y == 180f) || (!IsFlippingDown && curr.y == 0f))
            return false; //Exit immediately

        To = new Vector3(curr.x, IsFlippingDown ? 180f : 0f, curr.z);
        base.Init();
        return true;
    }

    protected override Vector3 GetCurrent() {
        return Owner.transform.rotation.eulerAngles;
    }

    protected override void SetCurrent(Vector3 val) {
        Vector3 current = Owner.transform.rotation.eulerAngles;
        current.y = val.y; // Only modify Y axis
        Owner.transform.rotation = Quaternion.Euler(current);
    }

    protected override Vector3 Interpolate(Vector3 from, Vector3 to, float t) {
        float y = Mathf.LerpAngle(from.y, to.y, t);
        return new Vector3(from.x, y, from.z); //the `from` values are ignored via set current
    }

    public override void IUpdate(float dt) {
        base.IUpdate(dt);
        if (Helpers.NearlyEqual(GetProgress(), 0.5f, 0.02f)) {
            cardReference.IsFaceDown = IsFlippingDown;
        }
    }

    public override void Exit() {
        base.Exit();
        cardReference.IsFaceDown = IsFlippingDown;
    }


}
