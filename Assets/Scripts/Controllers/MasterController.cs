//---------------------------------------------------------
// file:	MasterController.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	A persistant singleton instance to provide global user controls
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class MasterController : MonoBehaviour {

    public static MasterController Singleton;

    void Awake() {
        if (Singleton != null) {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
        DontDestroyOnLoad(gameObject);
    }

    //////////////////////////////

    public bool Autoplay;

    [HideInInspector]
    public DebugViewer debugViewer;
    private bool debugFlag;
    public bool DebugFlag {
        get => debugFlag;
        set {
            if (debugFlag == value) return;
            debugFlag = value;
            if (debugFlag) {
                //show debug menu
                GameObject instance = Instantiate(DebugPrefab);
                debugViewer = instance.GetComponent<DebugViewer>();
            } else {
                //hide debug menu
                debugViewer.Close();
            }
        }
    }

    private float globalTimeMultiplier;
    private float dirtyGTM; //GTM dirty flag by matching the value

    public List<ActionList> actionLists;
    [HideInInspector]
    public bool actionListsDirty;

    [HideInInspector]
    public PauseMenuController PauseMenu;

    public FrameBlock mFrameBlock;
    public int FrameBlockSize;

    [HideInInspector]
    public GameObject FrameBarPrefab;
    private GameObject DebugPrefab;
    private GameObject pMenuPrefab;

    public enum GunshipSize { Small, Medium, Large };
    public class GunshipData {
        public GunshipSize size;
        public float scale;
        public CustomPhysics_SO physicsData;

        public GunshipData(GunshipSize size, float scale, CustomPhysics_SO physicsData) {
            this.size = size;
            this.scale = scale;
            this.physicsData = physicsData;
        }
    }

    [HideInInspector]
    public GameObject GunshipPrefab;
    [HideInInspector]
    public Dictionary<GunshipSize, GunshipData> GunshipDatas;
    [HideInInspector]
    public GameObject CustomPhysicsPrefab;

    public PlayerController player;

    private bool _paused;
    public bool Paused {
        get => _paused;
        set {
            if (_paused == value) return; //do not set to itself

            if (SceneManager.GetActiveScene().name == "MainMenu") {
                //If we're in the main menu, somehow Pause got trigger, let's make sure we're not paused.
                A_StaticBlocker.CurrentFlag = SBFlag.PauseMenuExit;
                _paused = false;
                return;
            };

            _paused = value;

            if (_paused) {
                foreach (ActionList actionList in actionLists.Where(al => al.EnablePauseWithGame)) {
                    //make sure I'm only grabbing action lists I want to pause. 
                    actionList.PushFront(new A_StaticBlocker(SBFlag.PauseMenuExit));
                }

                GameObject instance = Instantiate(pMenuPrefab);
                PauseMenu = instance.GetComponentInChildren<PauseMenuController>(true);

            } else {
                A_StaticBlocker.CurrentFlag = SBFlag.PauseMenuExit; //clear out all of the SB_Pause
            }
        }
    }

    private void Start() {
        actionLists = new();
        actionListsDirty = true;
        PauseMenu = null;
        globalTimeMultiplier = 1.0f;
        DebugFlag = false;
        debugViewer = null;

        mFrameBlock = new FrameBlock(FrameBlockSize);

        pMenuPrefab = Resources.Load<GameObject>("Prefabs/Misc/PauseMenu");
        DebugPrefab = Resources.Load<GameObject>("Prefabs/Debug/DebugUI"); 
        FrameBarPrefab = Resources.Load<GameObject>("Prefabs/Debug/Bar");  

        GunshipPrefab = Resources.Load<GameObject>("Prefabs/Gunship/GunshipTriangle");
        CustomPhysicsPrefab = Resources.Load<GameObject>("Prefabs/Components/CustomPhysicsComponent");

        GunshipDatas = new() {
            [GunshipSize.Small] = new(GunshipSize.Small, 0.8f, Resources.Load<CustomPhysics_SO>("ScriptableObjects/Gunships/GSP_Small")),
            [GunshipSize.Medium] = new(GunshipSize.Medium, 1.3f, Resources.Load<CustomPhysics_SO>("ScriptableObjects/Gunships/GSP_Medium")),
            [GunshipSize.Large] = new(GunshipSize.Large, 1.8f, Resources.Load<CustomPhysics_SO>("ScriptableObjects/Gunships/GSP_Large")),
        };

        GameObject playerOBJ = Instantiate(GunshipPrefab);
        player = playerOBJ.AddComponent<PlayerController>();
    }

    private void Update() {
        mFrameBlock.Update();

        if (actionListsDirty) {
            actionLists = FindObjectsByType<ActionList>(FindObjectsSortMode.None).ToList();
            actionListsDirty = false;
            Debug.Log($"ALDirty, reloaded {actionLists.Count} lists.");
            dirtyGTM = -999; //flag the GTM as dirty so all lists get updated.
        }

        if (dirtyGTM != globalTimeMultiplier) {
            foreach (ActionList actionList in actionLists) {
                actionList.timeMultiplier = globalTimeMultiplier;
            }
            dirtyGTM = globalTimeMultiplier;
        }

        if (Input.GetKeyDown(KeyCode.T)) {
            SetAutoplay(!Autoplay);
        }

        if (Input.GetKeyDown(KeyCode.B)) {
            DebugFlag = !DebugFlag;
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (PauseMenu == null) PauseGame();
            else PauseMenu.DespawnPauseMenu();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) player.LoadGunship(GunshipSize.Small);
        if (Input.GetKeyDown(KeyCode.Alpha2)) player.LoadGunship(GunshipSize.Medium);
        if (Input.GetKeyDown(KeyCode.Alpha3)) player.LoadGunship(GunshipSize.Large);

    }

    public void SetAutoplay(bool state) {
        Autoplay = state;
        A_StaticBlocker.CurrentFlag = SBFlag.PlayerTurnComplete; //When autoplay is active, a SB_PTC is created, so this just clears out any PTCs.
    }

    public void TogglePause() => Paused = !Paused;
    public void PauseGame() => Paused = true;
    public void UnpauseGame() => Paused = false;

    public void QuitGame() {

        //If I was a good programmer, I would make a CDA

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetTime(float t) => globalTimeMultiplier = t;

}
