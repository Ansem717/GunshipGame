//---------------------------------------------------------
// file:	A_StaticBlocker.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	An action that checks a static enum for finish, which will be controlled by player actions
//          This makes it easy to clear the action (static access) but can lead to issues if multiple
//          static blockers are alive at a time. I need to be careful as to how I manage the enum.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using System;

public enum SBFlag {
    Blocking,
    PlayerTurnComplete,
    PauseMenuExit
}

public class A_StaticBlocker : ActionInterface {

    public Action callback;

    public static SBFlag CurrentFlag;
    public SBFlag targetFlag;

    public A_StaticBlocker(SBFlag target, Action callback = null) : base(_blocking: true) {
        CurrentFlag = SBFlag.Blocking;
        name = $"sb_{target}"; //this allows multiple static blockers as long as they have different flags
        this.callback = callback;
        targetFlag = target;
    }

    public override bool Init() {
        return true;
    }

    /// StaticBlockers are never blocked by other StaticBlockers.
    public override bool CanBeBlockedBy(ActionInterface blocker) => blocker is not A_StaticBlocker;

    public override void IUpdate(float dt) {
        if (CurrentFlag == targetFlag) State = ActionState.Done;
    }

    public override void Exit() {
        callback?.Invoke();
    }
}
