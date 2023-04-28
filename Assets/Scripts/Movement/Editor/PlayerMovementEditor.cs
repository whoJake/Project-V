using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlayerMovementEditor : Editor
{
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        //CustomInspector();
    }

    SerializedProperty speedProperty;
    SerializedProperty accelerationTimeProperty;
    SerializedProperty gravityProperty;

    void OnEnable() {
        speedProperty = serializedObject.FindProperty("speed");
        accelerationTimeProperty = serializedObject.FindProperty("accelerationTime");
        gravityProperty = serializedObject.FindProperty("gravity");
    }

    private void CustomEditor() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(speedProperty, new GUIContent("Speed"));
        EditorGUILayout.PropertyField(accelerationTimeProperty, new GUIContent("Acceleration Time"));
        EditorGUILayout.PropertyField(gravityProperty, new GUIContent("Gravity"));

        serializedObject.ApplyModifiedProperties();
    }
}
