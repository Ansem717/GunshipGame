//---------------------------------------------------------
// file:	A_FadeOut.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	Derive the ColorShift action to force a 1 to 0 alpha fade
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;

public class A_FadeOut : A_ColorShift {

    public A_FadeOut(float? _speed = null, EaseType? _easing = null, float? _duration = null, float? _delay = null, bool? _blocking = null) : 
        base(Color.black, _speed, _easing, _duration, _delay, _blocking) {
        name = "FadeOut";
    }

    public override bool Init() {
        if (!Owner.TryGetComponent(out uiImg)) {
            Debug.LogError("Cannot get Image from FadeOut action");
            return false;
        }

        // Get current graphic color as the starting point
        From = uiImg.color;
        // Target is the same color but fully transparent
        To = new Color(From.r, From.g, From.b, 0f);

        if (speed.HasValue && speed.Value > 0f) {
            float dist = From.a - To.a; // Just the alpha distance for fade
            duration = Mathf.Max(0.0001f, dist / speed.Value);
        }
        return true;
    }
}
