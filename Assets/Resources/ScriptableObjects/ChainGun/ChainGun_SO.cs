using UnityEngine;

[CreateAssetMenu(fileName = "ChainGun_SO", menuName = "Custom/ChainGun")]
public class ChainGun_SO : WeaponScriptableObject {

    [Header("Windup Timing")]
    [Min(0.1f)]
    [Tooltip("Seconds to reach maximum windup")]
    public float WindupTimeToMax;
    [Tooltip("Cooldown speed multiplier (1 = same as windup)")]
    public float CooldownMultipler;
    [Min(0f)]
    [Tooltip("Windup/cooldown accelerates at higher levels. Speed = 1 + (normalized^2 x Acceleration)")]
    public float Acceleration;

    [Header("Fire Rate")]
    [Min(0.1f)]
    [Tooltip("Bullets per second at minimum windup")]
    public float MinimumFireRate;
    [Min(0.1f)]
    [Tooltip("Bullets per second at maximum windup")]
    public float MaximumFireRate;

    [Header("Bullet Properties")]
    public float BulletSpeed;
    public float BulletLifetime;
    public float BulletRange;
    [Min(0f)]
    [Tooltip("Angle spread (degrees) at minimum windup")]
    public float MinDeviation;
    [Min(0f)]
    [Tooltip("Angle spread (degrees) at maximum windup")]
    public float MaxDeviation;

    [Header("Visuals")]
    public Vector3 indicatorOffset;
    [Tooltip("Time in seconds for cone to fade in/out")]
    public float coneFadeDuration;

}
