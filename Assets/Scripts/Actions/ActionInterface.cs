//---------------------------------------------------------
// file:	ActionInterface.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Fall 2025
//
// brief:	The main abstract interface for all actions.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;

public enum EaseType { None, EaseIn, EaseOut, EaseInOut };

public abstract class ActionInterface {

    /// <summary>
    /// default states for child implementations to allow nullable types.
    /// </summary>
    public const float DefaultDuration = 1f;
    public const float DefaultDelay = 0f;
    public const bool DefaultBlocking = false;


    /// <summary>
    /// Using state-style logic to track where in the action we are.
    /// </summary>
    public enum ActionState { Starting, Waiting, Running, Done }

    public ActionInterface(float? _speed = null, EaseType? _easing = null, float? _duration = null, float? _delay = null, bool? _blocking = null) {
        easing = (_easing == null) ? EaseType.None : _easing.Value;
        speed = _speed;

        if (speed != null) {
            duration = -1f; // sentinel, computed during init
        } else {
            duration = _duration ?? DefaultDuration;
        }

        delay = _delay ?? DefaultDelay;
        blocking = _blocking ?? DefaultBlocking;

        markedForDelete = false;
        State = ActionState.Starting;
        elapsed = 0;
    }

    /// <summary>
    /// A reference to the game object.
    /// </summary>
    public GameObject objRef;

    /// <summary>
    /// A pointer to the owner of the action list. 
    /// </summary>
    protected ActionList owner;
    public ActionList Owner { get => owner; }

    /// <summary>
    /// A function to wrap owner setting for handling child actions.
    /// </summary>
    public virtual void SetOwner(ActionList actionList) {
        owner = actionList;
        objRef = owner.gameObject;
    }

    /// <summary>
    /// A string ID for this action
    /// </summary>
    public string name;

    /// <summary>
    /// A state to track the action's lifecycle (note: not a state machine)
    /// </summary>
    private ActionState state;
    public ActionState State { 
        get => state; 
        set { 
            state = value;
            //These conditions occur on setter - so when we ENTER the state, we reset the timers for that state.
            if (state == ActionState.Starting || state == ActionState.Waiting) delayElapsed = 0f;
            if (state == ActionState.Starting || state == ActionState.Running) elapsed = 0f;
        } 
    }

    /// <summary>
    /// An easing state 
    /// </summary>
    public EaseType easing;

    /// <summary>
    /// Track how long this action has taken
    /// </summary>
    public float elapsed;

    /// <summary>
    /// Last N seconds *after delay*
    /// </summary>
    public float duration;

    /// <summary>
    /// Calculate percentage complete.
    /// </summary>
    public float GetProgress() => elapsed / duration;

    public float GetProgressWithEasing() {
        float t = GetProgress();
        return easing switch {
            EaseType.None => t,
            EaseType.EaseIn => t * t,
            EaseType.EaseOut => 1f - (1f - t) * (1f - t),
            EaseType.EaseInOut => (t < 0.5f) ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f,
            _ => t
        };
    }

    /// <summary>
    /// This function returns a duration in seconds that is calculated when Speed is used.
    /// </summary>
    public virtual float GetEstimatedDuration() => 0f;

    /// <summary>
    /// Track how long the action has been waiting
    /// </summary>
    public float delayElapsed;

    /// <summary>
    /// Do not execute dring first N seconds
    /// </summary>
    public float delay;

    /// <summary>
    /// Calculate percentage complete of delay
    /// </summary>
    /// <returns></returns>
    public float GetDelayProgress() => delayElapsed / delay;

    /// <summary>
    /// If speed is used, recalculate duration to fit the given speed
    /// </summary>
    public float? speed = null;

    /// <summary>
    /// A boolean flag to block future actions; stopping the action list early.
    /// </summary>
    public bool blocking;

    /// <summary>
    /// A boolean flag to mark this action for delete to clear it at the end of the AL update loop.
    /// </summary>
    public bool markedForDelete;

    /// <summary>
    /// Determines if this action can be blocked by another action. Override to change blocking behavior.
    /// </summary>
    public virtual bool CanBeBlockedBy(ActionInterface blocker) => true;

    /// <summary>
    /// Setup code before the first update. Return false to skip Update and exit the action.
    /// </summary>
    public abstract bool Init();

    /// <summary>
    /// Additional one time call before update but after waiting (incase data changes or we're on a loop)
    /// </summary>
    public abstract void PostWait();

    /// <summary>
    /// Update Loop.
    /// </summary>
    public abstract void IUpdate(float dt);

    /// <summary>
    /// Execute code before exiting (and destroying) this action
    /// </summary>
    public abstract void Exit();

    public string Log() => $"{name} | {State}";

    public void Loop() {
        if (State != ActionState.Done) State = ActionState.Waiting;
        elapsed = 0f;
        delayElapsed = 0f;
    }    
}
