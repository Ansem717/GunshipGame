//---------------------------------------------------------
// file:	A_AutoplayRequirements
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	The autoplay script for the scene required for the grade.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class A_AutoplayRequired : ActionInterface {

    private Zone playArea;
    private List<Hand> hands;

    public enum Option {  Play, Pause, Resume, Settings, ChangeZone, ChangeDebug, ChangeSpeed }
    public struct OptionEntry {
        public Option opt;
        public float weight;
        public OptionEntry(Option opt, float weight) { this.opt = opt; this.weight = weight; }
    }
    public static List<OptionEntry> CurrentOptions;

    public A_AutoplayRequired() : base() {
        delay = Random.Range(0.6f, 1.0f);
        name = "Autoplay";
        playArea = GameObject.Find("PlayArea").GetComponent<Zone>();
        hands = Object.FindObjectsByType<Hand>(FindObjectsSortMode.None)
                      .OrderBy(h => h.transform.GetSiblingIndex())
                      .ToList();
    }

    public override bool Init() {
        GameMaster.Turn = 0;
        GameMaster.PlayerIndex = 0;

        SetOptionsForGameplay();

        return true;
    }

    /// <summary>
    /// Autoplay should NOT be blocked by the PauseMenuExit blocker so it can still
    /// interact with pause menu UI (Resume, Settings, etc.). The options are already
    /// restricted via SetOptionsForPause() so it won't try to play cards while paused.
    /// </summary>
    public override bool CanBeBlockedBy(ActionInterface blocker) {
        if (blocker is A_StaticBlocker sb && sb.targetFlag == SBFlag.PauseMenuExit) {
            return false;
        }
        return true;
    }

    public static void SetOptionsForGameplay() {
        CurrentOptions = new() {
            new OptionEntry(Option.Play, 7f), //70%
            new OptionEntry(Option.Pause, 3f) //30%
        };
    }

    public static void SetOptionsForPause() {
        CurrentOptions = new() {
            new OptionEntry(Option.Resume, 1f),
            new OptionEntry(Option.Settings, 1f)
        };
    }

    public static void SetOptionsForSettings() {
        CurrentOptions = new() {
            new OptionEntry(Option.Resume, 1f),
            new OptionEntry(Option.ChangeZone, 1f),
            new OptionEntry(Option.ChangeDebug, 1f),
            new OptionEntry(Option.ChangeSpeed, 1f),
        };
    }

    public Option RollOption() {
        float sum = CurrentOptions.Sum(o => o.weight);
        float roll = Random.Range(0, sum);
        foreach (OptionEntry oe in CurrentOptions) {
            if (roll < oe.weight) return oe.opt;
            else roll -= oe.weight;
        }
        return Option.Play; //incase something goes wrong
    }

    public override void IUpdate(float dt) {

        Option opt;
        if (MasterController.Singleton.Autoplay) {
            opt = RollOption();
        } else if (MasterController.Singleton.Paused) {
            // Autoplay is off and game is paused - wait for user to unpause manually
            return;
        } else {
            opt = Option.Play;
        }

        Debug.LogWarning($"Autoplay: {opt}");

        if (opt == Option.Play) {
            Play();
            delay = Random.Range(0.6f, 1.0f);

        } else if (opt == Option.Pause) {
            MasterController.Singleton.PauseGame();
            // Wait for pause menu to fully animate in: initial move (0.16f) + items animating in
            PauseMenuController pMenu = MasterController.Singleton.PauseMenu;
            float d = pMenu.duration;
            float dl = pMenu.delay;
            delay = 0.1f + 0.16f + d + dl * 5; // 5 pause items

        } else if (opt == Option.Resume) {
            PauseMenuController pMenu = MasterController.Singleton.PauseMenu;
            float d = pMenu.duration;
            float dl = pMenu.delay;
            if (pMenu.pState == PauseMenuController.PauseState.Settings) {
                // DespawnPauseMenu will call SwapToMainPause and return early
                // Wait for Settings -> MainPause transition to complete
                delay = 0.1f + (d + dl * 5) + (d + dl * 4); // settings out + pause in
            } else {
                // MainPause -> gone: items animate out
                delay = 0.1f + d + dl * 5;
            }
            pMenu.DespawnPauseMenu();

        } else if (opt == Option.Settings) {
            PauseMenuController pMenu = MasterController.Singleton.PauseMenu;
            pMenu.SwapToSettingsView();
            float d = pMenu.duration;
            float dl = pMenu.delay;
            // MainPause items out + Settings items in
            delay = 0.1f + (d + dl * 5) + (d + dl * 4);

        } else if (opt == Option.ChangeZone) {
            MasterController.Singleton.PauseMenu.SettingItems["ShowZones"].obj.GetComponent<Toggle>().isOn = !Zone.Showing;
            delay = 0.2f; 

        } else if (opt == Option.ChangeDebug) {
            //DO NOTHING, Debug is not implemented yet
            delay = 0.2f;

        } else if (opt == Option.ChangeSpeed) {
            Slider s = MasterController.Singleton.PauseMenu.SettingItems["TimeSlider"].obj.GetComponent<Slider>();
            int r = Random.Range(0, 3); //0, 1, or 2

            if (r >= s.value) r++;
            /* slider | valid Rs
             *      0 | 1, 2, 3
             *      1 | 0, 2, 3
             *      2 | 0, 1, 3
             *      3 | 0, 1, 2
             */

            s.value = r;
            delay = 0.2f;

        }

        if (State != ActionState.Done) State = ActionState.Waiting;
    }

    public void Play() { 

        // If autoplay is OFF and it's the human player's turn, block and wait for their input
        if (!MasterController.Singleton.Autoplay && GameMaster.PlayerIndex == 0) {
            Owner.PushFront(new A_StaticBlocker(SBFlag.PlayerTurnComplete));
            Helpers.EnablePlayerHand();
            State = ActionState.Waiting;
            return;
        }

        Hand currentHand = hands[GameMaster.PlayerIndex];

        if (!currentHand.Empty()) {
            CardInstance card = currentHand.RemoveRandomCard();

            // Capture both values before callback to avoid race conditions
            int capturedPlayerIndex = GameMaster.PlayerIndex;
            int capturedTurn = GameMaster.Turn;

            card.AddAction(new A_MoveToZone(
                zone: playArea,
                callback: () => {
                    playArea.DEBUG_AddCardToZone(card);
                    bool trickTaken = GameMaster.CheckTrickTake(playArea);

                    Telemetry.Instance.RecordCardEntry(card.GetCardName(), capturedPlayerIndex, trickTaken, capturedTurn);

                    if (trickTaken) {
                        Telemetry.Instance.RecordTrickEntry(playArea.Count(), capturedPlayerIndex, capturedTurn);
                        Owner.PushFront(new A_TakeTrick());
                        GameMaster.PlayerIndex = capturedPlayerIndex; //set the player to the captured player
                    }
                }
            ));
        } else {
            Debug.Log($"Player {GameMaster.PlayerIndex + 1} wins on turn {GameMaster.Turn}!");
            State = ActionState.Done;
            return;
        }

        GameMaster.PlayerIndex++;
        GameMaster.Turn++;
    }

    public override void Exit() { }
}
