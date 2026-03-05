//---------------------------------------------------------
// file:	A_LerpVector3.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	An abstract action subclass that repurposes Lerping on Vector3s for many effects.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;

public abstract class A_LerpVector3 : ActionInterface {

    protected Vector3 From;
    protected Vector3 To;

    protected A_LerpVector3(Vector3 target, float? _speed = null, EaseType? _easing = null, float? _duration = null, float? _delay = null, bool? _blocking = null)
        : base(_speed, _easing, _duration, _delay, _blocking) {
        To = target;
    }

    public override bool Init() {
        From = GetCurrent();
        if (speed.HasValue && speed.Value > 0f) {
            float dist = Vector3.Distance(From, To);
            duration = Mathf.Max(0.0001f, dist / speed.Value);
        }
        return true;
    }

    public override void IUpdate(float dt) {
        if (duration <= 0f) {
            SetCurrent(To);
            State = ActionState.Done;
            return;
        }

        if (elapsed < duration) {
            float t = GetProgressWithEasing();

            Vector3 val = Interpolate(From, To, t);
            SetCurrent(val);

        } else {
            SetCurrent(To);
            State = ActionState.Done;
        }
    }

    protected virtual Vector3 Interpolate(Vector3 from, Vector3 to, float t) {
        return Vector3.Lerp(from, to, t);
    }

    protected abstract Vector3 GetCurrent();
    protected abstract void SetCurrent(Vector3 val);

    public override void Exit() { SetCurrent(To); }

    public override float GetEstimatedDuration() {
        Vector3 tFrom = GetCurrent();
        if (speed.HasValue && speed.Value > 0f) {
            float dist = Vector3.Distance(tFrom, To);
            return Mathf.Max(0.0001f, dist / speed.Value);
        }
        return duration;
    }

}
