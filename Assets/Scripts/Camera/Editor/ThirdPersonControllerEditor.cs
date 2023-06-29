using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ThirdPersonController))]
public class ThirdPersonControllerEditor : Editor
{
    private static bool usingCustomInspector;

    public override void OnInspectorGUI() {
        usingCustomInspector = EditorGUILayout.Toggle("Use Custom Inspector?", usingCustomInspector);
        EditorGUILayout.Space();

        if (usingCustomInspector)
            CustomInspector();
        else
            base.OnInspectorGUI();
    }

    SerializedProperty controlCameraProperty;
    SerializedProperty focusPointProperty;
    SerializedProperty verticalLookClampsProperty;
    SerializedProperty invertYProperty;
    SerializedProperty mouseSensitivityProperty;
    SerializedProperty rotationPointProperty;
    SerializedProperty lengthProperty;
    SerializedProperty willControlTransformProperty;
    SerializedProperty controlTransformProperty;
    SerializedProperty controlTransformTypeProperty;
    SerializedProperty shouldAvoidOcclusionProperty;
    SerializedProperty occlusionLayerMaskProperty;
    SerializedProperty avoidBufferProperty;
    SerializedProperty avoidSmoothingTimeProperty;

    void OnEnable() {
        controlCameraProperty = serializedObject.FindProperty("controlCamera");
        focusPointProperty = serializedObject.FindProperty("focusPoint");
        verticalLookClampsProperty = serializedObject.FindProperty("verticalLookClamps");
        invertYProperty = serializedObject.FindProperty("invertY");
        mouseSensitivityProperty = serializedObject.FindProperty("mouseSensitivity");
        rotationPointProperty = serializedObject.FindProperty("rotationPoint");
        lengthProperty = serializedObject.FindProperty("length");
        willControlTransformProperty = serializedObject.FindProperty("willControlTransform");
        controlTransformProperty = serializedObject.FindProperty("controlTransform");
        controlTransformTypeProperty = serializedObject.FindProperty("controlTransformType");
        shouldAvoidOcclusionProperty = serializedObject.FindProperty("shouldAvoidOcclusion");
        occlusionLayerMaskProperty = serializedObject.FindProperty("occlusionLayerMask");
        avoidBufferProperty = serializedObject.FindProperty("avoidBuffer");
        avoidSmoothingTimeProperty = serializedObject.FindProperty("avoidSmoothingTime");
    }

    private void CustomInspector() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(controlCameraProperty, new GUIContent("Control Camera"));
        EditorGUILayout.PropertyField(focusPointProperty, new GUIContent("Focus Point"));
        EditorGUILayout.PropertyField(verticalLookClampsProperty, new GUIContent("Vertical Look Clamps"));
        EditorGUILayout.PropertyField(invertYProperty, new GUIContent("Invert Y?"));
        EditorGUILayout.PropertyField(mouseSensitivityProperty, new GUIContent("X/Y Mouse Sensitivity"));
        EditorGUILayout.PropertyField(rotationPointProperty, new GUIContent("Point of Rotation"));
        EditorGUILayout.PropertyField(lengthProperty, new GUIContent("Camera Distance"));

        EditorGUILayout.PropertyField(willControlTransformProperty, new GUIContent("Will Control Transform Direction?"));
        if (willControlTransformProperty.boolValue) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(controlTransformProperty, new GUIContent("Transform"));
            EditorGUILayout.PropertyField(controlTransformTypeProperty, new GUIContent("Control Type"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(shouldAvoidOcclusionProperty, new GUIContent("Should Avoid Occlusion?"));
        if (shouldAvoidOcclusionProperty.boolValue) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(occlusionLayerMaskProperty, new GUIContent("Layer Mask"));
            EditorGUILayout.PropertyField(avoidBufferProperty, new GUIContent("Buffer Length"));
            EditorGUILayout.PropertyField(avoidSmoothingTimeProperty, new GUIContent("Smoothing Time"));
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
