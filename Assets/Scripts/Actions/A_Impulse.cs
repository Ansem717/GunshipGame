//---------------------------------------------------------
// file:	A_Impulse.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	An action that applies an instantaneous velocity impulse to
//          CustomPhysics and completes immediately.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;

public class A_Impulse : ActionInterface {

    private Vector3 impulse;

    public A_Impulse(Vector3 impulse, float? _delay = null, bool? _blocking = null)
        : base(_delay: _delay, _blocking: _blocking) {
        name = "Impulse";
        this.impulse = impulse;
    }

    public override bool Init() {
        CustomPhysics physics = objRef.GetComponentInChildren<CustomPhysics>();
        if (physics == null) {
            Debug.LogError("A_Impulse: No CustomPhysics found on object or its children.");
            return false;
        }
        physics.ApplyImpulse(impulse);
        return false;
    }

    public override void PostWait() { }
    public override void IUpdate(float dt) { }
    public override void Exit() { }

}
