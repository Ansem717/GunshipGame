//---------------------------------------------------------
// file:	A_WaitThenCallback.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	An action used to wait an alloted amount of time and then trigger a one-time void func
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using System;

public class A_WaitThenCallback : ActionInterface {

    public Action callback;

    public A_WaitThenCallback(float duration, bool blocking, Action callback) : base(_duration: duration, _blocking: blocking) {
        name = "Wait_Callback";
        this.callback = callback;
    }

    public override bool Init() {
        return true;
    }

    public override void IUpdate(float dt) {
        if (GetProgress() > 1f) State = ActionState.Done;
    }

    public override void Exit() {
        callback();
    }

    public override void PostWait() { }

}
