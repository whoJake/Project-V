using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/*
 * Created simply so that if any custom editor elements are needed, it is all set up already to simply add that feature
 */
[CustomEditor(typeof(MovementController))]
public class MovementControllerEditor : Editor
{
    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();
        CustomEditor();
    }

    SerializedProperty maxSpeedProperty;
    SerializedProperty accelerationProperty;
    SerializedProperty skinWidthProperty;
    SerializedProperty rayCountProperty;

    void OnEnable() {
        maxSpeedProperty = serializedObject.FindProperty("maxSpeed");
        accelerationProperty = serializedObject.FindProperty("acceleration");
        skinWidthProperty = serializedObject.FindProperty("skinWidth");
        rayCountProperty = serializedObject.FindProperty("rayCount");
    }

    private void CustomEditor() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(maxSpeedProperty, new GUIContent("Max Speed"));
        EditorGUILayout.PropertyField(accelerationProperty, new GUIContent("Acceleration"));
        EditorGUILayout.PropertyField(skinWidthProperty, new GUIContent("Skin Width"));
        EditorGUILayout.PropertyField(rayCountProperty, new GUIContent("Ray Count"));

        serializedObject.ApplyModifiedProperties();
    }
}
