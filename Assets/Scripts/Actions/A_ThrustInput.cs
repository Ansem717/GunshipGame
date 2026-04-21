//---------------------------------------------------------
// file:	A_ThrustInput.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	An action that applies a thrust input to CustomPhysics for a given
//          duration, then resets the input to zero on exit.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;

public class A_ThrustInput : ActionInterface {

    private float input;
    private CustomPhysics physics;

    public A_ThrustInput(float input, float? _duration = null, float? _delay = null, bool? _blocking = null)
        : base(_duration: _duration, _delay: _delay, _blocking: _blocking) {
        name = "ThrustInput";
        this.input = input;
    }

    public override bool Init() {
        physics = objRef.GetComponentInChildren<CustomPhysics>();
        if (physics == null) {
            Debug.LogError("A_ThrustInput: No CustomPhysics found on object or its children.");
            return false;
        }
        physics.ApplyThrustInput(input);
        return true;
    }

    public override void PostWait() { }

    public override void IUpdate(float dt) {
        if (GetProgress() > 1f) State = ActionState.Done;
    }

    public override void Exit() {
        physics?.ApplyThrustInput(0f);
    }

}
