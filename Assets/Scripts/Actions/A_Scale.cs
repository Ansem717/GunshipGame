//---------------------------------------------------------
// file:	A_Scale.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	An action to manipulate the scale of the object.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;

public class A_Scale : A_LerpVector3 {

    private float? scaleMultiplier = null;

    public A_Scale(Vector3 targetScale, float? _speed = null, EaseType? _easing = null, float? _duration = null, float? _delay = null, bool? _blocking = null)
        : base(targetScale, _speed, _easing, _duration, _delay, _blocking) {
        name = "Scale";
    }

    public A_Scale(float multiplier, float? _speed = null, EaseType? _easing = null, float? _duration = null, float? _delay = null, bool? _blocking = null)
        : base(Vector3.one, _speed, _easing, _duration, _delay, _blocking) {
        name = "Scale";
        scaleMultiplier = multiplier;
    }

    public override bool Init() {
        From = GetCurrent();
        
        if (scaleMultiplier.HasValue) {
            To = From * scaleMultiplier.Value;
        }
        
        if (speed.HasValue && speed.Value > 0f) {
            float dist = Vector3.Distance(From, To);
            duration = Mathf.Max(0.0001f, dist / speed.Value);
        }

        return true;
    }

    protected override Vector3 GetCurrent() {
        return Owner.transform.localScale;
    }

    protected override void SetCurrent(Vector3 val) {
        Owner.transform.localScale = val;
    }

}
