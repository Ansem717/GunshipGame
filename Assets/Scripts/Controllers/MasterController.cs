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
using UnityEngine.UI;

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

    private GameObject DebugPrefab;
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

    private GameObject pMenuPrefab;
    [HideInInspector]
    public PauseMenuController PauseMenu;

    public GameObject MainMenu;

    public FrameBlock mFrameBlock;
    public int FrameBlockSize;

    [HideInInspector]
    public GameObject FrameBarPrefab;

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
        pMenuPrefab = Resources.Load<GameObject>("Prefabs/PauseMenu");
        DebugPrefab = Resources.Load<GameObject>("Prefabs/DebugUI");
        FrameBarPrefab = Resources.Load<GameObject>("Prefabs/Bar");
        PauseMenu = null;
        globalTimeMultiplier = 1.0f;
        DebugFlag = false;
        debugViewer = null;

        mFrameBlock = new FrameBlock(FrameBlockSize);

        Graphic[] gItems = MainMenu.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < gItems.Length; i++) {
            Graphic item = gItems[i];
            if (item.TryGetComponent(out ActionList itemActionList)) {
                A_FadeIn fadeIn = new A_FadeIn(
                    graphic: item,
                    _targetAlpha: item.color.a,
                    _duration: 0.3f,
                    _delay: 0.2f * i,
                    _easing: EaseType.EaseIn
                );
                
                if (item.TryGetComponent(out UIButton uiButton)) {
                    uiButton.DelayInitialization();
                    itemActionList.PushBack(new A_Callback(fadeIn, () => uiButton.Initialize()));
                } else {
                    itemActionList.PushBack(fadeIn);
                }
            }
        }
        MainMenu.SetActive(true);

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

        if (Input.GetKeyDown(KeyCode.D)) {
            DebugFlag = !DebugFlag;
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (PauseMenu == null) PauseGame();
            else PauseMenu.DespawnPauseMenu();
        }

        if (!Paused) CollideMouseWithCardRaycast();
    }

    public void SetAutoplay(bool state) {
        Autoplay = state;
        A_StaticBlocker.CurrentFlag = SBFlag.PlayerTurnComplete; //When autoplay is active, a SB_PTC is created, so this just clears out any PTCs.
    }

    public void TogglePause() => Paused = !Paused;
    public void PauseGame() => Paused = true;
    public void UnpauseGame() => Paused = false;

    [HideInInspector]
    public CardObject cardUnderMouse;

    public void CollideMouseWithCardRaycast() {
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorld, Vector2.zero);

        CardObject next = null;
        int bestOrder = int.MinValue;

        foreach (var hit in hits) {
            if (!hit.collider) continue;

            CardObject card = hit.collider.GetComponent<CardObject>();
            if (!card) continue;

            int order = card.data.SortingOrder;

            if (order > bestOrder) {
                bestOrder = order;
                next = card;
            }
        }

        if (cardUnderMouse != next) {
            cardUnderMouse?.OnHoverExit();
            next?.OnHoverEnter();
            cardUnderMouse = next;
        }
    }

    public void SetZoneShowing(bool show) {
        Zone.Showing = show;
        if (show) Telemetry.Instance.RecordMenuEntry(Telemetry.TelemetryMenuOption.ShowZones);
        else if (!show) Telemetry.Instance.RecordMenuEntry(Telemetry.TelemetryMenuOption.HideZones);
    }

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
