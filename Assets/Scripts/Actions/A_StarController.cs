using UnityEngine;

public class A_StarController : ActionInterface {

    private float angle;

    private const float rotationValue = 20;
    private const float rotationDuration = 10f;

    public A_StarController() : 
        base(_duration: rotationDuration, _easing: EaseType.None) => name = "StarController";

    public override bool Init() => true;

    public override void PostWait() {
        float angleValue = (Random.value < 0.5f) ? -rotationValue : rotationValue;
        angle = (objRef.transform.rotation.eulerAngles.z + angleValue) % 360;
        Owner.PushBack(new A_Rotate(angle, _duration: duration, _easing: easing));
    }

    public override void IUpdate(float dt) {
        if (!Helper.IsInsideViewport(objRef.transform.position)) {
            State = ActionState.Done;
        }

        if (GetProgress() > 1f) Loop();
    }

    public override void Exit() {
    }

}
