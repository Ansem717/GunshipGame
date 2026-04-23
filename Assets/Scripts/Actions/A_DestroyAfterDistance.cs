using UnityEngine;

public class A_DestroyAfterDistance : ActionInterface {

    private Vector3 source;
    private float maxDistance;

    public A_DestroyAfterDistance(Vector3 source, float maxDistance) : base() {
        name = "DestroyAfterDistance";
        this.source = source;
        this.source.z = 0;
        this.maxDistance = maxDistance;
    }

    public override void Exit() {
        Object.Destroy(objRef);
    }

    public override bool Init() => true;

    public override void IUpdate(float dt) {
        Vector3 curr = objRef.transform.position;
        curr.z = 0;

        Vector2 distV = curr - source;

        if (distV.sqrMagnitude > maxDistance * maxDistance) {
            State = ActionState.Done;
        }
    }

    public override void PostWait() {}
}
