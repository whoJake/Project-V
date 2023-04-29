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
    SerializedProperty cameraHolderProperty;
    SerializedProperty mouseSensitivityProperty;

    SerializedProperty showOrbitWireframesProperty;

    void OnEnable() {
        orbitsProperty = serializedObject.FindProperty("orbits");
        cameraHolderProperty = serializedObject.FindProperty("cameraHolder");
        mouseSensitivityProperty = serializedObject.FindProperty("mouseSensitivity");

        showOrbitWireframesProperty = serializedObject.FindProperty("showOrbitWireframes");
    }

    private void CustomInspector() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(orbitsProperty, new GUIContent("Orbits"));
        EditorGUILayout.PropertyField(cameraHolderProperty, new GUIContent("Camera Holder"));
        EditorGUILayout.PropertyField(mouseSensitivityProperty, new GUIContent("Mouse Sensitivity"));

        EditorGUILayout.PropertyField(showOrbitWireframesProperty, new GUIContent("Show Orbit Wireframes"));

        serializedObject.ApplyModifiedProperties();
    }
}
