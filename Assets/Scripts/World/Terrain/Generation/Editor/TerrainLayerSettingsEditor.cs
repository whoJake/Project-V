using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainLayerSettings))]
public class TerrainLayerSettingsEditor : Editor
{
    bool useCustomInspector = true;

    SerializedProperty depthProperty;
    SerializedProperty topTransitionProperty;
    SerializedProperty bottomTransitionProperty;
    SerializedProperty chasmRadiusProperty;

    SerializedProperty groundThicknessProperty;
    SerializedProperty groundDepthProperty;

    SerializedProperty groundHeightChangeMaxProperty;
    SerializedProperty groundHeightChangeScaleProperty;
    SerializedProperty groundHeightChangeComplexityProperty;
    SerializedProperty groundHeightChangeDistortionStrengthProperty;

    SerializedProperty surfaceRoughnessProperty;
    SerializedProperty surfaceFeatureDepthProperty;

    SerializedProperty pillarDensityProperty;
    SerializedProperty pillarScaleProperty;
    SerializedProperty pillarIgnoreStateProperty;

    SerializedProperty octavesProperty;
    SerializedProperty frequencyProperty;
    SerializedProperty persistanceProperty;
    SerializedProperty lacunarityProperty;

    public override void OnInspectorGUI() {
        useCustomInspector = EditorGUILayout.Toggle("Use Custom Insepctor?", useCustomInspector);

        if (useCustomInspector) CustomInspector();
        else                    base.DrawDefaultInspector();
    }

    private void OnEnable() {
        depthProperty = serializedObject.FindProperty("depth");
        topTransitionProperty = serializedObject.FindProperty("topTransition");
        bottomTransitionProperty = serializedObject.FindProperty("bottomTransition");
        chasmRadiusProperty = serializedObject.FindProperty("chasmRadius");

        groundThicknessProperty = serializedObject.FindProperty("groundThickness");
        groundDepthProperty = serializedObject.FindProperty("groundDepth");

        groundHeightChangeMaxProperty = serializedObject.FindProperty("groundHeightChangeMax");
        groundHeightChangeScaleProperty = serializedObject.FindProperty("groundHeightChangeScale");
        groundHeightChangeComplexityProperty = serializedObject.FindProperty("groundHeightChangeComplexity");
        groundHeightChangeDistortionStrengthProperty = serializedObject.FindProperty("groundHeightChangeDistortionStrength");

        surfaceRoughnessProperty = serializedObject.FindProperty("surfaceRoughness");
        surfaceFeatureDepthProperty = serializedObject.FindProperty("surfaceFeatureDepth");

        pillarDensityProperty = serializedObject.FindProperty("pillarDensity");
        pillarScaleProperty = serializedObject.FindProperty("pillarScale");
        pillarIgnoreStateProperty = serializedObject.FindProperty("pillarIgnoreState");

        octavesProperty = serializedObject.FindProperty("octaves");
        frequencyProperty = serializedObject.FindProperty("frequency");
        persistanceProperty = serializedObject.FindProperty("persistance");
        lacunarityProperty = serializedObject.FindProperty("lacunarity");
    }

    private void CustomInspector() {
        serializedObject.Update();

        EditorGUILayout.LabelField(new GUIContent("General Layer Options"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(depthProperty, new GUIContent("Depth"));
        EditorGUILayout.PropertyField(topTransitionProperty, new GUIContent("Top Transition"));
        EditorGUILayout.PropertyField(bottomTransitionProperty, new GUIContent("Bottom Transition"));
        EditorGUILayout.PropertyField(chasmRadiusProperty, new GUIContent("Chasm Radius"));
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();

        EditorGUILayout.LabelField(new GUIContent("Ground Settings"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(groundThicknessProperty, new GUIContent("Thickness"));
        EditorGUILayout.PropertyField(groundDepthProperty, new GUIContent("Depth"));

        EditorGUILayout.LabelField(new GUIContent("HeightMap"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(groundHeightChangeMaxProperty, new GUIContent("Maximum Change"));
        float currentMaxChange = -(-groundDepthProperty.floatValue + (groundThicknessProperty.floatValue / 2));
        if (groundHeightChangeMaxProperty.floatValue >= currentMaxChange) groundHeightChangeMaxProperty.floatValue = Mathf.Max(0f, currentMaxChange);

        EditorGUILayout.PropertyField(groundHeightChangeScaleProperty, new GUIContent("Scale"));
        EditorGUILayout.PropertyField(groundHeightChangeComplexityProperty, new GUIContent("Complexity"));
        EditorGUILayout.PropertyField(groundHeightChangeDistortionStrengthProperty, new GUIContent("Distortion Strength"));
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(new GUIContent("Surface"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(surfaceRoughnessProperty, new GUIContent("Roughness"));
        EditorGUILayout.PropertyField(surfaceFeatureDepthProperty, new GUIContent("Feature Depth"));
        if (surfaceFeatureDepthProperty.floatValue >= groundThicknessProperty.floatValue) surfaceFeatureDepthProperty.floatValue = groundThicknessProperty.floatValue;

        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(new GUIContent("Inter-Layer Pillars"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(pillarDensityProperty, new GUIContent("Density"));
        EditorGUILayout.PropertyField(pillarScaleProperty, new GUIContent("Scale"));
        EditorGUILayout.PropertyField(pillarIgnoreStateProperty, new GUIContent("Ignore State"));
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(new GUIContent("Experimental"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(octavesProperty, new GUIContent("Octaves"));
        EditorGUILayout.PropertyField(frequencyProperty, new GUIContent("Frequency"));
        EditorGUILayout.PropertyField(persistanceProperty, new GUIContent("Persistance"));
        EditorGUILayout.PropertyField(lacunarityProperty, new GUIContent("Lacunarity"));
        EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();
    }
}
