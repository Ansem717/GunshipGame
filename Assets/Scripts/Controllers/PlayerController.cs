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

    private Dictionary<Action<float>, InputEntry> InputKeys;
    
    private CustomPhysics physics;

    void Start() {

        Instantiate(MasterController.Singleton.CustomPhysicsPrefab, transform); //attach physics to THIS
        physics = GetComponentInChildren<CustomPhysics>(); //capture the script
        if (!physics) {
            Debug.LogError("FAILURE TO FIND PHYSICS FOR PLAYER!"); //check it exists
        } else {

            //it does exist, attach functions
            InputKeys = new() {
                [physics.ApplyThrustInput] = new InputEntry(
                    name: "Move",
                    func: physics.ApplyThrustInput,
                    posKeys: new List<KeyCode> { KeyCode.W, KeyCode.UpArrow },
                    negKeys: new List<KeyCode> { KeyCode.S, KeyCode.DownArrow }
                ),

                [physics.ApplyRotationalInput] = new InputEntry(
                    name: "Rotate",
                    func: physics.ApplyRotationalInput,
                    posKeys: new List<KeyCode> { KeyCode.A, KeyCode.LeftArrow },
                    negKeys: new List<KeyCode> { KeyCode.D, KeyCode.RightArrow }
                ),
            };

            //but also load in data for small gunship
            LoadGunship(MasterController.GunshipSize.Small);
        }
    }

    public void LoadGunship(MasterController.GunshipSize size) {
        MasterController.GunshipData data = MasterController.Singleton.GunshipDatas[size];
        if (data != null && physics != null) {
            physics.LoadData(data.physicsData);
            transform.localScale = MasterController.Singleton.GunshipPrefab.transform.localScale * data.scale;
        }
    }

    void Update() {
        if (physics == null) return;

        foreach ((_, InputEntry entry) in InputKeys) {
            Debug.Log($"{entry.name} with value {entry.CurrentValue}");
            entry.func(entry.CurrentValue);
        }

        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }
}
