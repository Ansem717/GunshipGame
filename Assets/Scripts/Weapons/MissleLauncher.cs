using UnityEngine;

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
    private UnityEngine.UI.Image[] dotImages;
    private GameObject hudContainer;
    private static Sprite dotSprite;
    private const float DotSize = 12f;
    private const float DotSpacing = 16f;
    private const float HudMarginRight = 20f;
    private const float HudMarginBottom = 20f;

    // Reload particle system
    private ParticleSystem reloadParticles;

    /// <summary>
    /// Current missle count normalized (0-1) for UI.
    /// </summary>
    public float StoredNormalized => (float)currentStored / MaxStored;
    public int CurrentStored => currentStored;

    void Start() {
        currentStored = MaxStored; // Start fully loaded
        rechargeElapsed = 0f;
        dropoffElapsed = 0f;
        misslesToDrop = 0;
        shipPhysics = transform.parent.GetComponentInChildren<CustomPhysics>();

        CreateReloadParticles();

        if (transform.parent.CompareTag("Player")) {
            CreateHUDDots();
            UpdateHUDDots();
        }
    }

    public void LoadData(Missle_SO data) {
        missleData = data;
        MaxStored = data.MaxStored;
        RechargeTime = data.RechargeTime;
        DropoffInterval = data.DropoffInterval;
        currentStored = MaxStored;
    }

    void Update() {
        if (MasterController.Singleton.Paused) return;

        float dt = Time.deltaTime;
        int prevStored = currentStored;

        // Recharge when not actively dropping missles
        if (misslesToDrop <= 0 && currentStored < MaxStored) {
            rechargeElapsed += dt;
            if (rechargeElapsed >= RechargeTime) {
                rechargeElapsed = 0f;
                currentStored++;
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
        if (currentStored != prevStored) {
            UpdateHUDDots();
            if (currentStored > prevStored && reloadParticles != null) {
                reloadParticles.Play();
            }
        }
    }

    void SpawnMissle() {
        if (currentStored <= 0) return;
        currentStored--;
        rechargeElapsed = 0f; // Reset recharge timer on each drop

        GameObject missle = Instantiate(
            MasterController.Singleton.prefabs["Missle"],
            transform.position,
            transform.parent.rotation
        );

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
        if (currentStored <= 0) return; // Nothing to fire

        // Immediately drop the first missle, queue the rest
        misslesToDrop = currentStored - 1;
        dropoffElapsed = 0f;
        rechargeElapsed = 0f;
        SpawnMissle();
        UpdateHUDDots();
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

    void CreateHUDDots() {
        if (dotSprite == null) {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size * 0.5f;
            float rSq = center * center;
            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    tex.SetPixel(x, y, dx * dx + dy * dy <= rSq ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            dotSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        // Create a canvas on the camera for screen-space HUD
        Camera cam = Camera.main;
        hudContainer = new GameObject("MissleHUD");
        Canvas canvas = hudContainer.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        dotImages = new UnityEngine.UI.Image[MaxStored];
        float totalWidth = (MaxStored - 1) * DotSpacing;

        for (int i = 0; i < MaxStored; i++) {
            GameObject dot = new GameObject($"MissleDot_{i}");
            dot.transform.SetParent(hudContainer.transform, false);

            RectTransform rt = dot.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(DotSize, DotSize);
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(
                -HudMarginRight - totalWidth + i * DotSpacing,
                HudMarginBottom
            );

            UnityEngine.UI.Image img = dot.AddComponent<UnityEngine.UI.Image>();
            img.sprite = dotSprite;
            img.color = new Color(0.2f, 1f, 0.3f, 0.9f);
            dotImages[i] = img;
        }
    }

    void UpdateHUDDots() {
        if (dotImages == null) return;
        for (int i = 0; i < dotImages.Length; i++) {
            dotImages[i].color = i < currentStored
                ? new Color(0.2f, 1f, 0.3f, 0.9f)
                : new Color(0.2f, 0.2f, 0.2f, 0.3f);
        }
    }

    void OnDestroy() {
        if (hudContainer != null) Destroy(hudContainer);
    }

}
