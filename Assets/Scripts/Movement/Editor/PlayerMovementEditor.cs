using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerMovement))]
public class PlayerMovementEditor : Editor
{
    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();
        CustomInspector();
    }

    SerializedProperty velocityProperty;
    SerializedProperty timeToApexProperty;
    SerializedProperty gravityWhilstFallingMultiplier;
    SerializedProperty terminalVelocityProperty;
    SerializedProperty groundAccelerationTimeProperty;
    SerializedProperty hitGroundEventThresholdProperty;

    void OnEnable() {
        velocityProperty = serializedObject.FindProperty("velocity");
        timeToApexProperty = serializedObject.FindProperty("timeToApex");
        gravityWhilstFallingMultiplier = serializedObject.FindProperty("gravityWhilstFallingMultiplier");
        terminalVelocityProperty = serializedObject.FindProperty("terminalVelocity");
        groundAccelerationTimeProperty = serializedObject.FindProperty("groundAccelerationTime");
        hitGroundEventThresholdProperty = serializedObject.FindProperty("hitGroundEventThreshold");
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
        EditorGUILayout.PropertyField(hitGroundEventThresholdProperty, new GUIContent("HitGround Event Threshold"));

        serializedObject.ApplyModifiedProperties();
    }
}
