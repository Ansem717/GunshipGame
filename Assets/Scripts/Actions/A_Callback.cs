//---------------------------------------------------------
// file:	A_Callback.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	An action that forwards another action and executes a callback with the forwarded action is complete.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using System;

public class A_Callback : ActionInterface {

    ActionInterface action;
    Action callback;

    public A_Callback(ActionInterface action, Action callback) { 
        name = $"Callback";
        this.action = action;
        this.callback = callback;
    }

    //Note: Duration doesn't actually matter
    //      this action is using the default duration of 1 second
    //      but it could even be 0 seconds if we wanted to
    //      the action list is not looking for duration, that's our choice how we use it

    public override bool Init() { 
        Owner.PushAfter(this, action); //"Push after this" : action.
        return true; 
    }

    public override void PostWait() {}
    public override void IUpdate(float dt) {
        if (action == null || action.State == ActionState.Done) State = ActionState.Done;
    }

    public override void Exit() => callback();
    public override float GetEstimatedDuration() => action.GetEstimatedDuration();

}
