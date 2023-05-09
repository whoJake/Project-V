using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerMovement))]
public class PlayerMovementEditor : Editor
{
    bool useCustomInspector = true;

    public override void OnInspectorGUI() {
        useCustomInspector = EditorGUILayout.Toggle("Use Custom Editor?", useCustomInspector);
        EditorGUILayout.Space();

        if (useCustomInspector) CustomInspector();
        else base.OnInspectorGUI();
    }

    SerializedProperty velocityProperty;
    SerializedProperty timeToApexProperty;
    SerializedProperty gravityWhilstFallingMultiplier;
    SerializedProperty terminalVelocityProperty;
    SerializedProperty groundAccelerationTimeProperty;
    SerializedProperty sprintTransitionTimeProperty;

    SerializedProperty airbourneMovementPenaltyProperty;

    SerializedProperty hitGroundEventThresholdProperty;

    SerializedProperty slowDownDragProperty;
    SerializedProperty airDragProperty;
    SerializedProperty dragTransitionTimeProperty;

    void OnEnable() {
        velocityProperty = serializedObject.FindProperty("velocity");
        timeToApexProperty = serializedObject.FindProperty("timeToApex");
        gravityWhilstFallingMultiplier = serializedObject.FindProperty("gravityWhilstFallingMultiplier");
        terminalVelocityProperty = serializedObject.FindProperty("terminalVelocity");
        groundAccelerationTimeProperty = serializedObject.FindProperty("groundAccelerationTime");
        sprintTransitionTimeProperty = serializedObject.FindProperty("sprintTransitionTime");
        airbourneMovementPenaltyProperty = serializedObject.FindProperty("airbourneMovementPenalty");
        hitGroundEventThresholdProperty = serializedObject.FindProperty("hitGroundEventThreshold");
        slowDownDragProperty = serializedObject.FindProperty("slowDownDrag");
        airDragProperty = serializedObject.FindProperty("airDrag");
        dragTransitionTimeProperty = serializedObject.FindProperty("dragTransitionTime");
    }

    private void CustomInspector() {
        serializedObject.Update();

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(velocityProperty, new GUIContent("Velocity"));
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.PropertyField(timeToApexProperty, new GUIContent("Time to Apex"));
        EditorGUILayout.PropertyField(gravityWhilstFallingMultiplier, new GUIContent("Gravity Whilst Falling Multiplier"));
        EditorGUILayout.PropertyField(terminalVelocityProperty, new GUIContent("Terminal Velocity"));
        EditorGUILayout.PropertyField(groundAccelerationTimeProperty, new GUIContent("Ground Acceleration Time"));
        EditorGUILayout.PropertyField(sprintTransitionTimeProperty, new GUIContent("Sprint Transition Time"));
        EditorGUILayout.PropertyField(airbourneMovementPenaltyProperty, new GUIContent("Airbourne Movement Penalty"));
        EditorGUILayout.PropertyField(hitGroundEventThresholdProperty, new GUIContent("HitGround Event Threshold"));
        EditorGUILayout.PropertyField(slowDownDragProperty, new GUIContent("Slowdown Drag"));
        EditorGUILayout.PropertyField(airDragProperty, new GUIContent("Airbourne Drag"));
        EditorGUILayout.PropertyField(dragTransitionTimeProperty, new GUIContent("Drag Transition Time"));

        serializedObject.ApplyModifiedProperties();
    }
}
