using UnityEngine;

public class StarBehavior : MonoBehaviour {

    private SpaceSystem spaaacee;

    private void Start() {
        spaaacee = FindFirstObjectByType<SpaceSystem>();
    }

    void Update() {
        if (!Helper.IsInsideViewport(transform.position)) {
            spaaacee.RemoveStar(gameObject);
            Destroy(gameObject);
        }
    }
}
