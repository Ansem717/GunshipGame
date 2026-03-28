using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class DebugViewer : MonoBehaviour {

    public GameObject mainPanel;
    public TextMeshProUGUI performanceText;
    public TextMeshProUGUI actionsText;
    public RectTransform performanceGraphPanel;

    public float duration;
    public EaseType easing;

    private List<GameObject> bars;

    void Start() {
        MasterController.Singleton.actionListsDirty = true;

        if (mainPanel.TryGetComponent(out ActionList pAL)) {
            pAL.PushBack(new A_MoveInDirection(
                relativeDirection: new(-320, 0, 0),
                _easing: easing,
                _duration: duration
            ));
        }

        if (Camera.main.TryGetComponent(out ActionList cAL)) {
            cAL.PushBack(new A_LerpCameraZoom(
                targetZoom: 6,
                _easing: easing,
                _duration: duration
            ));
            cAL.PushBack(new A_MoveInDirection(
                relativeDirection: new Vector3(2, 0, 0),
                _easing: easing,
                _duration: duration
            ));
        }


        bars = new();

        float graphWidth = performanceGraphPanel.sizeDelta.x;

        for (int i = 0; i < MasterController.Singleton.FrameBlockSize; i++) {
            GameObject barO = Instantiate(MasterController.Singleton.FrameBarPrefab, performanceGraphPanel);
            bars.Add(barO);
            barO.GetComponent<RectTransform>().sizeDelta = new Vector2(graphWidth / MasterController.Singleton.FrameBlockSize, 0);
        }

    }

    void Update() {

        // UPDATE ACTIONS TEXT
        string actext = "<size=36><align=center><b>Actions</b></align></size>\n";

        foreach (ActionList aList in MasterController.Singleton.actionLists.OrderBy(al => al.DebugSortOrder)) {
            if (aList.gameObject == null) {
                Debug.LogWarning("An action list has been destroyed, but the MasterController did not update!");
                continue;
            }
            if (aList.actions.Count == 0) continue;
            actext += $"{aList.gameObject.name}:\n";
            foreach (ActionInterface aInterface in aList.actions) {
                actext += $" - {aInterface.name}\n";
            }
        }

        actionsText.text = actext;


        FrameBlock block = MasterController.Singleton.mFrameBlock;
        if (block.frames.Count == 0) return;

        performanceText.text = $"<size=36><align=center><b>Frame Rates</b></align></size>\r\n" +
            $"Worst . . . {block.Worst()}\r\n" +
            $"Median . . . {block.Median()}\r\n" +
            $"Mean . . . {block.Mean():F1}";


        List<int> reverseFrames = new(block.frames);
        reverseFrames.Reverse();

        int min = reverseFrames.Min();
        int max = reverseFrames.Max();
        float floor = 0.2f; //padding
        float ceil = 0.95f; //padding

        float range = max - min;
        if (range == 0) range = 1; // avoid division by zero

        List<float> reverseFramesNormalized = reverseFrames.Select(v => floor + (ceil - floor) * (v - min) / range).ToList();
        float minCheck = reverseFramesNormalized.Min();
        float maxCheck = reverseFramesNormalized.Max();

        float avg = reverseFramesNormalized.Average();
        float med = reverseFramesNormalized.OrderBy(f => f).ToList()[reverseFramesNormalized.Count / 2];

        float graphHeight = performanceGraphPanel.sizeDelta.y;

        for (int i = 0; i < reverseFramesNormalized.Count; i++) {
            RectTransform bar = (RectTransform)performanceGraphPanel.GetChild(i);
            bar.sizeDelta = new Vector2(bar.sizeDelta.x, graphHeight * reverseFramesNormalized[i]);
            if (reverseFramesNormalized[i] == minCheck) {
                bar.GetComponent<Image>().color = Color.red;
            } else if (reverseFramesNormalized[i] == maxCheck) {
                bar.GetComponent<Image>().color = Color.green;
            } else {
                bar.GetComponent<Image>().color = Color.yellow;
            }
        }

    }

    public void Close() {
        if (mainPanel.TryGetComponent(out ActionList pAL)) {
            pAL.PushBack(new A_Callback(
                action: new A_MoveInDirection(
                    relativeDirection: new(320, 0, 0),
                    _easing: easing,
                    _duration: duration
                ),
                callback: () => {
                    MasterController.Singleton.actionListsDirty = true;
                    Destroy(gameObject);
                }
            ));
        }

        if (Camera.main.TryGetComponent(out ActionList cAL)) {
            cAL.PushBack(new A_LerpCameraZoom(
                targetZoom: 5,
                _easing: easing,
                _duration: duration
            ));
            cAL.PushBack(new A_MoveInDirection(
                relativeDirection: new Vector3(-2, 0, 0),
                _easing: easing,
                _duration: duration
            ));
        }
    }
}
