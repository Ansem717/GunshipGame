using UnityEngine;
using UnityEngine.UI;

public class ChainGunIndicatorScript : MonoBehaviour {

    [Header("References")]
    public Image donutImage;
    public ParticleSystem particles;

    [Header("Settings")]
    public Color startColor = new Color(0f, 0.5f, 1f, 1f); // Blue
    public Color endColor = new Color(1f, 0f, 0f, 1f);     // Red
    public float shakeIntensity = 5f;

    private RectTransform donutRect;
    private Vector2 basePosition;

    private bool DidSparksTrigger;

    void Awake() {
        if (donutImage != null) {
            donutRect = donutImage.GetComponent<RectTransform>();
            basePosition = donutRect.anchoredPosition;
        }
        DidSparksTrigger = false;
    }

    /// <summary>
    /// Updates the indicator visuals based on normalized windup (0-1).
    /// </summary>
    public void SetWindup(float normalized) {
        if (donutImage == null) return;

        // Fill
        donutImage.fillAmount = normalized;

        // Color
        donutImage.color = Color.Lerp(startColor, endColor, normalized);

        // Shake
        Vector2 shakeOffset = Random.insideUnitCircle * shakeIntensity * normalized * normalized; //multiply by normalized squared so the shake is less intense early on.
        donutRect.anchoredPosition = basePosition + shakeOffset;

        // Sparks
        if (particles == null) return;

        if (!DidSparksTrigger && normalized > 0.99f) {
            DidSparksTrigger = true;
            particles.Play();
        }

        if (DidSparksTrigger && normalized < 0.75f) {
            DidSparksTrigger = false;
        }
    }
}
