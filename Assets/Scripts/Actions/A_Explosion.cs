using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class A_Explosion : ActionInterface {

    // Create a circle and apply contact damage to Rocks and Gunships.

    private SpriteRenderer explosionSR;
    private GameObject explosionObj;
    private readonly System.Collections.Generic.HashSet<Collider2D> alreadyHit = new();

    //data
    private bool DestroySource;
    private float ExplosionRadius;
    private float ExplosionDamage;
    private Vector3 SpawnPosition;
    private bool RandomizePos;

    public A_Explosion(float radius, float duration, float damage, bool DestroySource, bool RandomizePos = false)
        : base(_duration: duration, _blocking: true) {
        name = "Explosion";
        this.DestroySource = DestroySource;
        ExplosionRadius = radius;
        ExplosionDamage = damage;
        this.RandomizePos = RandomizePos;
    }

    public override bool Init() {
        explosionObj = new GameObject("Explosion");
        explosionObj.transform.position = objRef.transform.position; //default
        //explosionObj.transform.SetParent(objRef.transform);

        explosionSR = explosionObj.AddComponent<SpriteRenderer>();
        explosionSR.sprite = MakeCircleSprite(64);
        explosionSR.color = new Color(1f, 0.5f, 0f, 1f);

        if (RandomizePos) {
            SpriteRenderer sr = objRef.GetComponent<SpriteRenderer>();

            Bounds b = sr.bounds;

            float posX = Random.Range(b.min.x, b.max.x);
            float posY = Random.Range(b.min.y, b.max.y);

            SpawnPosition = new Vector2(posX, posY);
        } else {
            SpawnPosition = objRef.transform.position;
        }


        if (DestroySource) {
            // Hide the source sprite during explosion animation.
            SpriteRenderer sr = objRef.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }

        return true;
    }

    public override void IUpdate(float dt) {
        float progress = GetProgress();
        float radius = Mathf.Lerp(0f, ExplosionRadius, progress);
        float alpha = 1f - progress;

        explosionSR.color = new Color(1f, 0.5f, 0f, alpha);
        explosionObj.transform.localScale = Vector3.one * radius * 2f;
        explosionObj.transform.position = SpawnPosition; //maintain original position even if parent moves

        if (ExplosionDamage > 0) ApplyExplosionDamage(radius);

        if (elapsed >= duration) {
            State = ActionState.Done;
        }

        if (objRef == null && explosionObj != null) {
            Object.Destroy(explosionObj);
        }
    }

    private void ApplyExplosionDamage(float radius) {
        Collider2D[] hits = Physics2D.OverlapCircleAll(explosionObj.transform.position, radius);

        foreach (Collider2D hit in hits) {
            if (hit.gameObject == objRef) continue;
            if (alreadyHit.Contains(hit)) continue;
            alreadyHit.Add(hit);

            if (hit.TryGetComponent(out Rock rock)) {
                rock.TakeDamage(ExplosionDamage);
            }

            if (hit.TryGetComponent(out GunshipController gunship)) {
                gunship.TakeDamage(ExplosionDamage);
            }
        }
    }

    public override void Exit() {
        if (DestroySource) {
            Object.Destroy(objRef);
        }
        //if (explosionObj != null) 
            Object.Destroy(explosionObj);
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

    public override void PostWait() { }

   
}
