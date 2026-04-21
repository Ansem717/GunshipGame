//---------------------------------------------------------
// file:	A_RotationalImpulse.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	An action that applies an instantaneous rotational velocity impulse
//          to CustomPhysics and completes immediately.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;

public class A_RotationalImpulse : ActionInterface {

    private float impulse;

    public A_RotationalImpulse(float impulse, float? _delay = null, bool? _blocking = null)
        : base(_delay: _delay, _blocking: _blocking) {
        name = "RotationalImpulse";
        this.impulse = impulse;
    }

    public override bool Init() {
        CustomPhysics physics = objRef.GetComponentInChildren<CustomPhysics>();
        if (physics == null) {
            Debug.LogError("A_RotationalImpulse: No CustomPhysics found on object or its children.");
            return false;
        }
        physics.ApplyRotationalImpulse(impulse);
        return false;
    }

    public override void PostWait() { }
    public override void IUpdate(float dt) { }
    public override void Exit() { }

}
