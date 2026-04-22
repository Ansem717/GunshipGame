using UnityEngine;

public class A_MissleSearch : ActionInterface {

    // Expands a gradient circle from the missle.
    // After maxScans expansions, if fail to find, pushfront A_MissleExplosion.
    // If a target is found during scan, push A_SeekTarget then A_MissleExplosion.

    public readonly Missle_SO data;
    private readonly string spawnerTag;

    private int currentScan;
    private float scanElapsed;
    private float currentPulseRadius;

    private LineRenderer pulseRing;
    private GameObject pulseObj;

    private const int CircleSegments = 64;

    public A_MissleSearch(Missle_SO data, string spawnerTag)
        : base(_duration: float.MaxValue, _delay: data.ArmingTime, _blocking: true) {
        name = "MissleSearch";
        this.data = data;
        this.spawnerTag = spawnerTag;
    }

    public override bool Init() {
        currentScan = 0;
        scanElapsed = 0f;
        currentPulseRadius = 0f;

        pulseObj = new GameObject("MissleSearch_Pulse");
        // Parent the pulse to the missile so it is destroyed with it
        pulseObj.transform.SetParent(objRef.transform, false);
        // Counteract missile scale so pulse always appears at world scale
        Vector3 invScale = new Vector3(
            1f / objRef.transform.lossyScale.x,
            1f / objRef.transform.lossyScale.y,
            1f / objRef.transform.lossyScale.z
        );
        pulseObj.transform.localScale = invScale;
        pulseRing = pulseObj.AddComponent<LineRenderer>();
        pulseRing.positionCount = CircleSegments + 1;
        pulseRing.useWorldSpace = true;
        pulseRing.loop = false;
        pulseRing.startWidth = 0.25f;
        pulseRing.endWidth = 0.25f;
        pulseRing.material = new Material(Shader.Find("Sprites/Default"));
        pulseObj.SetActive(false);

        return true;
    }

    public override void PostWait() {
        pulseObj.SetActive(true);
        scanElapsed = 0f;
        currentPulseRadius = 0f;
    }

    public override void IUpdate(float dt) {
        scanElapsed += dt;
        float pulseProgress = Mathf.Clamp01(scanElapsed / data.ScanInterval);
        currentPulseRadius = Mathf.Lerp(0f, data.ScanRadius, pulseProgress);

        UpdatePulseVisual();

        Transform found = ScanForTarget(currentPulseRadius);
        if (found != null) {
            A_SeekTarget seek = new A_SeekTarget(found);
            seek.blocking = true;
            owner.PushBack(seek);
            owner.PushBack(new A_MissleExplosion(data));

            State = ActionState.Done;
            return;
        }

        if (pulseProgress >= 1f) {
            currentScan++;

            if (currentScan >= data.MaxScans) {
                owner.PushBack(new A_MissleExplosion(data));
                State = ActionState.Done;
                return;
            }

            scanElapsed = 0f;
            currentPulseRadius = 0f;
        }
    }

    private Transform ScanForTarget(float radius) {
        Collider2D[] hits = Physics2D.OverlapCircleAll(objRef.transform.position, radius);

        float closestDist = float.MaxValue;
        Transform closest = null;

        foreach (Collider2D hit in hits) {
            if (hit.gameObject == objRef) continue;
            if (hit.CompareTag(spawnerTag)) continue;
            if (!hit.CompareTag("Rock") && !hit.CompareTag("Player") && !hit.CompareTag("Enemy")) continue;

            float dist = Vector3.Distance(objRef.transform.position, hit.transform.position);
            if (dist < closestDist) {
                closestDist = dist;
                closest = hit.transform;
            }
        }

        return closest;
    }

    private void UpdatePulseVisual() {
        float alpha = 1f - (currentPulseRadius / data.ScanRadius);
        pulseRing.startColor = new Color(1f, 0.5f, 1f, alpha);
        pulseRing.endColor = new Color(1f, 0.5f, 1f, alpha * 0.5f);

        for (int i = 0; i <= CircleSegments; i++) {
            float angle = (i / (float)CircleSegments) * Mathf.PI * 2f;
            Vector3 point = objRef.transform.position + new Vector3(
                Mathf.Cos(angle) * currentPulseRadius,
                Mathf.Sin(angle) * currentPulseRadius,
                0f
            );
            pulseRing.SetPosition(i, point);
        }
    }

    public override void Exit() {
        if (pulseObj != null) Object.Destroy(pulseObj);
    }

}