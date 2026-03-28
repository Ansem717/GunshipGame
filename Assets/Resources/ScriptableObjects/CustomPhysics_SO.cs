using UnityEngine;

[CreateAssetMenu(fileName = "CustomPhysics_SO", menuName = "Custom/Phyiscs")]
public class CustomPhysics_SO : ScriptableObject {
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
}
