using UnityEngine;

public class A_MissleExplosion : ActionInterface {

    // Explode the missle.
    // Create a circle and apply contact damage to Rocks and Gunships.
    // Destroy missle gameobject.

    public readonly Missle_SO data;

    private SpriteRenderer explosionSR;
    private GameObject explosionObj;
    private readonly System.Collections.Generic.HashSet<Collider2D> alreadyHit = new();

    public A_MissleExplosion(Missle_SO data)
        : base(_duration: data.ExplosionDuration, _blocking: true) {
        name = "MissleExplosion";
        this.data = data;
    }

    public override bool Init() {
        explosionObj = new GameObject("MissleExplosion_Circle");
        Object.DontDestroyOnLoad(explosionObj);

        explosionSR = explosionObj.AddComponent<SpriteRenderer>();
        explosionSR.sprite = MakeCircleSprite(64);
        explosionSR.color = new Color(1f, 0.5f, 0f, 1f);

        // Hide the missle sprite during explosion animation.
        SpriteRenderer sr = objRef.GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        return true;
    }

    public override void IUpdate(float dt) {
        float progress = GetProgress();
        float radius = Mathf.Lerp(0f, data.ExplosionRadius, progress);
        float alpha = 1f - progress;

        explosionSR.color = new Color(1f, 0.5f, 0f, alpha);
        explosionObj.transform.position = objRef.transform.position;
        explosionObj.transform.localScale = Vector3.one * radius * 2f;

        ApplyExplosionDamage(radius);

        if (elapsed >= duration) {
            State = ActionState.Done;
        }
    }

    private void ApplyExplosionDamage(float radius) {
        Collider2D[] hits = Physics2D.OverlapCircleAll(objRef.transform.position, radius);

        foreach (Collider2D hit in hits) {
            if (hit.gameObject == objRef) continue;
            if (alreadyHit.Contains(hit)) continue;
            alreadyHit.Add(hit);

            if (hit.TryGetComponent(out Rock rock)) {
                rock.TakeDamage((int)data.ExplosionDamage);
            }

            if (hit.TryGetComponent(out GunshipController gunship)) {
                gunship.TakeDamage(data.ExplosionDamage);
            }
        }
    }

    public override void Exit() {
        if (explosionObj != null) Object.Destroy(explosionObj);
        // Destroy all children of the missile root (including CustomPhysicsComponent)
        if (owner != null && owner.gameObject != null) {
            foreach (Transform child in owner.gameObject.transform) {
                Object.Destroy(child.gameObject);
            }
            Object.Destroy(owner.gameObject);
        }
    }

    private static Sprite MakeCircleSprite(int size) {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size * 0.5f;
        float radiusSq = center * center;

        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                float dx = x - center + 0.5f;
                float dy = y - center + 0.5f;
                tex.SetPixel(x, y, dx * dx + dy * dy <= radiusSq ? Color.white : Color.clear);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public override void PostWait() {}
}
