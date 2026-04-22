using UnityEngine;

[CreateAssetMenu(fileName = "Missle_SO", menuName = "Custom/Missle")]
public class Missle_SO : WeaponScriptableObject {

    [Header("Arming")]
    [Tooltip("Seconds before the missile activates after being dropped")]
    public float ArmingTime;

    [Header("Search")]
    [Tooltip("Maximum radius of each scan pulse")]
    public float ScanRadius;
    [Tooltip("Number of scan pulses before self-destruct")]
    public int MaxScans;
    [Tooltip("Duration of each scan pulse expansion (seconds)")]
    public float ScanInterval;

    [Header("Explosion")]
    [Tooltip("Radius of the explosion damage area")]
    public float ExplosionRadius;
    [Tooltip("Damage applied to objects within the explosion")]
    public float ExplosionDamage;
    [Tooltip("Duration of the explosion visual (seconds)")]
    public float ExplosionDuration;

    [Header("Spawn")]
    [Tooltip("Lateral impulse applied perpendicular to the ship when dropped")]
    public float SidewaysKick;

    [Header("Physics")]
    [Tooltip("Physics data for the missile body")]
    public CustomPhysics_SO PhysicsData;

    [Header("Launcher")]
    [Tooltip("Maximum missles the launcher can hold")]
    [Min(1)]
    public int MaxStored;
    [Tooltip("Seconds to recharge one missle")]
    [Min(0.1f)]
    public float RechargeTime;
    [Tooltip("Seconds between each missle drop while firing (cluster dropoff rate)")]
    [Min(0.05f)]
    public float DropoffInterval;

    [Header("UI")]
    public Vector2 CellSize;
    public Vector2 CellSpacing;
    public RectOffset GridPadding;

}
