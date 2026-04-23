using UnityEngine;

public class ChainGun : MonoBehaviour {

    [Header("Windup Timing")]
    [Min(0.1f)]
    [Tooltip("Seconds to reach maximum windup")]
    public float WindupTimeToMax = 1f;
    [Tooltip("Cooldown speed multiplier (1 = same as windup)")]
    public float CooldownMultipler = 0.8f;
    [Min(0f)]
    [Tooltip("Windup/cooldown accelerates at higher levels. Speed = 1 + (normalized^2 x Acceleration)")]
    public float Acceleration = 2f;
    
    [Header("Fire Rate")]
    [Min(0.1f)]
    [Tooltip("Bullets per second at minimum windup")]
    public float MinimumFireRate = 0.5f;
    [Min(0.1f)]
    [Tooltip("Bullets per second at maximum windup")]
    public float MaximumFireRate = 10f;

    [Header("Bullet Properties")]
    public float BulletSpeed;
    public float BulletLifetime;
    public float BulletRange;
    [Min(0f)]
    [Tooltip("Angle spread (degrees) at minimum windup")]
    public float MinDeviation = 2f;
    [Min(0f)]
    [Tooltip("Angle spread (degrees) at maximum windup")]
    public float MaxDeviation = 10f;

    [Header("Visuals")]
    public Vector3 indicatorOffset;
    [Tooltip("Time in seconds for cone to fade in/out")]
    public float coneFadeDuration = 0.15f;

    // Runtime state
    private float WindupElapsed;
    private float BulletElapsed;
    private bool IsShooting;
    private CustomPhysics shipPhysics;
    private float coneOpacity;
    private bool coneFadingOut;

    // Visual instances
    private GameObject indicatorInstance;
    private ChainGunIndicatorScript indicator;
    private GameObject coneInstance;
    private ChainGunCone cone;

    // Prefabs (loaded from MasterController)
    private GameObject IndicatorPrefab;
    private GameObject ConePrefab;

    private static bool RadialVisualShowing; //A static bool used to prevent multiple chain guns from showing the radial visual

    /// <summary>
    /// Returns windup progress normalized (0-1).
    /// </summary>
    public float WindupNormalized => Mathf.Clamp01(WindupElapsed / WindupTimeToMax);

    void Start() {
        IsShooting = false;
        WindupElapsed = 0;
        shipPhysics = transform.parent.GetComponentInChildren<CustomPhysics>();
        IndicatorPrefab = MasterController.Singleton.prefabs["ChainGunIndicator"];
        ConePrefab = MasterController.Singleton.prefabs["ChainGunCone"];
        RadialVisualShowing = false;
    }

    public void LoadData(ChainGun_SO chainGunData) {
        WindupTimeToMax = chainGunData.WindupTimeToMax;
        CooldownMultipler = chainGunData.CooldownMultipler;
        Acceleration = chainGunData.Acceleration;
        MinimumFireRate = chainGunData.MinimumFireRate;
        MaximumFireRate = chainGunData.MaximumFireRate;
        BulletSpeed = chainGunData.BulletSpeed;
        BulletLifetime = chainGunData.BulletLifetime;
        BulletRange = chainGunData.BulletRange;
        MinDeviation = chainGunData.MinDeviation;
        MaxDeviation = chainGunData.MaxDeviation;
        indicatorOffset = chainGunData.indicatorOffset;
        coneFadeDuration = chainGunData.coneFadeDuration;
    }

    void Update() {
        if (MasterController.Singleton.Paused) return;
        float windupNormalized = UpdateWindup();
        float currentDeviation = Mathf.Lerp(MinDeviation, MaxDeviation, windupNormalized);

        if (transform.parent.CompareTag("Player")) UpdateVisuals(windupNormalized, currentDeviation); //if this is the player, update the gunship visuals

        if (windupNormalized > 0) {
            UpdateFiring(windupNormalized, currentDeviation);
        }
    }

    private void OnDestroy() {
        if (indicatorInstance != null) Destroy(indicatorInstance);
        if (coneInstance != null) Destroy(coneInstance);
    }

