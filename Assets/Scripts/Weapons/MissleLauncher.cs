using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MissleLauncher : MonoBehaviour {

    [Header("Launcher")]
    [Min(1)]
    [Tooltip("Maximum missles the launcher can hold")]
    public int MaxStored = 3;
    [Min(0.1f)]
    [Tooltip("Seconds to recharge one missle")]
    public float RechargeTime = 4f;
    [Min(0.05f)]
    [Tooltip("Seconds between each missle drop while firing")]
    public float DropoffInterval = 0.3f;

    // Missle SO data to pass to each spawned missle
    private Missle_SO missleData;

    // Runtime state
    private int currentStored;
    private float rechargeElapsed;
    private float dropoffElapsed;
    private int misslesToDrop;  // remaining missles in the current salvo
    private CustomPhysics shipPhysics;

    // HUD dots
    private GridLayoutGroup MissleGridUI;

    // Reload particle system
    private ParticleSystem reloadParticles;

    /// <summary>
    /// Current missle count normalized (0-1) for UI.
    /// </summary>
    public float StoredNormalized => (float)CurrentStored / MaxStored;
    public bool IsPlayer => transform.parent.CompareTag("Player");

    public int CurrentStored { 
        get => currentStored; 
        set {
            currentStored = value;
            if (IsPlayer) UpdateHUD();
        }
    }

    void Start() {
        CurrentStored = MaxStored; // Start fully loaded
        rechargeElapsed = 0f;
        dropoffElapsed = 0f;
        misslesToDrop = 0;
        shipPhysics = transform.parent.GetComponentInChildren<CustomPhysics>();

        CreateReloadParticles();

        if (IsPlayer) {
            //find hud
            MissleGridUI = FindFirstObjectByType<GridLayoutGroup>();
        }
    }

    public void LoadData(Missle_SO data) {
        missleData = data;
        MaxStored = data.MaxStored;
        RechargeTime = data.RechargeTime;
        DropoffInterval = data.DropoffInterval;

        if (IsPlayer) {
            //Assert HUD
            MissleGridUI ??= FindFirstObjectByType<GridLayoutGroup>();

            //Set Scaling
            MissleGridUI.cellSize = data.CellSize;
            MissleGridUI.spacing = data.CellSpacing;
            MissleGridUI.padding = data.GridPadding;
            
            // Update Icon Count
            for (int i = 0; i < MissleGridUI.transform.childCount; i++) {
                bool shouldBeActive = i < MaxStored;
                MissleGridUI.transform.GetChild(i).gameObject.SetActive(shouldBeActive);
            }
        }

        CurrentStored = MaxStored; 
    }

    void Update() {
        if (MasterController.Singleton.Paused) return;

        float dt = Time.deltaTime;
        int prevStored = CurrentStored;

        // Recharge when not actively dropping missles
        if (misslesToDrop <= 0 && CurrentStored < MaxStored) {
            rechargeElapsed += dt;
            if (rechargeElapsed >= RechargeTime) {
                rechargeElapsed = 0f;
                CurrentStored++;
            }
        }

        // Dropoff: release all stored missles one at a time at DropoffInterval
        if (misslesToDrop > 0) {
            dropoffElapsed += dt;
            if (dropoffElapsed >= DropoffInterval) {
                dropoffElapsed -= DropoffInterval;
                SpawnMissle();
                misslesToDrop--;
            }
        }

        // Reload particle + HUD update on count change
        if (CurrentStored != prevStored) {
            if (CurrentStored > prevStored && reloadParticles != null) {
                reloadParticles.Play();
            }
        }
    }

    void SpawnMissle() {
        if (CurrentStored <= 0) return;
        CurrentStored--;
        rechargeElapsed = 0f; // Reset recharge timer on each drop

        GameObject missle = Instantiate(
            MasterController.Singleton.prefabs["Missle"],
            transform.position,
            transform.parent.rotation
        );

        missle.GetComponent<MissleModel>().Data = missleData;

        // Give the missle an initial velocity matching the ship so it doesn't fall behind
        // Plus a random sideways kick perpendicular to the ship's facing
        CustomPhysics misslePhysics = missle.GetComponentInChildren<CustomPhysics>();
        if (misslePhysics != null) {
            if (missleData.PhysicsData != null) {
                misslePhysics.LoadData(missleData.PhysicsData);
            }

            if (shipPhysics != null) {
                misslePhysics.ApplyImpulse(shipPhysics.VelocityVector);
            }

            Vector2 lateral = transform.parent.right * (Random.value > 0.5f ? 1f : -1f);
            misslePhysics.ApplyImpulse(lateral * missleData.SidewaysKick);
        }

        // Push missile actions directly onto its ActionList
        if (missle.TryGetComponent(out ActionList mal)) {
            mal.PushBack(new A_MissleSearch(missleData, transform.parent.tag));
        }
    }

    /// <summary>
    /// Called once on key press. Dumps all stored missles as a salvo.
    /// </summary>
    public void Use(float input) {
        if (input <= 0) return;
        if (misslesToDrop > 0) return; // Already dropping
        if (CurrentStored <= 0) return; // Nothing to fire

        // Immediately drop the first missle, queue the rest
        misslesToDrop = CurrentStored - 1;
        dropoffElapsed = 0f;
        rechargeElapsed = 0f;
        SpawnMissle();
    }

    void CreateReloadParticles() {
        GameObject psObj = new GameObject("MissleReloadParticles");
        psObj.transform.SetParent(transform, false);

        reloadParticles = psObj.AddComponent<ParticleSystem>();
        reloadParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = reloadParticles.main;
        main.duration = 0.3f;
        main.startLifetime = 0.4f;
        main.startSpeed = 1.5f;
        main.startSize = 0.06f;
        main.startColor = new Color(0.2f, 1f, 0.3f, 1f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false;
        main.playOnAwake = false;

        var emission = reloadParticles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 8) });

        var shape = reloadParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.05f;

        var colorOverLifetime = reloadParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(0.2f, 1f, 0.3f), 0f), new GradientColorKey(new Color(0.2f, 1f, 0.3f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = grad;

        var renderer = psObj.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    void UpdateHUD() {
        if (!IsPlayer) return; 

        MissleGridUI ??= FindFirstObjectByType<GridLayoutGroup>();

        if (MissleGridUI == null) return;

        // Update Icon Count
        for (int i = 0; i < MaxStored; i++) {
            var icon = MissleGridUI.transform.GetChild(i);
            if (icon == null) continue;
            if (!icon.gameObject.activeSelf) continue;

            var img = icon.GetComponent<Image>();
            if (img == null) continue;
            
            img.color = i < CurrentStored ? Color.green : new Color(0.2f, 0.2f, 0.2f, 0.5f);
        }
    }

}
