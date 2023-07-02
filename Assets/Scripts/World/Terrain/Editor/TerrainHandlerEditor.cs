using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainHandler))]
public class TerrainHandlerEditor : Editor
{
    public override void OnInspectorGUI() {
        TerrainHandler handler = (TerrainHandler)target;
        base.OnInspectorGUI();
        EditorGUILayout.Space();

        if (!Application.isPlaying) {
            if (GUILayout.Button(new GUIContent("Force Generation"))) {
                handler.ForceGenerate();
            }

            if (GUILayout.Button(new GUIContent("Unload"))) {
                handler.Unload(true);
            }
        }
    }
}
