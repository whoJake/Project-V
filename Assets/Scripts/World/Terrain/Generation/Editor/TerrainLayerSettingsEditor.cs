using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainLayerSettings))]
public class TerrainLayerSettingsEditor : Editor
{
    bool useCustomInspector = true;

    SerializedProperty depthProperty;
    SerializedProperty topMarginProperty;
    SerializedProperty bottomMarginProperty;
    SerializedProperty chasmRadiusProperty;

    public override void OnInspectorGUI() {
        useCustomInspector = EditorGUILayout.Toggle("Use Custom Insepctor?", useCustomInspector);

        if (useCustomInspector) CustomInspector();
        else                    base.DrawDefaultInspector();
    }

    private void OnEnable() {
        depthProperty = serializedObject.FindProperty("depth");
        topMarginProperty = serializedObject.FindProperty("topMargin");
        bottomMarginProperty = serializedObject.FindProperty("bottomMargin");
        chasmRadiusProperty = serializedObject.FindProperty("chasmRadius");
    }

    private void CustomInspector() {
        serializedObject.Update();

        EditorGUILayout.LabelField(new GUIContent("General Options"), EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(depthProperty, new GUIContent("Depth"));
        EditorGUILayout.Space();

        EditorGUILayout.LabelField(new GUIContent("Margin"), EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(topMarginProperty, new GUIContent("Top Margin"));
        EditorGUILayout.PropertyField(bottomMarginProperty, new GUIContent("Bottom Margin"));
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(chasmRadiusProperty, new GUIContent("Chasm Radius"));
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.LabelField(new GUIContent("Noise/Generation"), EditorStyles.boldLabel);

        serializedObject.ApplyModifiedProperties();
    }
}
