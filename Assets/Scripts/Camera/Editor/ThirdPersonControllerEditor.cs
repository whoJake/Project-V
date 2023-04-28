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

    void OnEnable() {
        orbitsProperty = serializedObject.FindProperty("orbits");
    }

    private void CustomInspector() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(orbitsProperty, new GUIContent("Orbits"));

        serializedObject.ApplyModifiedProperties();
    }
}
