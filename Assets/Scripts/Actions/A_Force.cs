//---------------------------------------------------------
// file:	A_Force.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	An action that applies a continuous force to CustomPhysics each
//          update for a given duration.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;

public class A_Force : ActionInterface {

    private Vector3 force;
    private CustomPhysics physics;

    public A_Force(Vector3 force, float? _duration = null, float? _delay = null, bool? _blocking = null)
        : base(_duration: _duration, _delay: _delay, _blocking: _blocking) {
        name = "Force";
        this.force = force;
    }

    public override bool Init() {
        physics = objRef.GetComponentInChildren<CustomPhysics>();
        if (physics == null) {
            Debug.LogError("A_Force: No CustomPhysics found on object or its children.");
            return false;
        }
        return true;
    }

    public override void PostWait() { }

    public override void IUpdate(float dt) {
        physics.ApplyForce(force);
        if (GetProgress() > 1f) State = ActionState.Done;
    }

    public override void Exit() { }

}
