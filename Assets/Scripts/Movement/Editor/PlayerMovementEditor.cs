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
    SerializedProperty gravityProperty;
    SerializedProperty terminalVelocityProperty;
    SerializedProperty jumpHeightProperty;
    SerializedProperty groundAccelerationTimeProperty;
    SerializedProperty hitGroundEventThresholdProperty;

    void OnEnable() {
        velocityProperty = serializedObject.FindProperty("velocity");
        gravityProperty = serializedObject.FindProperty("gravity");
        terminalVelocityProperty = serializedObject.FindProperty("terminalVelocity");
        jumpHeightProperty = serializedObject.FindProperty("jumpHeight");
        groundAccelerationTimeProperty = serializedObject.FindProperty("groundAccelerationTime");
        hitGroundEventThresholdProperty = serializedObject.FindProperty("hitGroundEventThreshold");
    }

    private void CustomInspector() {
        serializedObject.Update();

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(velocityProperty, new GUIContent("Velocity"));
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.PropertyField(gravityProperty, new GUIContent("Gravity"));
        EditorGUILayout.PropertyField(terminalVelocityProperty, new GUIContent("Terminal Velocity"));
        EditorGUILayout.PropertyField(jumpHeightProperty, new GUIContent("Jump Height"));
        EditorGUILayout.PropertyField(groundAccelerationTimeProperty, new GUIContent("Ground Acceleration Time"));
        EditorGUILayout.PropertyField(hitGroundEventThresholdProperty, new GUIContent("HitGround Event Threshold"));

        serializedObject.ApplyModifiedProperties();
    }
}
