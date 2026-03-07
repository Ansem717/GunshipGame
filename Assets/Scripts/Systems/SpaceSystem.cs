//---------------------------------------------------------
// file:	SpaceSystem.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	NPC Director. Manages outer space, NPCs, asteroids, and other objects.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

public class SpaceSystem : MonoBehaviour {

    public GameObject starPrefab;
    public int starCount;
    public List<GameObject> activeStars;

    void Start() {
        activeStars = new();

        for (int i = 0; i < starCount; i++) {
            activeStars.Add(BuildStar(true));
        }

    }

    void Update() {
        if (activeStars.Count < starCount) {
            activeStars.Add(BuildStar(false));
        }
    }

    public GameObject BuildStar(bool onScreen) {

        //Assume onScreen is true.

        Camera cam = Camera.main;
        Vector3 min = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 max = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

        if (!onScreen) {
            //1. get direction from velocity
            //2. use direction to determine which edges to spawn near
            //2b. probably flip a coin to determine edge
            //3. use velocity + camera zoom to determine how far away to spawn
            //4. set min and max within proper bounds
        }

        float rx = Random.Range(min.x, max.x);
        float ry = Random.Range(min.y, max.y);

        Vector3 pos = new Vector3(rx, ry, cam.nearClipPlane);

        GameObject star = Instantiate(starPrefab, pos, Quaternion.identity);
        star.transform.localScale *= Random.value;

        if (star.TryGetComponent(out ActionList star_al)) {
            star_al.PushFront(new A_Callback(
                action: new A_StarController(), 
                callback: () => {
                    activeStars.Remove(star);
                    Destroy(star);
                    MasterController.Singleton.actionListsDirty = true;
                }
            ));
        }
        return star;
    }

}
