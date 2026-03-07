using UnityEngine;

public class CustomPhysics : MonoBehaviour {

    [Header("Physical Properties")]
    public float drag;
    public float angularDrag;

    [Header("Translation")]
    public float maximumAcceleration;
    public float brakeAcceleration;
    public float maxVelocity;
    public float jerkStrength;

    [Header("Rotation")]
    public float maxRotationalAcceleration;
    public float maxRotationalVelocity;
    public float rotationalJerkStrength;

    [Header("Debug")]
    [Tooltip("Debug lines are only shown in SCENE view, not game view")]
    public bool ShowSceneDebugLines;

    private float velocity;
    private float acceleration;
    private float jerk;
    private float rotationalVelocity;
    private float rotationalAcceleration;
    private float rotationalJerk;

    public enum MovementState { None, Forward, Backward }
    public enum RotationState { None, Left, Right }

    private MovementState moveState;
    private RotationState rotationState;

    public LineRenderer VelLine;
    public LineRenderer RotLine;

    void Start() {
        velocity = 0;      
        acceleration = 0;  
        jerk = 0;          
        rotationalVelocity = 0;
        rotationalAcceleration = 0;
        rotationalJerk = 0;

        moveState = MovementState.None;
        rotationState = RotationState.None;
    }

    void FixedUpdate() {
        if (MasterController.Singleton.Paused) return;
        float dt = Time.fixedDeltaTime;

        HandleTranslation(dt);
        HandleRotation(dt);

        //apply Drag
        velocity *= 1f - drag * dt;
        rotationalVelocity *= 1f - angularDrag * dt;

        //Apply Movement
        Vector3 end = transform.up * velocity;
        if (ShowSceneDebugLines) Debug.DrawLine(transform.position, end, Color.green);
        transform.position += end * dt;
        if (ShowSceneDebugLines) Debug.DrawLine(transform.position, dt * -rotationalVelocity * transform.right, Color.red);
        transform.Rotate(0, 0, rotationalVelocity * dt);

        if (MasterController.Singleton.DebugFlag) {
            VelLine.gameObject.SetActive(true);
            RotLine.gameObject.SetActive(true);

            VelLine.SetPosition(0, transform.position);
            VelLine.SetPosition(1, transform.position + end);

            RotLine.SetPosition(0, transform.position);
            RotLine.SetPosition(1, transform.position + dt * -rotationalVelocity * transform.right);

        } else {
            VelLine.gameObject.SetActive(false);
            RotLine.gameObject.SetActive(false);
        }

    }

    void HandleTranslation(float dt) {
        float input = 0;

        if (moveState == MovementState.Forward) input = 1;
        else if (moveState == MovementState.Backward) input = -1;

        float targetAccel = 0;

        if (input > 0) targetAccel = maximumAcceleration;
        else if (input < 0) targetAccel = -brakeAcceleration;   // stronger braking

        // Jerk pushes acceleration toward target
        jerk = (targetAccel - acceleration) * jerkStrength;
        acceleration += jerk * dt;
        acceleration = Mathf.Clamp(acceleration, -brakeAcceleration, maximumAcceleration);
        velocity += acceleration * dt;
        velocity = Mathf.Clamp(velocity, 0f, maxVelocity);
    }

    void HandleRotation(float dt) {
        float input = 0;

        if (rotationState == RotationState.Left) input = 1;
        else if (rotationState == RotationState.Right) input = -1;

        float targetAccel = input * maxRotationalAcceleration;


        rotationalJerk = (targetAccel - rotationalAcceleration) * rotationalJerkStrength;

        rotationalAcceleration += rotationalJerk * dt;
        rotationalAcceleration = Mathf.Clamp(
            rotationalAcceleration,
            -maxRotationalAcceleration,
            maxRotationalAcceleration
        );

        rotationalVelocity += rotationalAcceleration * dt;
        rotationalVelocity = Mathf.Clamp(
            rotationalVelocity,
            -maxRotationalVelocity,
            maxRotationalVelocity
        );
    }

    public void ApplyThrustInput(float input) {
        if (input > 0) moveState = MovementState.Forward;
        else if (input < 0) moveState = MovementState.Backward;
        else moveState = MovementState.None;
    }

    public void ApplyRotationalInput(float input) {
        if (input > 0) rotationState = RotationState.Left;
        else if (input < 0) rotationState = RotationState.Right;
        else rotationState = RotationState.None;
    }

}
