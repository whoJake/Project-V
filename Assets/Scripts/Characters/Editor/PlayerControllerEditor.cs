using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerController))]
public class PlayerControllerEditor : EntityControllerEditor
{
    SerializedProperty maxHealthProperty;
    SerializedProperty healthProperty;

    protected override void OnEnable() {
        base.OnEnable();
        maxHealthProperty = serializedObject.FindProperty("maxHealth");
        healthProperty = serializedObject.FindProperty("health");
    }

    protected override void CustomInspector() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(maxHealthProperty, new GUIContent("Max Health"));
        EditorGUILayout.PropertyField(healthProperty, new GUIContent("Health"));
        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();

        base.CustomInspector();
    }
}
