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

    private float velocity;
    private float acceleration;
    private float jerk;
    private float rotationalVelocity;
    private float rotationalAcceleration;
    private float rotationalJerk;

    // External forces (knockback, explosions, etc.)
    private Vector3 externalVelocity;

    /// <summary>
    /// Returns the current velocity as a world-space vector (controlled + external).
    /// </summary>
    public Vector3 VelocityVector => transform.up * velocity + externalVelocity;

    /// <summary>
    /// Returns velocity normalized to max velocity (0-1).
    /// </summary>
    public float VelocityNormalized => maxVelocity > 0 ? Mathf.Clamp01(velocity / maxVelocity) : 0f;

    public enum MovementState { None, Forward, Backward }
    public enum RotationState { None, Left, Right }

    private MovementState moveState;
    private RotationState rotationState;

    [Header("Debug Lines")]
    public LineRenderer VelLine;
    public LineRenderer RotLine;

    void Awake() {
        if (transform.parent == null) {
            Debug.LogError("ERROR: CustomPhysics script cannot be a root level entity.");
        }

        velocity = 0;      
        acceleration = 0;  
        jerk = 0;          
        rotationalVelocity = 0;
        rotationalAcceleration = 0;
        rotationalJerk = 0;
        externalVelocity = Vector3.zero;

        moveState = MovementState.None;
        rotationState = RotationState.None;
    }

    public void LoadData(CustomPhysics_SO data) {
        drag = data.drag;
        angularDrag = data.angularDrag;

        maximumAcceleration = data.maximumAcceleration;
        brakeAcceleration = data.brakeAcceleration;
        maxVelocity = data.maxVelocity;
        jerkStrength = data.jerkStrength;

        maxRotationalAcceleration = data.maxRotationalAcceleration;
        maxRotationalVelocity = data.maxRotationalVelocity;
        rotationalJerkStrength = data.rotationalJerkStrength;
    }

    void FixedUpdate() {
        if (MasterController.Singleton.Paused) return;
        float dt = Time.fixedDeltaTime;

        HandleTranslation(dt);
        HandleRotation(dt);

        //apply Drag
        velocity *= 1f - drag * dt;
        rotationalVelocity *= 1f - angularDrag * dt;
        externalVelocity *= 1f - drag * dt;

        // Hard cap external velocity to prevent game-breaking speeds (2x normal max)
        float hardCap = maxVelocity * 2f;
        if (hardCap > 0f && externalVelocity.sqrMagnitude > hardCap * hardCap) {
            externalVelocity = externalVelocity.normalized * hardCap;
        }

        //Apply Movement
        transform.parent.position += VelocityVector * dt;
        transform.parent.Rotate(0, 0, rotationalVelocity * dt);

        if (MasterController.Singleton.DebugFlag) {
            VelLine.gameObject.SetActive(true);
            RotLine.gameObject.SetActive(true);

            // Minimum line length for visibility
            const float minLineLength = 0.5f;

            // Velocity line
            Vector3 velDir = VelocityVector;
            float velMag = velDir.magnitude;
            if (velMag > 0.001f) {
                velDir = velDir.normalized * Mathf.Max(velMag, minLineLength);
            }
            VelLine.SetPosition(0, transform.parent.position);
            VelLine.SetPosition(1, transform.parent.position + velDir);

            RotLine.SetPosition(0, transform.parent.position);
            RotLine.SetPosition(1, transform.parent.position + dt * -rotationalVelocity * transform.right);

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

    public Vector3 ExternalVelocity => externalVelocity;
    public float RotationalVelocity => rotationalVelocity;

    public void ClearExternalVelocity() => externalVelocity = Vector3.zero;
    public void ApplyImpulse(Vector3 impulse) => externalVelocity += impulse;
    public void ApplyRotationalImpulse(float impulse) => rotationalVelocity += impulse;
    public void ApplyForce(Vector3 force) => externalVelocity += force * Time.fixedDeltaTime;

    public void ApplyKnockback(Vector3 origin, float strength) {
        Vector3 direction = (transform.parent.position - origin).normalized;
        externalVelocity += direction * strength;
    }

}
