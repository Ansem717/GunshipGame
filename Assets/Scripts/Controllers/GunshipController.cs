using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GunshipController : MonoBehaviour {

    private class InputEntry {
        public string name;
        public List<KeyCode> posKeys;
        public List<KeyCode> negKeys;
        public Action<float> func;

        // computed per frame
        public float CurrentValue {
            get {
                int pos = (posKeys.Any(k => Input.GetKey(k)) ? 1 : 0);
                int neg = (negKeys.Any(k => Input.GetKey(k)) ? -1 : 0);
                return pos + neg;
            }
        }

        public InputEntry(string name, Action<float> func, List<KeyCode> posKeys, List<KeyCode> negKeys) {
            this.name = name;
            this.func = func;
            this.posKeys = posKeys;
            this.negKeys = negKeys;
        }
    }

    private List<InputEntry> InputKeys;

    public Dictionary<Type, List<MonoBehaviour>> components;

    // CAMERAS
    public float baseOrthoSize = 5f; // Base orthographic Size when stationary
    public float velocityZoomOut = 2f; // Additional zoom out at max velocity
    public float chaingunZoomOut = 0.5f; // Additional zoom out at max chaingun warmup
    public float cameraLeadTime = 0.6f; // How far ahead the camera leads (in seconds of travel
    public float cameraSmoothSpeed = 6f; // How smoothly the camera follows/zooms (lower = smoother)

    // OTHER DATAS
    public float MaxHealth = 100f;
    public float CurrentHealth;
    private bool isDying = false;

    public float PseudoMass; //ship scale

    // PHYSICS MANAGEMENT
    private CustomPhysics physics => components[typeof(CustomPhysics)][0] as CustomPhysics;
    private bool hasPhysics => components.ContainsKey(typeof(CustomPhysics)) && components[typeof(CustomPhysics)].Count > 0;

    void Start() {
        components = new();
        InputKeys = new();

        CurrentHealth = MaxHealth;

        //Load small gunship by default
        LoadGunship(MasterController.GunshipSize.Small);
    }

    public void LoadGunship(MasterController.GunshipSize size) {

        ClearComponentsAndScripts();
        InputKeys = new(); //clear current inputs

        MasterController.GunshipData data = MasterController.Singleton.GunshipDatas[size];

        if (data == null) {
            Debug.LogError($"GunshipController ERROR: LoadGunship - {size} data is null");
            return;
        }

        //Since data exists, load in the Scale
        transform.localScale = MasterController.Singleton.prefabs["GunshipTriangle"].transform.localScale * data.Scale;
        PseudoMass = data.Scale;

        //Update health with accurate ratio
        float healthFrac = CurrentHealth / MaxHealth;
        MaxHealth = data.MaxHealth;
        CurrentHealth = data.MaxHealth * healthFrac;

        foreach (CustomPhysics_SO physicsData in data.GetData<CustomPhysics_SO>()) {
            //!! Guard: If the script contains this key, we already added in a physics, so why are we still here?
            if (hasPhysics) {
                Debug.LogError("GunshipController ERROR: Multiple instances of Physics_SO found.");
                break;
            }

            GameObject physicsObj = Instantiate(MasterController.Singleton.prefabs["CustomPhysicsComponent"], transform); //attach physics as a child object
            CustomPhysics physics = physicsObj.GetComponent<CustomPhysics>(); //capture the script

            physics.LoadData(physicsData); //load custom SO data

            components[typeof(CustomPhysics)] = new() { physics }; //track it in the dict; we're guaranteed to only have one so this shorthand works

            if (gameObject.CompareTag("Player")) {
                InputKeys.Add(new InputEntry(
                    name: "Physics_Move",
                    func: physics.ApplyThrustInput,
                    posKeys: new List<KeyCode> { KeyCode.W, KeyCode.UpArrow },
                    negKeys: new List<KeyCode> { KeyCode.S, KeyCode.DownArrow }
                ));

                //Rotate A,Left,D,Right
                InputKeys.Add(new InputEntry(
                    name: "Physics_Rotate",
                    func: physics.ApplyRotationalInput,
                    posKeys: new List<KeyCode> { KeyCode.A, KeyCode.LeftArrow },
                    negKeys: new List<KeyCode> { KeyCode.D, KeyCode.RightArrow }
                ));
            }
        }

        List<WeaponScriptableObject> weapons = data.GetData<WeaponScriptableObject>();
        // Separate chain guns and missile launchers
        List<ChainGun_SO> chainGuns = new();
        List<Missle_SO> missleLaunchers = new();
        foreach (var w in weapons) {
            if (w is ChainGun_SO cg) chainGuns.Add(cg);
            else if (w is Missle_SO ml) missleLaunchers.Add(ml);
        }

        // --- Chain Guns: position along the front/top ---
        for (int i = 0; i < chainGuns.Count; i++) {
            var cgSO = chainGuns[i];
            GameObject obj = Instantiate(MasterController.Singleton.prefabs["ChainGun"], transform);
            ChainGun chainGunScript = obj.GetComponent<ChainGun>();
            chainGunScript.LoadData(cgSO);
            components.TryAdd(typeof(ChainGun), new());
            components[typeof(ChainGun)].Add(chainGunScript);
            if (gameObject.CompareTag("Player")) {
                InputKeys.Add(new InputEntry(
                    name: "Fire_ChainGun",
                    func: chainGunScript.Use,
                    posKeys: new List<KeyCode> { KeyCode.Space, KeyCode.Mouse0 },
                    negKeys: new List<KeyCode> { }
                ));
            }
            // Position along the front (top)
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            Bounds bounds = sr.sprite.bounds;
            float width = bounds.size.x;
            float height = bounds.size.y;
            float step = width / (1 + chainGuns.Count);
            float left = width * -0.5f;
            float posX = left + (step * (i + 1));
            float posY = height * 0.5f;
            obj.transform.localPosition = new Vector2(posX, posY);
        }

        // --- Missile Launchers: position along the back/bottom ---
        for (int i = 0; i < missleLaunchers.Count; i++) {
            var mlSO = missleLaunchers[i];
            GameObject obj = Instantiate(MasterController.Singleton.prefabs["MissleLauncher"], transform);
            MissleLauncher missleLauncher = obj.GetComponent<MissleLauncher>();
            missleLauncher.LoadData(mlSO);
            components.TryAdd(typeof(MissleLauncher), new());
            components[typeof(MissleLauncher)].Add(missleLauncher);
            if (gameObject.CompareTag("Player")) {
                InputKeys.Add(new InputEntry(
                    name: "Fire_Missle",
                    func: missleLauncher.Use,
                    posKeys: new List<KeyCode> { KeyCode.Return, KeyCode.KeypadEnter, KeyCode.Mouse1 },
                    negKeys: new List<KeyCode> { }
                ));
            }
            // Position along the back (bottom)
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            Bounds bounds = sr.sprite.bounds;
            float width = bounds.size.x;
            float height = bounds.size.y;
            float step = width / (1 + missleLaunchers.Count);
            float left = width * -0.5f;
            float posX = left + (step * (i + 1));
            float posY = -height * 0.5f;
            obj.transform.localPosition = new Vector2(posX, posY);
        }

    }

    void Update() {
        if (isDying) {

            //* Dying Animation Goes here *//
            

            return;
        }

        if (gameObject.CompareTag("Player")) {

            if (!MasterController.Singleton.Autoplay) {
                foreach (InputEntry entry in InputKeys) {
                    entry.func(entry.CurrentValue);
                }
            }

            UpdateCamera();
        }
    }

    void UpdateCamera() {
        Camera cam = Camera.main;
        if (cam == null) return;

        float dt = Time.deltaTime;

        // Get velocity info from physics
        float velocityNormalized = 0f;
        Vector3 velocityVector = Vector3.zero;


        if (hasPhysics) {
            if (physics != null) {
                velocityNormalized = physics.VelocityNormalized;
                velocityVector = physics.VelocityVector;
            }
        }

        // Get max chaingun warmup from all chainguns
        float maxChaingunWarmup = 0f;
        if (components.TryGetValue(typeof(ChainGun), out var chaingunList)) {
            foreach (var cg in chaingunList) {
                ChainGun chaingun = cg as ChainGun;
                if (chaingun != null && chaingun.WindupNormalized > maxChaingunWarmup) {
                    maxChaingunWarmup = chaingun.WindupNormalized;
                }
            }
        }

        // Calculate target zoom (orthographic Size)
        float velocityZoom = velocityNormalized * velocityZoomOut;
        float chaingunZoom = maxChaingunWarmup * chaingunZoomOut;
        float targetOrthoSize = baseOrthoSize + velocityZoom + chaingunZoom;

        // Calculate target position with lead
        Vector3 leadOffset = velocityVector * cameraLeadTime;
        Vector3 targetPos = new Vector3(
            transform.position.x + leadOffset.x,
            transform.position.y + leadOffset.y,
            -10f
        );

        // Smoothly interpolate camera
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthoSize, dt * cameraSmoothSpeed);
        cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, dt * cameraSmoothSpeed);
    }

    private void ClearComponentsAndScripts() {
        foreach (Transform childComponent in transform) {
            Destroy(childComponent.gameObject);
        }

        components = new();
    }

    public void TakeDamage(float damage) {
        if (isDying) return;

        CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, MaxHealth);

        if (CurrentHealth <= 0) isDying = true;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if      (CompareTag("Player") && other.CompareTag("EnemyBullet"))   ResolveBulletHit(other);
        else if (CompareTag("Enemy") && other.CompareTag("PlayerBullet"))   ResolveBulletHit(other);
        else if (CompareTag("Player") && other.CompareTag("Enemy"))         ResolveGunshipHit(other);
        else if (other.CompareTag("Rock"))                                  ResolveRockHit(other);
        else if (other.CompareTag("Missle"))                         Helper.ResolveMissleHit(other);
    }

    private void ResolveBulletHit(Collider2D other) {
        TakeDamage(1);
        if (hasPhysics) {
            physics.ApplyKnockback(other.transform.position, 10f);
        }
        Destroy(other.gameObject); // Destroy the bullet on impact
    }

    private void ResolveRockHit(Collider2D other) {
        TakeDamage(1);
        other.GetComponent<Rock>().TakeDamage(100);
    }

    private void ResolveGunshipHit(Collider2D other) {
        TakeDamage(1);
        other.GetComponent<GunshipController>().TakeDamage(1);
    }

}
