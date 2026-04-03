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

    public int starCount;
    public int rockCount;
    private List<GameObject> activeStars;
    private List<GameObject> activeRocks;

    void Start() {
        activeStars = new();
        activeRocks = new();

        for (int i = 0; i < starCount; i++) {
            activeStars.Add(Build("Star", true));
        }

        for (int i = 0; i < rockCount; i++) {
            activeRocks.Add(Build("Rock", true));
        }

    }

    void Update() {
        // Clean up destroyed rocks (from bullets or other sources)
        activeRocks.RemoveAll(r => r == null);

        // Check for rocks that have drifted too far off-screen
        for (int i = activeRocks.Count - 1; i >= 0; i--) {
            if (Helper.IsOutsideExpandedViewport(activeRocks[i].transform.position, 2f)) {
                Destroy(activeRocks[i]);
                activeRocks.RemoveAt(i);
            }
        }

        if (activeStars.Count < starCount) {
            activeStars.Add(Build("Star", false));
        }

        if (activeRocks.Count < rockCount) {
            activeRocks.Add(Build("Rock", false));
        }
    }

    public GameObject Build(string type, bool onScreen) {

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

        GameObject retobj = type switch {
            "Star" => BuildStar(pos),
            "Rock" => BuildRock(pos),
            _ => throw new System.NotImplementedException(),
        };

        return retobj;
    }

    public GameObject BuildStar(Vector3 pos) {
        GameObject star = Instantiate(MasterController.Singleton.prefabs["Star"], pos, Quaternion.identity);
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

    public GameObject BuildRock(Vector3 pos) {
        GameObject rock = Instantiate(MasterController.Singleton.prefabs["Rock"], pos, Quaternion.identity);

        //init with random size
        List<Rock.RockSize> rockSizes = new() { Rock.RockSize.Small, Rock.RockSize.Medium, Rock.RockSize.Large };
        rock.GetComponent<Rock>().Initialize(rockSizes[Random.Range(0, 3)]);

        return rock;
    }

}
