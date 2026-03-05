//---------------------------------------------------------
// file:	A_FadeIn.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	Derive the ColorShift action to force a 0 to 1 alpha fade
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

public class A_FadeIn : A_ColorShift {

    private float targetAlpha;

    public A_FadeIn(Graphic graphic, float _targetAlpha = 1f, float? _speed = null, EaseType? _easing = null, float? _duration = null, float? _delay = null, bool? _blocking = null) : 
        base(Color.black, _speed, _easing, _duration, _delay, _blocking) {
        targetAlpha = Mathf.Clamp01(_targetAlpha);

        name = "FadeIn";

        // Immediately set to transparent
        if (graphic != null) {
            Color c = graphic.color;
            graphic.color = new Color(c.r, c.g, c.b, 0f);
        }
    }

    public override bool Init() {
        if (!Owner.TryGetComponent(out uiImg)) {
            Debug.LogError("Cannot get Image from FadeIn action");
            return false;
        }

        // Get current graphic color and set target with specified alpha
        Color current = uiImg.color;
        To = new Color(current.r, current.g, current.b, targetAlpha);
        // Start from the same color but fully transparent
        From = new Color(current.r, current.g, current.b, 0f);

        if (speed.HasValue && speed.Value > 0f) {
            float dist = To.a - From.a; // Just the alpha distance for fade
            duration = Mathf.Max(0.0001f, dist / speed.Value);
        }
        return true;
    }
}
