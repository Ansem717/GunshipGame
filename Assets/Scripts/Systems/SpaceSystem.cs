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
    private List<GameObject> activeEnemies;

    public int MaxEnemiesOnScreen;
    public float EnemySpawnCooldown;
    private float elapsed;

    void Start() {
        activeStars = new();
        activeRocks = new();
        activeEnemies = new();

        for (int i = 0; i < starCount; i++) {
            activeStars.Add(Build("Star", true));
        }

        for (int i = 0; i < rockCount; i++) {
            activeRocks.Add(Build("Rock", true));
        }

        elapsed = 0;
    }

    void Update() {
        // Clean up destroyed objs
        activeRocks.RemoveAll(r => r == null);
        activeStars.RemoveAll(s => s == null);
        activeEnemies.RemoveAll(e => e == null);

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

        // ENEMY SPAWNING
        if (activeEnemies.Count >= MaxEnemiesOnScreen) return;
        elapsed += Time.deltaTime;
        if (elapsed > EnemySpawnCooldown) {
            activeEnemies.Add(Build("Enemy", false));
            elapsed = 0;
        }
    }

    public GameObject Build(string type, bool onScreen) {

        Camera cam = Camera.main;
        Vector3 min = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 max = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

        float rx, ry;

        if (!onScreen) {
            // Spawn off-screen in the direction of player velocity
            float margin = 1.5f;
            float viewWidth = max.x - min.x;
            float viewHeight = max.y - min.y;

            // Get player velocity direction
            Vector3 velocity = Vector3.zero;
            if (MasterController.Singleton.player != null) {
                CustomPhysics physics = MasterController.Singleton.player.GetComponentInChildren<CustomPhysics>();
                if (physics != null) {
                    velocity = physics.VelocityVector;
                }
            }

            // If player is moving, spawn ahead of them; otherwise pick random edge
            if (velocity.sqrMagnitude > 0.01f) {
                Vector3 dir = velocity.normalized;
                
                // Determine which edge(s) to spawn from based on velocity
                // Spawn on the edge the player is moving toward
                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) {
                    // Moving more horizontally
                    if (dir.x > 0) {
                        // Moving right, spawn on right edge
                        rx = max.x + Random.Range(0.1f, viewWidth * margin);
                    } else {
                        // Moving left, spawn on left edge
                        rx = min.x - Random.Range(0.1f, viewWidth * margin);
                    }
                    ry = Random.Range(min.y, max.y);
                } else {
                    // Moving more vertically
                    if (dir.y > 0) {
                        // Moving up, spawn on top edge
                        ry = max.y + Random.Range(0.1f, viewHeight * margin);
                    } else {
                        // Moving down, spawn on bottom edge
                        ry = min.y - Random.Range(0.1f, viewHeight * margin);
                    }
                    rx = Random.Range(min.x, max.x);
                }
            } else {
                // Player stationary - pick random edge
                int edge = Random.Range(0, 4);
                switch (edge) {
                    case 0: // Top
                        rx = Random.Range(min.x, max.x);
                        ry = max.y + Random.Range(0.1f, viewHeight * margin);
                        break;
                    case 1: // Bottom
                        rx = Random.Range(min.x, max.x);
                        ry = min.y - Random.Range(0.1f, viewHeight * margin);
                        break;
                    case 2: // Left
                        rx = min.x - Random.Range(0.1f, viewWidth * margin);
                        ry = Random.Range(min.y, max.y);
                        break;
                    default: // Right
                        rx = max.x + Random.Range(0.1f, viewWidth * margin);
                        ry = Random.Range(min.y, max.y);
                        break;
                }
            }
        } else {
            // Spawn on screen with deadzone around player
            float deadzone = 2f; // Radius around player to avoid
            Vector3 playerPos = MasterController.Singleton.player != null 
                ? MasterController.Singleton.player.transform.position 
                : Vector3.zero;

            int maxAttempts = 10;
            do {
                rx = Random.Range(min.x, max.x);
                ry = Random.Range(min.y, max.y);
                maxAttempts--;
            } while (maxAttempts > 0 && Vector2.Distance(new Vector2(rx, ry), playerPos) < deadzone);
        }

        Vector3 pos = new Vector3(rx, ry, cam.nearClipPlane);

        GameObject retobj = type switch {
            "Star" => BuildStar(pos),
            "Rock" => BuildRock(pos),
            "Enemy" => BuildEnemy(pos),
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

        //init with random Size
        List<Rock.RockSize> rockSizes = new() { Rock.RockSize.Small, Rock.RockSize.Medium, Rock.RockSize.Large };
        rock.GetComponent<Rock>().Initialize(rockSizes[Random.Range(0, 3)]);

        return rock;
    }

    public GameObject BuildEnemy(Vector3 pos) {
        GameObject enemy = Instantiate(MasterController.Singleton.prefabs["GunshipTriangle"], pos, Quaternion.identity);

        enemy.tag = "Enemy";
        GunshipController enemyGS = enemy.AddComponent<GunshipController>();

        List<MasterController.GunshipSize> shipSizes = new() { MasterController.GunshipSize.Small, MasterController.GunshipSize.Medium, MasterController.GunshipSize.Large };
        enemyGS.LoadGunship(shipSizes[Random.Range(0, 3)]);

        /* Insert AI */

        return enemy;
    }

}
