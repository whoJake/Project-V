using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ThirdPersonController))]
public class ThirdPersonControllerEditor : Editor
{
    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();
        CustomInspector();
    }

    SerializedProperty orbitsProperty;
    SerializedProperty orbitAroundProperty;
    SerializedProperty affectedCameraProperty;
    SerializedProperty mouseSensitivityProperty;
    SerializedProperty focusPointProperty;
    SerializedProperty willControlTransformDirectionProperty;
    SerializedProperty controlTransformProperty;
    SerializedProperty controlTransformTypeProperty;
    SerializedProperty showOrbitWireframesProperty;

    void OnEnable() {
        orbitsProperty = serializedObject.FindProperty("orbits");
        orbitAroundProperty = serializedObject.FindProperty("orbitAround");
        affectedCameraProperty = serializedObject.FindProperty("affectedCamera");
        mouseSensitivityProperty = serializedObject.FindProperty("mouseSensitivity");
        focusPointProperty = serializedObject.FindProperty("focusPoint");
        willControlTransformDirectionProperty = serializedObject.FindProperty("willControlTransformDirection");
        controlTransformProperty = serializedObject.FindProperty("controlTransform");
        controlTransformTypeProperty = serializedObject.FindProperty("controlTransformType");
        showOrbitWireframesProperty = serializedObject.FindProperty("showOrbitWireframes");
    }

    private void CustomInspector() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(orbitsProperty, new GUIContent("Orbits"));
        EditorGUILayout.PropertyField(orbitAroundProperty, new GUIContent("Orbit Around Transform"));
        EditorGUILayout.PropertyField(affectedCameraProperty, new GUIContent("Affected Camera"));
        EditorGUILayout.PropertyField(mouseSensitivityProperty, new GUIContent("Mouse Sensitivity"));
        EditorGUILayout.PropertyField(focusPointProperty, new GUIContent("Focus Point"));
        EditorGUILayout.PropertyField(willControlTransformDirectionProperty, new GUIContent("Will Control Transform Direction?"));
        if (willControlTransformDirectionProperty.boolValue) {
            EditorGUILayout.PropertyField(controlTransformProperty, new GUIContent("Control Transform"));
            EditorGUILayout.PropertyField(controlTransformTypeProperty, new GUIContent("Control Transform Type"));
        }

        EditorGUILayout.PropertyField(showOrbitWireframesProperty, new GUIContent("Show Orbit Wireframes"));

        serializedObject.ApplyModifiedProperties();
    }
}
