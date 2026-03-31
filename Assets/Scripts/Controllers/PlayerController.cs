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
    private ChainGun chaingun;

    void Start() {

        Instantiate(MasterController.Singleton.prefabs["CustomPhysicsComponent"], transform); //attach physics to this
        physics = GetComponentInChildren<CustomPhysics>(); //capture the script
        if (!physics) Debug.LogError("FAILURE TO FIND PHYSICS FOR PLAYER!"); //check it exists

        Instantiate(MasterController.Singleton.prefabs["ChainGun"], transform); //attach chain gun
        chaingun = GetComponentInChildren<ChainGun>();
        if (!chaingun) Debug.LogError("FAILURE TO FIND CHAINGUN FOR PLAYER!"); //check it exists

        InputKeys = new() {

            //Move W,Up,S,Down
            [physics.ApplyThrustInput] = new InputEntry(
                name: "Move",
                func: physics.ApplyThrustInput,
                posKeys: new List<KeyCode> { KeyCode.W, KeyCode.UpArrow },
                negKeys: new List<KeyCode> { KeyCode.S, KeyCode.DownArrow }
            ),

            //Rotate A,Left,D,Right
            [physics.ApplyRotationalInput] = new InputEntry(
                name: "Rotate",
                func: physics.ApplyRotationalInput,
                posKeys: new List<KeyCode> { KeyCode.A, KeyCode.LeftArrow },
                negKeys: new List<KeyCode> { KeyCode.D, KeyCode.RightArrow }
            ),

            //ChainGun Space, Left Mouse Button
            [chaingun.Use] = new InputEntry(
                name: "Fire_ChainGun",
                func: chaingun.Use,
                posKeys: new List<KeyCode> { KeyCode.Space, KeyCode.Mouse0 },
                negKeys: new List<KeyCode> { /* Not applicable */ }
            )

        };

        //but also load in data for small gunship
        LoadGunship(MasterController.GunshipSize.Small);
        
    }

    public void LoadGunship(MasterController.GunshipSize size) {
        MasterController.GunshipData data = MasterController.Singleton.GunshipDatas[size];
        if (data != null && physics != null) {
            physics.LoadData(data.physicsData);
            transform.localScale = MasterController.Singleton.prefabs["GunshipTriangle"].transform.localScale * data.scale;
        }
    }

    void Update() {
        if (physics == null) return;
        if (chaingun == null) return;

        foreach ((_, InputEntry entry) in InputKeys) {
            entry.func(entry.CurrentValue);
        }

        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }
}
