//---------------------------------------------------------
// file:	A_Knockback.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	An action that applies an instantaneous knockback force to
//          CustomPhysics from a world-space origin and completes immediately.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;

public class A_Knockback : ActionInterface {

    private Vector3 origin;
    private float strength;

    public A_Knockback(Vector3 origin, float strength, float? _delay = null, bool? _blocking = null)
        : base(_delay: _delay, _blocking: _blocking) {
        name = "Knockback";
        this.origin = origin;
        this.strength = strength;
    }

    public override bool Init() {
        CustomPhysics physics = objRef.GetComponentInChildren<CustomPhysics>();
        if (physics == null) {
            Debug.LogError("A_Knockback: No CustomPhysics found on object or its children.");
            return false;
        }
        physics.ApplyKnockback(origin, strength);
        return false;
    }

    public override void PostWait() { }
    public override void IUpdate(float dt) { }
    public override void Exit() { }

}
