using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EntityController))]
public class EntityControllerEditor : Editor {
    protected static bool useCustomInspector = true;

    public override void OnInspectorGUI() {
        useCustomInspector = EditorGUILayout.Toggle("Use Custom Inspector?", useCustomInspector);
        EditorGUILayout.Space();

        if (useCustomInspector)
            CustomInspector();
        else
            base.OnInspectorGUI();
    }

    SerializedProperty movementProviderProperty;
    SerializedProperty behaviourProviderProperty;

    SerializedProperty useGravityProperty;
    SerializedProperty timeToReachApexProperty;
    SerializedProperty lockonTargetProperty;
    SerializedProperty velocitySmoothingTimeProperty;
    SerializedProperty velocitySmoothingProperty;
    SerializedProperty isGroundedProperty;

    protected virtual void OnEnable() {
        movementProviderProperty = serializedObject.FindProperty("movementProvider");
        behaviourProviderProperty = serializedObject.FindProperty("behaviourProvider");

        useGravityProperty = serializedObject.FindProperty("useGravity");
        timeToReachApexProperty = serializedObject.FindProperty("timeToReachApex");
        velocitySmoothingTimeProperty = serializedObject.FindProperty("velocitySmoothTime");
        isGroundedProperty = serializedObject.FindProperty("isGroundedDisplay");
        velocitySmoothingProperty = serializedObject.FindProperty("velocitySmoothingDisplay");
        lockonTargetProperty = serializedObject.FindProperty("lockonTarget");
    }

    protected virtual void CustomInspector() {
        serializedObject.Update();

        EditorGUILayout.LabelField(new GUIContent("Controlling Scripts"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(movementProviderProperty, new GUIContent("Movement Provider"));
        EditorExtras.OpenPropertyButton(movementProviderProperty.objectReferenceValue, new GUIContent("Edit"));
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(behaviourProviderProperty, new GUIContent("Behaviour Provider"));
        EditorExtras.OpenPropertyButton(behaviourProviderProperty.objectReferenceValue, new GUIContent("Edit"));
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();

        EditorGUILayout.LabelField(new GUIContent("Physics"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(useGravityProperty, new GUIContent("Use Gravity?"));
        EditorGUILayout.PropertyField(timeToReachApexProperty, new GUIContent("Time to reach apex"));
        EditorGUILayout.PropertyField(velocitySmoothingTimeProperty, new GUIContent("Velocity Smooth Time"));

        EditorGUILayout.Space();
        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField(new GUIContent("Advanced"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUI.BeginDisabledGroup(true);

        EditorGUILayout.PropertyField(isGroundedProperty, new GUIContent("Is Grounded?"));
        EditorGUILayout.PropertyField(lockonTargetProperty, new GUIContent("Lockon Target"));
        EditorGUILayout.PropertyField(velocitySmoothingProperty, new GUIContent("Velocity Smoothing"));
        EditorGUI.EndDisabledGroup();

        EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();
    }
}