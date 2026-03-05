//---------------------------------------------------------
// file:	PauseMenuController.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	The controller to provide pause menu management
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour {

    public static float offset = 1080.0f;

    public struct ItemEntry {
        public enum Direction { Top, Right, Bottom, Left }

        public GameObject obj;
        public Direction fromDir;

        public ItemEntry(GameObject o, Direction d) {
            obj = o;
            fromDir = d;

            Vector3 p = obj.transform.position;
            switch (d) {
                case Direction.Top:
                    p.y += offset;
                    break;
                case Direction.Right:
                    p.x += offset;
                    break;
                case Direction.Bottom:
                    p.y -= offset;
                    break;
                case Direction.Left:
                    p.x -= offset;
                    break;
                default:
                    break;
            }
            obj.transform.position = p;
        }

        public Vector3 GetMotionVector() {
            return fromDir switch {
                Direction.Top => new Vector3(0, -offset, 0),
                Direction.Right => new Vector3(-offset, 0, 0),
                Direction.Bottom => new Vector3(0, offset, 0),
                Direction.Left => new Vector3(offset, 0, 0),
                _ => Vector3.zero,
            };
        }

        public Vector3 GetRandomMotionVector() {
            List<Vector3> mvs = new() {
                new Vector3(0, -offset, 0),
                new Vector3(-offset, 0, 0),
                new Vector3(0, offset, 0),
                new Vector3(offset, 0, 0),
            };
            return mvs[Random.Range(0, mvs.Count)];
        }
    }
    public Dictionary<string, ItemEntry> PauseItems;
    public Dictionary<string, ItemEntry> SettingItems;

    public float duration;
    public float delay;

    public enum PauseState { Entering, Leaving, MainPause, Settings, MainPauseToSettings, SettingsToMainPause }
    public PauseState pState;

    public TextMeshProUGUI timeCaption;

    void Start() {

        List<ItemEntry.Direction> dirs = new() { ItemEntry.Direction.Top, ItemEntry.Direction.Bottom, ItemEntry.Direction.Right, ItemEntry.Direction.Left };

        PauseItems = new Dictionary<string, ItemEntry> {
            ["Title"] = new ItemEntry(transform.Find("Title").gameObject, dirs[Random.Range(0, dirs.Count)]),
            ["Resume"] = new ItemEntry(transform.Find("Resume").gameObject, dirs[Random.Range(0, dirs.Count)]),
            ["Settings"] = new ItemEntry(transform.Find("Settings").gameObject, dirs[Random.Range(0, dirs.Count)]),
            ["Main Menu"] = new ItemEntry(transform.Find("Main Menu").gameObject, dirs[Random.Range(0, dirs.Count)]),
            ["Quit"] = new ItemEntry(transform.Find("Quit").gameObject, dirs[Random.Range(0, dirs.Count)]),
        };

        SettingItems = new Dictionary<string, ItemEntry> {
            ["Title"] = PauseItems["Title"], //do not reconstruct, just use the same entry
            ["Autoplay"] = new ItemEntry(transform.Find("Autoplay").gameObject, dirs[Random.Range(0, dirs.Count)]),
            ["TimeSlider"] = new ItemEntry(transform.Find("TimeSlider").gameObject, dirs[Random.Range(0, dirs.Count)]),
            ["Back"] = new ItemEntry(transform.Find("Back").gameObject, dirs[Random.Range(0, dirs.Count)]),
        };

        SettingItems["Autoplay"].obj.GetComponent<Toggle>().isOn = MasterController.Singleton.Autoplay;
        SettingItems["TimeSlider"].obj.GetComponent<Slider>().value = 1;
        timeCaption.text = "Normal Speed";

        pState = PauseState.Entering;
        MasterController.Singleton.actionListsDirty = true;
        //Telemetry.Instance.RecordMenuEntry(Telemetry.TelemetryMenuOption.Open);

        /*
        A_AutoplayRequired.SetOptionsForPause();
        */

        if (TryGetComponent(out ActionList actionList)) {
            actionList.PushBack(new A_Callback(
                action: new A_MoveInDirection(
                    relativeDirection: new Vector3(650, 0, 0),
                    _duration: 0.16f,
                    _easing: EaseType.None
                ),
                callback: () => {
                    int k = 0;

                    foreach ((string key, ItemEntry item) in PauseItems.OrderBy(_ => Random.value)) {
                        GameObject go = item.obj;

                        Vector3 rD = item.GetMotionVector();

                        if (go.TryGetComponent(out ActionList go_al)) {
                            go_al.PushBack(new A_MoveInDirection(
                                relativeDirection: rD,
                                _duration: duration,
                                _easing: EaseType.EaseOut,
                                _delay: delay * k
                            ));
                        }
                        k++;
                    }

                    //because i'm too lazy to change my loop, i just create a wait action equal to the total loading duration
                    actionList.PushBack(new A_WaitThenCallback(duration + delay * k, false, () => {
                        pState = PauseState.MainPause;
                        PauseItems["Quit"].obj.GetComponent<Button>().onClick.AddListener(MasterController.Singleton.QuitGame);
                    }));
                }
            ));
        }
    }

    public void DespawnPauseMenu() {
        if (pState == PauseState.Settings) {
            //This is hit on escape. If we're in settings, go back to main.
            SwapToMainPause();
            return;
        }
        if (pState != PauseState.MainPause) return;
        pState = PauseState.Leaving;
        //Telemetry.Instance.RecordMenuEntry(Telemetry.TelemetryMenuOption.Resume);
        
        /*
        A_AutoplayRequired.SetOptionsForGameplay();
        */

        int i = 0;
        foreach ((string key, ItemEntry item) in PauseItems.OrderBy(_ => Random.value)) {
            if (item.obj.TryGetComponent(out ActionList al)) {
                al.PushBack(new A_MoveInDirection(
                    relativeDirection: item.GetRandomMotionVector(),
                    _duration: duration,
                    _easing: EaseType.EaseIn,
                    _delay: delay * i
                ));
                i++;
            }
        }

        if (TryGetComponent(out ActionList myAL)) {
            myAL.PushBack(new A_Callback(
                action: new A_MoveInDirection(
                    relativeDirection: new Vector3(-offset, 0, 0),
                    _duration: duration,
                    _easing: EaseType.EaseIn,
                    _delay: delay * i
                ),
                callback: () => {
                    MasterController.Singleton.UnpauseGame();
                    Destroy(transform.parent.gameObject); //since we are PANEL, we go to parent to destroy the prefab itself
                    MasterController.Singleton.actionListsDirty = true;
                }
            ));
        }

    }

    public void SwapToSettingsView() {
        if (pState != PauseState.MainPause) return;
        pState = PauseState.MainPauseToSettings;
        //Telemetry.Instance.RecordMenuEntry(Telemetry.TelemetryMenuOption.Settings);

        /*
        A_AutoplayRequired.SetOptionsForSettings();
        */

        int i = 0;
        foreach ((string key, ItemEntry item) in PauseItems.OrderBy(_ => Random.value)) {
            if (item.obj.TryGetComponent(out ActionList al)) {

                if (key == "Title") {
                    al.PushBack(new A_Callback(
                        action: new A_ColorShift(
                            targetColor: new Color(1f, 1f, 1f, 0f),
                            _duration: duration,
                            _easing: EaseType.EaseIn,
                            _delay: delay * i
                        ),
                        callback: () => {
                            if (item.obj.TryGetComponent(out TextMeshProUGUI tmp)) {
                                tmp.text = "SETTINGS";
                            }
                            al.PushBack(new A_ColorShift(
                                targetColor: new Color(1f, 1f, 1f, 1f),
                                _duration: duration,
                                _easing: EaseType.EaseOut
                            ));
                        }
                    ));
                } else {
                    al.PushBack(new A_MoveInDirection(
                        relativeDirection: item.GetMotionVector() * -1,
                        _duration: duration,
                        _easing: EaseType.EaseIn,
                        _delay: delay * i
                    ));
                }
                i++;
            }
        }

        if (TryGetComponent(out ActionList actionList)) {
            actionList.PushBack(new A_WaitThenCallback(duration + delay * i, false, () => {

                int k = 0;
                foreach ((string key, ItemEntry item) in SettingItems.OrderBy(_ => Random.value)) {
                    if (key == "Title") continue;

                    GameObject go = item.obj;
                    Vector3 rD = item.GetMotionVector();

                    if (go.TryGetComponent(out ActionList go_al)) {
                        go_al.PushBack(new A_MoveInDirection(
                            relativeDirection: rD,
                            _duration: duration,
                            _easing: EaseType.EaseOut,
                            _delay: delay * k
                        ));
                    }
                    k++;
                }

                actionList.PushBack(new A_WaitThenCallback(duration + delay * k, false, () => {
                    pState = PauseState.Settings;
                    SettingItems["Autoplay"].obj.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();//clear listeners first
                    SettingItems["Autoplay"].obj.GetComponent<Toggle>().onValueChanged.AddListener(MasterController.Singleton.SetAutoplay);


                    SettingItems["TimeSlider"].obj.GetComponent<Slider>().onValueChanged.RemoveAllListeners();
                    SettingItems["TimeSlider"].obj.GetComponent<Slider>().onValueChanged.AddListener(TimeSliderChange);
                }));
            }));
        }
    }

    public void SwapToMainPause() {
        if (pState != PauseState.Settings) return;
        pState = PauseState.SettingsToMainPause;
        //Telemetry.Instance.RecordMenuEntry(Telemetry.TelemetryMenuOption.SettingsBack);

        /*
        A_AutoplayRequired.SetOptionsForPause();
        */

        int i = 0;
        foreach ((string key, ItemEntry item) in SettingItems.OrderBy(_ => Random.value)) {
            if (item.obj.TryGetComponent(out ActionList al)) {

                if (key == "Title") {
                    al.PushBack(new A_Callback(
                        action: new A_FadeOut(
                            _duration: duration,
                            _easing: EaseType.None,
                            _delay: delay * i
                        ),
                        callback: () => {
                            if (item.obj.TryGetComponent(out TextMeshProUGUI tmp)) {
                                tmp.text = "PAUSED";
                                al.PushBack(new A_FadeIn(
                                    graphic: tmp,
                                    _targetAlpha: 1f,
                                    _duration: duration,
                                    _easing: EaseType.None
                                ));
                            }
                        }
                    ));
                } else {
                    al.PushBack(new A_MoveInDirection(
                        relativeDirection: item.GetMotionVector() * -1,
                        _duration: duration,
                        _easing: EaseType.EaseIn,
                        _delay: delay * i
                    ));
                }
                i++;
            }
        }

        if (TryGetComponent(out ActionList actionList)) {
            actionList.PushBack(new A_WaitThenCallback(duration + delay * i, false, () => {

                int k = 0;
                foreach ((string key, ItemEntry item) in PauseItems.OrderBy(_ => Random.value)) {
                    if (key == "Title") continue;

                    GameObject go = item.obj;
                    Vector3 rD = item.GetMotionVector();

                    if (go.TryGetComponent(out ActionList go_al)) {
                        go_al.PushBack(new A_MoveInDirection(
                            relativeDirection: rD,
                            _duration: duration,
                            _easing: EaseType.EaseOut,
                            _delay: delay * k
                        ));
                    }
                    k++;
                }

                actionList.PushBack(new A_WaitThenCallback(duration + delay * k, false, () => {
                    pState = PauseState.MainPause;
                }));
            }));
        }
    }

    public void TimeSliderChange(float _t) {
        int t = (int)_t;
        switch (t) {
            case 0:
                MasterController.Singleton.SetTime(0.5f);
                timeCaption.text = "Slow Speed";
                //Telemetry.Instance.RecordMenuEntry(Telemetry.TelemetryMenuOption.SpeedSlow);
                break;
            case 1:
                MasterController.Singleton.SetTime(1f);
                timeCaption.text = "Normal Speed";
                //Telemetry.Instance.RecordMenuEntry(Telemetry.TelemetryMenuOption.SpeedNormal);
                break;
            case 2:
                MasterController.Singleton.SetTime(2f);
                timeCaption.text = "Fast Speed";
                //Telemetry.Instance.RecordMenuEntry(Telemetry.TelemetryMenuOption.SpeedFast);
                break;
            case 3:
                MasterController.Singleton.SetTime(4f);
                timeCaption.text = "Ultra Speed";
                //Telemetry.Instance.RecordMenuEntry(Telemetry.TelemetryMenuOption.SpeedUltra);
                break;
            default:
                MasterController.Singleton.SetTime(1f);
                timeCaption.text = "Normal Speed";
                //Telemetry.Instance.RecordMenuEntry(Telemetry.TelemetryMenuOption.SpeedNormal);
                break;
        }
    }

}
