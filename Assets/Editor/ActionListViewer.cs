using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(ActionList), true)]
public class ActionListViewer : Editor {
    public override void OnInspectorGUI() {
        // Draw default inspector (optional)
        DrawDefaultInspector();

        // Get the target ActionList
        ActionList actionList = (ActionList)target;

        if (Application.isPlaying) {
            EditorGUILayout.LabelField("Actions (Runtime Readonly):", EditorStyles.boldLabel);

            if (actionList.actions != null && actionList.actions.Count > 0) { 
                foreach (var action in actionList.actions) {
                    if (action != null) {
                        EditorGUILayout.LabelField(action.name);
                    }
                }
            }
        } else {
            EditorGUILayout.HelpBox("Actions are only visible during runtime.", MessageType.Info);
        }
    }
}
