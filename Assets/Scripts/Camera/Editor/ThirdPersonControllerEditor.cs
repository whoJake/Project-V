using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ThirdPersonControllerEditor : Editor
{
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        //CustomInspector();
    }

    SerializedProperty orbitsProperty;
    SerializedProperty mouseSensitivityProperty;

    void OnEnable() {
        orbitsProperty = serializedObject.FindProperty("orbits");
        mouseSensitivityProperty = serializedObject.FindProperty("mouseSensitivity");
    }

    private void CustomInspector() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(orbitsProperty, new GUIContent("Orbits"));
        EditorGUILayout.PropertyField(mouseSensitivityProperty, new GUIContent("Mouse Sensitivity"));

        serializedObject.ApplyModifiedProperties();
    }
}
