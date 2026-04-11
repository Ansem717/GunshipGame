using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChainGunCone : MonoBehaviour {

    [Header("Appearance")]
    public Color coneColor = Color.white;
    public float edgeAlpha = 0.7f;
    public float centerAlpha = 0.3f;
    public float coneLength = 3f;
    [Range(0f, 1f)]
    [Tooltip("How much of the cone width is 'edge' vs 'center'")]
    public float edgeThickness = 0.15f;
    [Range(1f, 20f)]
    [Tooltip("Exponential decay rate. Higher = faster initial fade, longer tail.")]
    public float falloffRate = 8f;
    [Tooltip("Number of segments for smooth exponential curve")]
    public int segments = 10;

    private Mesh mesh;
    private MeshFilter meshFilter;
    private float opacity = 1f;
    private float lastDeviation;
    private int lastSegments;

    void Awake() {
        meshFilter = GetComponent<MeshFilter>();
    }

    void CreateMesh() {
        mesh = new Mesh();
        mesh.name = "ConeMesh";
        meshFilter.mesh = mesh;
        lastSegments = segments;
    }

    public void SetDeviation(float deviationDegrees) {
        if (mesh == null || lastSegments != segments) {
            CreateMesh();
        }

        lastDeviation = deviationDegrees;
        float halfAngle = deviationDegrees * Mathf.Deg2Rad;

        // 1 tip vertex + 4 vertices per segment row
        int vertCount = 1 + (segments * 4);
        Vector3[] vertices = new Vector3[vertCount];
        Color[] colors = new Color[vertCount];

        // Tip vertex
        vertices[0] = Vector3.zero;
        colors[0] = new Color(coneColor.r, coneColor.g, coneColor.b, edgeAlpha * opacity);

        // Segment rows
        for (int s = 1; s <= segments; s++) {
            float t = (float)s / segments;
            float y = t * coneLength;
            float halfWidth = Mathf.Tan(halfAngle) * y;
            float innerHalfWidth = halfWidth * (1f - edgeThickness);

            int baseIdx = 1 + (s - 1) * 4;
            vertices[baseIdx + 0] = new Vector3(-halfWidth, y, 0);
            vertices[baseIdx + 1] = new Vector3(-innerHalfWidth, y, 0);
            vertices[baseIdx + 2] = new Vector3(innerHalfWidth, y, 0);
            vertices[baseIdx + 3] = new Vector3(halfWidth, y, 0);

            // Exponential decay: e^(-rate * t)
            float alphaFactor = Mathf.Exp(-falloffRate * t);

            Color edgeColor = new Color(coneColor.r, coneColor.g, coneColor.b, edgeAlpha * alphaFactor * opacity);
            Color centerColor = new Color(coneColor.r, coneColor.g, coneColor.b, centerAlpha * alphaFactor * opacity);

            colors[baseIdx + 0] = edgeColor;
            colors[baseIdx + 1] = centerColor;
            colors[baseIdx + 2] = centerColor;
            colors[baseIdx + 3] = edgeColor;
        }

        // Build triangles
        // First row: fan from tip to first segment
        // Subsequent rows: quads between segment rows
        int triCount = 4 * 3 + (segments - 1) * 4 * 6; // first fan (4 tris) + remaining quads (4 quads × 6 indices each)
        int[] triangles = new int[triCount];
        int triIdx = 0;

        // Fan from tip (vertex 0) to first segment row (vertices 1-4)
        triangles[triIdx++] = 0; triangles[triIdx++] = 1; triangles[triIdx++] = 2;
        triangles[triIdx++] = 0; triangles[triIdx++] = 2; triangles[triIdx++] = 3;
        triangles[triIdx++] = 0; triangles[triIdx++] = 3; triangles[triIdx++] = 4;
        triangles[triIdx++] = 0; triangles[triIdx++] = 4; triangles[triIdx++] = 1; // close the fan

        // Quads between subsequent rows
        for (int s = 1; s < segments; s++) {
            int row0 = 1 + (s - 1) * 4;
            int row1 = 1 + s * 4;

            for (int i = 0; i < 4; i++) {
                int next = (i + 1) % 4;
                triangles[triIdx++] = row0 + i;
                triangles[triIdx++] = row1 + i;
                triangles[triIdx++] = row1 + next;
                triangles[triIdx++] = row0 + i;
                triangles[triIdx++] = row1 + next;
                triangles[triIdx++] = row0 + next;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    public void SetOpacity(float newOpacity) {
        opacity = Mathf.Clamp01(newOpacity);
        if (mesh != null) {
            SetDeviation(lastDeviation);
        }
    }

    public void SetVisible(bool visible) {
        GetComponent<MeshRenderer>().enabled = visible;
    }
}
