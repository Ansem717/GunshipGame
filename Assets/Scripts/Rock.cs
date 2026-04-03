using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour {

    public enum RockSize { Large, Medium, Small }

    [Header("Rock Configuration")]
    public RockSize size = RockSize.Large;

    [Header("Health Settings")]
    [Tooltip("Hits required to destroy a Large rock")]
    public int LargeHealth = 24;
    [Tooltip("Hits required to destroy a Medium rock")]
    public int MediumHealth = 12;
    [Tooltip("Hits required to destroy a Small rock")]
    public int SmallHealth = 6;

    [Header("Visual Settings")]
    [Tooltip("Base radius for Large rocks")]
    public float LargeRadius = 2.0f;
    [Tooltip("Base radius for Medium rocks")]
    public float MediumRadius = 1.2f;
    [Tooltip("Base radius for Small rocks")]
    public float SmallRadius = 0.6f;
    [Tooltip("Number of vertices for the asteroid polygon")]
    [Range(6, 16)]
    public int vertexCount = 10;
    [Tooltip("How jagged the asteroid looks (0 = smooth circle, 1 = very jagged)")]
    [Range(0f, 0.5f)]
    public float jaggedness = 0.3f;
    [Tooltip("Outline color for the rock")]
    public Color outlineColor = new Color(0.25f, 0.15f, 0.1f, 1f); // Dark brown
    [Tooltip("Outline width")]
    public float outlineWidth = 0.08f;

    [Header("Movement Settings")]
    [Tooltip("Random velocity range for spawned rocks")]
    public float minVelocity = 0.5f;
    public float maxVelocity = 2.0f;
    [Tooltip("Random rotation speed range (degrees per second)")]
    public float minRotationSpeed = 10f;
    public float maxRotationSpeed = 60f;

    [Header("Spawn Settings")]
    [Tooltip("Offset distance from parent when spawning child rocks")]
    public float spawnOffset = 0.5f;

    // Runtime state
    private static int nextSortingOrder = 0; // Static counter for unique sorting
    private bool initialized;
    private int currentHealth;
    private float rotationSpeed;
    private Vector2 velocity;
    private int mySortingOrder;

    // Visual components
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private PolygonCollider2D polyCollider;
    private LineRenderer lineRenderer;

    void Awake() {
        // Ensure we have necessary components
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();

        polyCollider = GetComponent<PolygonCollider2D>();
        if (polyCollider == null) polyCollider = gameObject.AddComponent<PolygonCollider2D>();

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null) lineRenderer = gameObject.AddComponent<LineRenderer>();

        // Rigidbody2D required for trigger detection, but Kinematic won't interfere with manual movement
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void Start() {
        if (!initialized) Initialize(size);
    }

    /// <summary>
    /// Initialize the rock with a specific size. Call this when spawning programmatically.
    /// </summary>
    public void Initialize(RockSize rockSize, Vector2? initialVelocity = null) {
        initialized = true;
        size = rockSize;
        currentHealth = GetHealthForSize(size);

        // Assign unique sorting order so overlapping rocks layer properly
        mySortingOrder = nextSortingOrder;
        nextSortingOrder += 2; // Reserve 2 slots: one for fill, one for outline

        // Set up random movement
        if (initialVelocity.HasValue) {
            velocity = initialVelocity.Value;
        } else {
            float speed = Random.Range(minVelocity, maxVelocity);
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
        }
        rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed) * (Random.value > 0.5f ? 1f : -1f);

        // Generate the visual mesh
        GenerateAsteroidMesh();

        // Set up default material if none exists
        if (meshRenderer.sharedMaterial == null) {
            meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
            meshRenderer.material.color = new Color(0.5f, 0.45f, 0.4f); // Grayish-brown rock color
        }

        // Tag for identification
        gameObject.tag = "Rock";
    }

    void Update() {
        if (MasterController.Singleton.Paused) return;

        float dt = Time.deltaTime;

        // Apply movement
        transform.position += (Vector3)velocity * dt;

        // Apply rotation
        transform.Rotate(0, 0, rotationSpeed * dt);
    }

    /// <summary>
    /// Generate a procedural asteroid mesh with irregular edges.
    /// </summary>
    private void GenerateAsteroidMesh() {
        float baseRadius = GetRadiusForSize(size);
        Vector2[] points = new Vector2[vertexCount];
        Vector3[] vertices = new Vector3[vertexCount + 1]; // +1 for center
        int[] triangles = new int[vertexCount * 3];

        // Generate random radii for each vertex
        float[] radii = new float[vertexCount];
        for (int i = 0; i < vertexCount; i++) {
            radii[i] = baseRadius * (1f - jaggedness + Random.Range(0f, jaggedness * 2f));
        }

        // Center vertex
        vertices[0] = Vector3.zero;

        // Generate vertices around the perimeter
        for (int i = 0; i < vertexCount; i++) {
            float angle = (i / (float)vertexCount) * Mathf.PI * 2f;
            float radius = radii[i];

            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            points[i] = new Vector2(vertices[i + 1].x, vertices[i + 1].y);
        }

        // Generate triangles (fan from center)
        for (int i = 0; i < vertexCount; i++) {
            int triIndex = i * 3;
            triangles[triIndex] = 0; // center
            triangles[triIndex + 1] = i + 1;
            triangles[triIndex + 2] = (i + 1) % vertexCount + 1;
        }

        // Create mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        // Set sorting order for mesh
        meshRenderer.sortingOrder = mySortingOrder;

        // Set up collider
        polyCollider.SetPath(0, points);
        polyCollider.isTrigger = true; // Use trigger for bullet detection

        // Set up outline
        lineRenderer.positionCount = vertexCount + 1; // +1 to close the loop
        lineRenderer.loop = false;
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = outlineWidth;
        lineRenderer.endWidth = outlineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = outlineColor;
        lineRenderer.endColor = outlineColor;
        lineRenderer.sortingOrder = mySortingOrder + 1; // Render above this rock's mesh

        for (int i = 0; i < vertexCount; i++) {
            lineRenderer.SetPosition(i, vertices[i + 1]);
        }
        lineRenderer.SetPosition(vertexCount, vertices[1]); // Close the loop
    }

    /// <summary>
    /// Called when a bullet hits this rock.
    /// </summary>
    public void TakeDamage(int damage = 1) {
        currentHealth -= damage;

        if (currentHealth <= 0) {
            DestroyRock();
        }
    }

    /// <summary>
    /// Handle rock destruction and spawn child rocks.
    /// </summary>
    private void DestroyRock() {
        SpawnChildRocks();
        Destroy(gameObject);
    }

    /// <summary>
    /// Spawn child rocks based on the current rock size.
    /// </summary>
    private void SpawnChildRocks() {
        switch (size) {
            case RockSize.Large:
                SpawnLargeRockChildren();
                break;
            case RockSize.Medium:
                SpawnMediumRockChildren();
                break;
            case RockSize.Small:
                // Small rocks spawn nothing
                break;
        }
    }

    /// <summary>
    /// Large rocks spawn 6 "value" worth of rocks.
    /// Medium = 2 value, Small = 1 value.
    /// </summary>
    private void SpawnLargeRockChildren() {
        int remainingValue = 6;
        List<RockSize> toSpawn = new List<RockSize>();

        // Randomly distribute value among medium (2) and small (1) rocks
        while (remainingValue > 0) {
            if (remainingValue >= 2 && Random.value > 0.4f) {
                // 60% chance to spawn a medium if we have enough value
                toSpawn.Add(RockSize.Medium);
                remainingValue -= 2;
            } else {
                // Spawn a small
                toSpawn.Add(RockSize.Small);
                remainingValue -= 1;
            }
        }

        SpawnRocksFromList(toSpawn);
    }

    /// <summary>
    /// Medium rocks spawn 0-3 small rocks.
    /// </summary>
    private void SpawnMediumRockChildren() {
        int count = Random.Range(0, 4); // 0, 1, 2, or 3
        List<RockSize> toSpawn = new List<RockSize>();

        for (int i = 0; i < count; i++) {
            toSpawn.Add(RockSize.Small);
        }

        SpawnRocksFromList(toSpawn);
    }

    /// <summary>
    /// Spawn rocks from a list, distributing them in different directions.
    /// </summary>
    private void SpawnRocksFromList(List<RockSize> rocks) {
        int count = rocks.Count;
        if (count == 0) return;

        for (int i = 0; i < count; i++) {
            // Calculate spawn angle to spread rocks evenly
            float angle = (i / (float)count) * Mathf.PI * 2f + Random.Range(-0.3f, 0.3f);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // Spawn position offset from center
            Vector3 spawnPos = transform.position + (Vector3)(direction * spawnOffset);

            // Create new rock
            GameObject newRockObj = new GameObject($"Rock_{rocks[i]}");
            newRockObj.transform.position = spawnPos;

            Rock newRock = newRockObj.AddComponent<Rock>();

            // Copy settings
            newRock.LargeHealth = LargeHealth;
            newRock.MediumHealth = MediumHealth;
            newRock.SmallHealth = SmallHealth;
            newRock.LargeRadius = LargeRadius;
            newRock.MediumRadius = MediumRadius;
            newRock.SmallRadius = SmallRadius;
            newRock.vertexCount = vertexCount;
            newRock.jaggedness = jaggedness;
            newRock.minVelocity = minVelocity;
            newRock.maxVelocity = maxVelocity;
            newRock.minRotationSpeed = minRotationSpeed;
            newRock.maxRotationSpeed = maxRotationSpeed;
            newRock.spawnOffset = spawnOffset;

            // Calculate velocity - inherit some parent velocity plus outward push
            Vector2 inheritedVelocity = velocity * 0.5f;
            Vector2 outwardVelocity = direction * Random.Range(minVelocity, maxVelocity);
            newRock.Initialize(rocks[i], inheritedVelocity + outwardVelocity);
        }
    }

    /// <summary>
    /// Handle trigger collision with bullets.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Bullet")) {
            TakeDamage(1);
            Destroy(other.gameObject); // Destroy the bullet on impact
        }
    }

    /// <summary>
    /// Get health value for a given rock size.
    /// </summary>
    private int GetHealthForSize(RockSize rockSize) {
        return rockSize switch {
            RockSize.Large => LargeHealth,
            RockSize.Medium => MediumHealth,
            RockSize.Small => SmallHealth,
            _ => SmallHealth
        };
    }

    /// <summary>
    /// Get base radius for a given rock size.
    /// </summary>
    private float GetRadiusForSize(RockSize rockSize) {
        return rockSize switch {
            RockSize.Large => LargeRadius,
            RockSize.Medium => MediumRadius,
            RockSize.Small => SmallRadius,
            _ => SmallRadius
        };
    }

    /// <summary>
    /// Get remaining health percentage (0-1).
    /// </summary>
    public float GetHealthPercentage() {
        return (float)currentHealth / GetHealthForSize(size);
    }

    /// <summary>
    /// Get current health.
    /// </summary>
    public int GetCurrentHealth() {
        return currentHealth;
    }
}
