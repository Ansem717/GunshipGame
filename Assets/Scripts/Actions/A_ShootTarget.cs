using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class A_ShootTarget : ActionInterface {

    private Transform assignedTarget;
    private ChainGun[] guns;
    private CustomPhysics engine;

    private Vector3 activeTargetPos;
    private const float AngleDeadBand = 5f;     // Minimum angle error (degrees) required before we apply rotation.

    public A_ShootTarget(Transform assignedTarget) {
        name = "ShootTarget";
        this.assignedTarget = assignedTarget;
    }

    public override void Exit() => Debug.LogWarning("A_ShootTarget action finished, even though it's a permanent");
    public override bool Init() {
        guns = objRef.GetComponentsInChildren<ChainGun>();
        return true;
    }

    public override void IUpdate(float dt) {
        if (guns.Length <= 0) guns = objRef.GetComponentsInChildren<ChainGun>();
        if (guns.Length <= 0) return;
        if (guns[0] == null) return;

        engine ??= objRef.GetComponentInChildren<CustomPhysics>();
        if (engine == null) return;
        if (assignedTarget == null) State = ActionState.Done;

        //update target location
        activeTargetPos = assignedTarget.position;
        activeTargetPos.z = 0;

        Vector3 curr = objRef.transform.position;
        curr.z = 0;

        Vector3 toTarget = activeTargetPos - curr;

        bool inRange = toTarget.sqrMagnitude < guns[0].BulletRange * guns[0].BulletRange && Helper.IsInsideViewport(curr);
        blocking = inRange;

        foreach (ChainGun gun in guns) {
            gun.Use(inRange ? 1 : 0);
        }

        //We can't do the fancy thing above because that would influence our Seek action.
        if (inRange) {
            engine.ApplyThrustInput(-1); //put on the brakes


            //!! Copied code from A_Seek: Rotate to Target using Physics

            float angle = Vector3.SignedAngle(objRef.transform.up, toTarget.normalized, Vector3.forward);

            float rotVel = engine.RotationalVelocity;
            float brakeAngle = (rotVel * rotVel) / (2f * engine.maxRotationalAcceleration);

            if (angle > AngleDeadBand) {
                // Need to turn left. If we're already spinning left fast enough to overshoot, brake.
                bool willOvershoot = rotVel > 0f && brakeAngle >= angle;
                engine.ApplyRotationalInput(willOvershoot ? -1f : 1f);
            } else if (angle < -AngleDeadBand) {
                // Need to turn right. If we're already spinning right fast enough to overshoot, brake.
                bool willOvershoot = rotVel < 0f && brakeAngle >= -angle;
                engine.ApplyRotationalInput(willOvershoot ? 1f : -1f);
            } else {
                engine.ApplyRotationalInput(0f);
            }
        }
    }

    public override void PostWait() { }
}
