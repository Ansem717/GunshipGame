using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    private class InputEntry {
        public string name;
        public List<KeyCode> posKeys;
        public List<KeyCode> negKeys;
        public Action<float> func;

        // computed per frame
        public float CurrentValue {
            get {
                int pos = (posKeys.Any(k => Input.GetKey(k)) ? 1 : 0);
                int neg = (negKeys.Any(k => Input.GetKey(k)) ? -1 : 0);
                return pos + neg;
            }
        }

        public InputEntry(string name, Action<float> func, List<KeyCode> posKeys, List<KeyCode> negKeys) {
            this.name = name;
            this.func = func;
            this.posKeys = posKeys;
            this.negKeys = negKeys;
        }
    }

    private List<InputEntry> InputKeys;

    private Dictionary<Type, List<MonoBehaviour>> scripts;

    void Start() {
        scripts = new();
        InputKeys = new();

        //Load small gunship by default
        LoadGunship(MasterController.GunshipSize.Small);

    }

    public void LoadGunship(MasterController.GunshipSize size) {

        ClearComponentsAndScripts();
        InputKeys = new(); //clear current inputs

        MasterController.GunshipData data = MasterController.Singleton.GunshipDatas[size];

        if (data == null) {
            Debug.LogError($"GunshipController ERROR: LoadGunship - {size} data is null");
            return;
        }

        //Since data exists, load in the scale
        transform.localScale = MasterController.Singleton.prefabs["GunshipTriangle"].transform.localScale * data.scale;

        Debug.Log(data);

        foreach (CustomPhysics_SO physicsData in data.GetData<CustomPhysics_SO>()) {
            //!! Guard: If the script contains this key, we already added in a physics, so why are we still here?
            if (scripts.ContainsKey(typeof(CustomPhysics))) {
                Debug.LogError("GunshipController ERROR: Multiple instances of Physics_SO found.");
                break;
            }

            GameObject physicsObj = Instantiate(MasterController.Singleton.prefabs["CustomPhysicsComponent"], transform); //attach physics as a child object
            CustomPhysics physics = physicsObj.GetComponent<CustomPhysics>(); //capture the script

            physics.LoadData(physicsData); //load custom SO data

            scripts[typeof(CustomPhysics)] = new() { physics }; //track it in the dict; we're guaranteed to only have one so this shorthand works

            if (gameObject.CompareTag("Player")) {
                InputKeys.Add(new InputEntry(
                    name: "Physics_Move",
                    func: physics.ApplyThrustInput,
                    posKeys: new List<KeyCode> { KeyCode.W, KeyCode.UpArrow },
                    negKeys: new List<KeyCode> { KeyCode.S, KeyCode.DownArrow }
                ));

                //Rotate A,Left,D,Right
                InputKeys.Add(new InputEntry(
                    name: "Physics_Rotate",
                    func: physics.ApplyRotationalInput,
                    posKeys: new List<KeyCode> { KeyCode.A, KeyCode.LeftArrow },
                    negKeys: new List<KeyCode> { KeyCode.D, KeyCode.RightArrow }
                ));
            }
        }

        List<WeaponScriptableObject> weapons = data.GetData<WeaponScriptableObject>();

        for (int i = 0; i < weapons.Count; i++) {
            GameObject obj = null;
            if (weapons[i].GetType() == typeof(ChainGun_SO)) {
                obj = Instantiate(MasterController.Singleton.prefabs["ChainGun"], transform); //attach chain gun
                ChainGun chainGunScript = obj.GetComponent<ChainGun>(); //capture script
                chainGunScript.LoadData(weapons[i] as ChainGun_SO); //load data

                scripts.TryAdd(typeof(ChainGun), new()); //If the key doesn't exist, add the key with a new instantiated list.
                scripts[typeof(ChainGun)].Add(chainGunScript); //track in dict

                if (gameObject.CompareTag("Player")) {
                    InputKeys.Add(new InputEntry(
                        name: "Fire_ChainGun",
                        func: chainGunScript.Use,
                        posKeys: new List<KeyCode> { KeyCode.Space, KeyCode.Mouse0 },
                        negKeys: new List<KeyCode> { /* Not applicable */ }
                    ));
                }

            }

            if (obj != null) {
                // Position weapon based on weapon count and ship dimensions

                SpriteRenderer sr = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer component from this gunship
                Bounds bounds = sr.sprite.bounds; // Get the sprite's bounds in local space (unscaled dimensions)

                float width = bounds.size.x; // Extract width from the bounds (this is the actual sprite size, not scale)
                float height = bounds.size.y; // Extract height from the bounds

                float step = width / (1 + weapons.Count); // Calculate spacing step: divide width evenly by (weapon count + 1) for margins

                float left = width * -0.5f; // Calculate left edge position (anchor is at center, so shift left by half width)
                float posX = left + (step * (i + 1)); // Calculate X position: start from left edge, then add step * (index + 1)
                float posY = height * 0.5f; // Calculate Y position: place at top edge of sprite (half height up from center)

                // Assign the calculated local position to the weapon object
                // Unity will automatically apply parent scale when converting to world space
                obj.transform.localPosition = new Vector2(posX, posY);
            }

        }

    }

    void Update() {
        if (gameObject.CompareTag("Player")) {

            foreach (InputEntry entry in InputKeys) {
                entry.func(entry.CurrentValue);
            }

            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
        }
    }

    private void ClearComponentsAndScripts() {
        foreach (Transform childComponent in transform) {
            Destroy(childComponent.gameObject);
        }

        scripts = new();
    }
}
