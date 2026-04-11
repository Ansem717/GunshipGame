using UnityEngine;

public class A_StarController : ActionInterface {

    private float angle;
    private bool hasEnteredViewport;

    private const float rotationValue = 20;
    private const float rotationDuration = 10f;

    public A_StarController() : 
        base(_duration: rotationDuration, _easing: EaseType.None) => name = "StarController";

    public override bool Init() {
        hasEnteredViewport = false;
        return true;
    }

    public override void PostWait() {
        float angleValue = (Random.value < 0.5f) ? -rotationValue : rotationValue;
        angle = (objRef.transform.rotation.eulerAngles.z + angleValue) % 360;
        Owner.PushBack(new A_Rotate(angle, _duration: duration, _easing: easing));
    }

    public override void IUpdate(float dt) {
        bool isInViewport = Helper.IsInsideViewport(objRef.transform.position);
        
        // Track when star first enters viewport
        if (isInViewport) {
            hasEnteredViewport = true;
        }
        
        // Destroy if star has entered and then left the viewport
        // OR if star drifted too far away (2x viewport) without ever entering
        if (hasEnteredViewport && !isInViewport) {
            State = ActionState.Done;
        } else if (!hasEnteredViewport && Helper.IsOutsideExpandedViewport(objRef.transform.position, 2f)) {
            State = ActionState.Done;
        }

        if (GetProgress() > 1f) Loop();
    }

    public override void Exit() {
    }

}
