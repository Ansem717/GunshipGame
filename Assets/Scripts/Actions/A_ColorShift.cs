//---------------------------------------------------------
// file:	A_ColorShift.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	An action to shift the color of an object
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

public class A_ColorShift : ActionInterface {

    protected Color From;
    protected Color To;

    protected Graphic uiImg;

    public A_ColorShift(Color targetColor, float? _speed = null, EaseType? _easing = null, float? _duration = null, float? _delay = null, bool? _blocking = null)
        : base(_speed, _easing, _duration, _delay, _blocking) {
        name = "ColorShift";
        To = targetColor;
    }

    public override bool Init() {
        if (!Owner.TryGetComponent(out uiImg)) {
            Debug.LogError("Cannot get Image from ColorShift action");
        }

        From = uiImg.color;
        if (speed.HasValue && speed.Value > 0f) {
            float dist = ColorDistance(From, To);
            duration = Mathf.Max(0.0001f, dist / speed.Value);
        }
        return true;
    }

    public override void IUpdate(float dt) {
        if (duration <= 0f) {
            uiImg.color = To;
            State = ActionState.Done;
            return;
        }

        if (elapsed < duration) {
            float t = GetProgressWithEasing(); // a fraction of elapsed/duration

            Color val = Color.Lerp(From, To, t);
            uiImg.color = val;

        } else {
            uiImg.color = To;
            State = ActionState.Done;
        }
    }

    private float ColorDistance(Color a, Color b) {
        float dr = a.r - b.r;
        float dg = a.g - b.g;
        float db = a.b - b.b;
        float da = a.a - b.a;
        return Mathf.Sqrt(dr * dr + dg * dg + db * db + da * da);
    }

    public override void Exit() { 
        uiImg.color = To; 
    }

    public override float GetEstimatedDuration() {
        From = uiImg.color;
        if (speed.HasValue && speed.Value > 0f) {
            float dist = ColorDistance(From, To);
            return Mathf.Max(0.0001f, dist / speed.Value);
        }
        return duration;
    }

}