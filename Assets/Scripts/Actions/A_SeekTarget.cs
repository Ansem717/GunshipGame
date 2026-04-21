using UnityEngine;

public class A_SeekTarget : ActionInterface {

    // The optional Transform to chase. If null, a random point is used instead.
    private Transform assignedTarget;

    // The world-space position we are actually navigating toward each frame.
    // This is updated each frame if assignedTarget is live, otherwise fixed at Init.
    private Vector3 activeTarget;

    // Cached reference to the owner's CustomPhysics child component.
    private CustomPhysics physics;

    // Debug line renderer showing the path from this entity to its active target.
    private LineRenderer debugLine;

    private const float ArrivalRadius = 0.5f; // How close the entity must get before the action is considered complete.
    private const float AngleDeadBand = 15f;     // Minimum angle error (degrees) required before we apply rotation.
    private const float FullSpeedDistance = 10f;     // Distance at which the entity targets full speed.
    private const float ThrottleDeadBand = 0.05f;     // How much velocity error (0-1) is tolerated before switching thrust state.
    private const float MinVelocity = 0.4f;

    public A_SeekTarget(Transform assignedTarget) : base(_duration: float.MaxValue) {
        name = "SeekTarget";
        this.assignedTarget = assignedTarget;
    }

    public override bool Init() {
        // CustomPhysics lives on a child object of the owner entity.
        physics = objRef.GetComponentInChildren<CustomPhysics>();
        if (physics == null) {
            Debug.LogError("A_SeekTarget: No CustomPhysics found on owner or its children.");
            return false;
        }


        // Use the assigned target's position, or pick a random point in bounds.
        activeTarget = (assignedTarget != null) ? assignedTarget.position : Helper.RandomPointInBounds();

        // Create a standalone GameObject for the debug line (not parented to objRef,
        // because ClearComponentsAndScripts destroys all children during gunship swaps).
        GameObject lineObj = new GameObject("SeekTarget_DebugLine");
        Object.DontDestroyOnLoad(lineObj);
        debugLine = lineObj.AddComponent<LineRenderer>();
        debugLine.positionCount = 2;
        debugLine.useWorldSpace = true;
        debugLine.startWidth = 0.01f;
        debugLine.endWidth = 0.01f;
        debugLine.material = new Material(Shader.Find("Sprites/Default"));
        debugLine.startColor = Color.white;
        debugLine.endColor = Color.red;
        debugLine.gameObject.SetActive(false);

        return true;
    }

    public override void PostWait() { }

    public override void IUpdate(float dt) {
        // Re-acquire physics each frame — ClearComponentsAndScripts destroys
        // the old CustomPhysics child when switching gunships.
        physics = objRef.GetComponentInChildren<CustomPhysics>();
        if (physics == null) return;

        // If we have a live target Transform, keep updating the destination each frame.
        if (assignedTarget != null) {
            activeTarget = assignedTarget.position;
        }

        Vector3 toTarget = activeTarget - objRef.transform.position;
        float distance = toTarget.magnitude;

        // Arrived — mark the action as complete.
        if (distance < ArrivalRadius) {
            State = ActionState.Done;
            return;
        }

        // Debug line: owner → active target.
        if (MasterController.Singleton.DebugFlag) {
            debugLine.gameObject.SetActive(true);
            debugLine.SetPosition(0, objRef.transform.position);
            debugLine.SetPosition(1, activeTarget);
        } else {
            debugLine.gameObject.SetActive(false);
        }

        float aimBias = 0;//Random.Range(-6f, 6f);

        // Compute how many degrees we need to rotate to face the target.
        // Positive = target is to our left, negative = target is to our right.
        float angle = Vector3.SignedAngle(objRef.transform.up, toTarget.normalized, Vector3.forward) + aimBias;

        // Predictive rotation: estimate the angle we'd sweep if we started braking right now.
        // brakeAngle = v² / (2a) — the classic kinematic stopping distance, applied to rotation.
        // If that sweep would carry us past the target, flip the input to start decelerating early.
        float rotVel = physics.RotationalVelocity;
        float brakeAngle = (rotVel * rotVel) / (2f * physics.maxRotationalAcceleration);

        if (angle > AngleDeadBand) {
            // Need to turn left. If we're already spinning left fast enough to overshoot, brake.
            bool willOvershoot = rotVel > 0f && brakeAngle >= angle;
            physics.ApplyRotationalInput(willOvershoot ? -1f : 1f);
        } else if (angle < -AngleDeadBand) {
            // Need to turn right. If we're already spinning right fast enough to overshoot, brake.
            bool willOvershoot = rotVel < 0f && brakeAngle >= -angle;
            physics.ApplyRotationalInput(willOvershoot ? 1f : -1f);
        } else {
            physics.ApplyRotationalInput(0f);
        }

        // Aggressive seek: always target max speed unless facing error is large.
        float facingFactor = Mathf.InverseLerp(180f, 30f, Mathf.Abs(angle)); // sharper cutoff for slowing
        float desiredVelocity = Mathf.Lerp(MinVelocity, 1f, facingFactor); // Only slow for large turns

        // If facing is good (angle small), go full speed. If not, slow down just enough to turn.
        float velocityError = desiredVelocity - physics.VelocityNormalized;
        if (velocityError > ThrottleDeadBand)        physics.ApplyThrustInput(1f);   // accelerate
        else if (velocityError < -ThrottleDeadBand)  physics.ApplyThrustInput(-1f);  // brake
        else                                         physics.ApplyThrustInput(0f);   // coast
    }

    public override void Exit() {
        physics?.ApplyThrustInput(0f);
        physics?.ApplyRotationalInput(0f);
        Object.Destroy(debugLine.gameObject);
    }

}