    float UpdateWindup() {
        float windupNormalized = Mathf.Clamp01(WindupElapsed / WindupTimeToMax);
        float rampSpeed = 1f + (windupNormalized * windupNormalized) * Acceleration;
        
        if (IsShooting) {
            WindupElapsed += Time.deltaTime * rampSpeed;
        } else if (WindupElapsed > 0) {
            WindupElapsed -= Time.deltaTime * CooldownMultipler * rampSpeed;
        }
        
        WindupElapsed = Mathf.Clamp(WindupElapsed, 0, WindupTimeToMax);
        return Mathf.Clamp01(WindupElapsed / WindupTimeToMax);
    }

    void UpdateVisuals(float windupNormalized, float currentDeviation) {

        bool shouldShowVisuals = windupNormalized > 0;

        // Indicator lifecycle (instant)
        if (shouldShowVisuals) {
            if (indicatorInstance == null && IndicatorPrefab != null && !RadialVisualShowing) {
                indicatorInstance = Instantiate(IndicatorPrefab);
                indicator = indicatorInstance.GetComponent<ChainGunIndicatorScript>();
                RadialVisualShowing = true;
            }
        }
        if (!shouldShowVisuals && indicatorInstance != null) {
            Destroy(indicatorInstance);
            indicatorInstance = null;
            indicator = null;
            RadialVisualShowing = false;
        }

        // Cone lifecycle (with fade)
        if (shouldShowVisuals && coneInstance == null && ConePrefab != null) {
            coneInstance = Instantiate(ConePrefab);
            cone = coneInstance.GetComponent<ChainGunCone>();
            coneOpacity = 0f;
            coneFadingOut = false;
        }
        
        // Cone fade in/out
        if (cone != null) {
            if (shouldShowVisuals && !coneFadingOut) {
                // Fade in
                coneOpacity = Mathf.MoveTowards(coneOpacity, 1f, Time.deltaTime / coneFadeDuration);
            } else {
                // Fade out
                coneFadingOut = true;
                coneOpacity = Mathf.MoveTowards(coneOpacity, 0f, Time.deltaTime / coneFadeDuration);
                if (coneOpacity <= 0f) {
                    Destroy(coneInstance);
                    coneInstance = null;
                    cone = null;
                    coneFadingOut = false;
                }
            }
        }

        // Update visuals
        if (indicator != null) {
            indicatorInstance.transform.position = transform.parent.position + indicatorOffset;
            indicator.SetWindup(windupNormalized);
        }
        if (cone != null) {
            coneInstance.transform.position = transform.position; //the cone spawns at the position of the chaingun
            coneInstance.transform.rotation = transform.parent.rotation;
            cone.SetDeviation(currentDeviation);
            cone.SetOpacity(coneOpacity);
        }
    }

    void UpdateFiring(float windupNormalized, float currentDeviation) {
        BulletElapsed += Time.deltaTime;
        float currentFireRate = Mathf.Lerp(MinimumFireRate, MaximumFireRate, windupNormalized);
        float bulletCooldown = 1f / currentFireRate;
        
        if (BulletElapsed > bulletCooldown) {
            BulletElapsed = 0;
            SpawnBullet(currentDeviation, windupNormalized > 0.99f);
        }
    }

    void SpawnBullet(float deviation, bool hasTrail) {

        string bulletType = transform.parent.CompareTag("Player") ? "PlayerBullet" : "EnemyBullet";
        GameObject bullet = Instantiate(MasterController.Singleton.prefabs[bulletType], transform.position, transform.parent.rotation);
        
        if (bullet.TryGetComponent(out ActionList bal)) {
            float randomAngle = Random.Range(-deviation, deviation);
            Vector3 direction = Quaternion.Euler(0, 0, randomAngle) * transform.parent.up;
            
            Vector3 velocity = direction * BulletSpeed;
            if (shipPhysics != null) velocity += shipPhysics.VelocityVector;
            
            Vector3 displacement = velocity * BulletLifetime;
            
            bal.PushBack(new A_Callback(
                action: new A_MoveInDirection(
                    relativeDirection: displacement, 
                    _duration: BulletLifetime
                ),
                callback: () => Destroy(bullet)
            ));

            bal.PushBack(new A_DestroyAfterDistance(
                source: transform.position,
                maxDistance: BulletRange
            ));
        }

        if (hasTrail && bullet.TryGetComponent(out ParticleSystem bps)) {
            bps.Play();
        }
    }

    public void Use(float input) => IsShooting = input > 0;
    
}
