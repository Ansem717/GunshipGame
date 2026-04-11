//---------------------------------------------------------
// file:	ActionList.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	A data structure used to manage and control the flow of the game through actions.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

public class ActionList : MonoBehaviour {

    public List<ActionInterface> actions = new();

    private List<(int, ActionInterface)> pendingActions = new();

    public float timeMultiplier = 1f;
    public void ResetTimeMultiplier() => timeMultiplier = 1f;

    public bool EnablePauseWithGame;
    public int DebugSortOrder;

    protected virtual void Update() {
        ActionInterface currentBlocker = null;
        foreach (ActionInterface act in actions) {
            if (act == null) continue;
            if (act.Owner == null) continue;

            // Skip this action if it's blocked by the current blocker
            if (currentBlocker != null && act.CanBeBlockedBy(currentBlocker)) continue;

            if (act.State == ActionInterface.ActionState.Starting) {
                bool stay = act.Init();
                //Debug.Log($"{gameObject.name} Action Started: {act.name}");

                act.State = !stay ? ActionInterface.ActionState.Done : ActionInterface.ActionState.Waiting;

            }

            float dt = Time.deltaTime * timeMultiplier;

            if (act.State == ActionInterface.ActionState.Waiting) {
                act.delayElapsed += dt;
                //Debug.Log($"{gameObject.name} Action Waiting: {act.name} {100f * act.GetDelayProgress():F0}%");
                if (act.delayElapsed >= act.delay) {
                    act.PostWait();
                    act.State = ActionInterface.ActionState.Running;
                }

            } else if (act.State == ActionInterface.ActionState.Running) {
                //Debug.Log($"{gameObject.name} Action Running: {act.name} {100f * act.GetProgress():F0}%");
                act.elapsed += dt;
                act.IUpdate(dt);

            } else if (act.State == ActionInterface.ActionState.Done) {
                //Debug.Log($"{gameObject.name} Action Done: {act.name}");
                act.Exit();
                act.markedForDelete = true;
                act.blocking = false; //finished actions immediatly stop blocking
            }

            if (act.blocking) currentBlocker = act; //this action blocks future actions that can be blocked by it
        }

        actions.RemoveAll(a => a.markedForDelete);

        foreach ((int position, ActionInterface a) in pendingActions) {
            int _position = position; //stupid tuples...
            if (_position > actions.Count) _position = actions.Count;
            actions.Insert(_position, a);
        }

        pendingActions.Clear();
    }

    public bool TryGetAction<T>(string name, out T action) where T : class {
        action = actions.Find(a => a.name == name) as T;
        return action != null;
    }

    public void PushFront(ActionInterface ai) {
        PushIntoPending(0, ai);
    }

    public void PushBack(ActionInterface ai) {
        PushIntoPending(actions.Count + pendingActions.Count, ai); //the back of the list is the current count + the new count
    }

    public void PushAfter(ActionInterface aiBefore, ActionInterface aiToPush) {
        int index = 0;
        while (index < actions.Count) {
            ActionInterface curr = actions[index];
            if (curr == aiBefore) break;
            index++;
        }
        //INDEX is now either at the position for aiSpotBefore OR it's at the end
        if (index == actions.Count) throw new System.IndexOutOfRangeException("PushAfter could not find current action.");
        PushIntoPending(index + 1, aiToPush);
    }

    private void PushIntoPending(int pos, ActionInterface ai) {
        if (TryGetAction(ai.name, out ActionInterface existing)) {
            existing.markedForDelete = true; //only one action with the same name can exist in the list. 
        }
        if (ai.SetOwner(this)) {
            //Debug.Log($"{gameObject.name} Action added: {ai.name}");
            pendingActions.Add((pos, ai));
        }
    }

    public void Clear() {
        actions.ForEach(a => a.markedForDelete = true);
    }

}
