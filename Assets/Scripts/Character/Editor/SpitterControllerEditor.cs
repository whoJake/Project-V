using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpitterController))]
public class SpitterControllerEditor : EntityControllerEditor
{
    SerializedProperty attackTargetProperty;

    protected override void OnEnable() {
        base.OnEnable();
        attackTargetProperty = serializedObject.FindProperty("attackTarget");
    }

    protected override void CustomInspector() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(attackTargetProperty, new GUIContent("Attack Target"));
        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();

        base.CustomInspector();
    }

}
