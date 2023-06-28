using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class EditorExtras
{
    public static void OpenPropertyButton(Object property, GUIContent contents, params GUILayoutOption[] options) {
        if (GUILayout.Button(contents, options))
            EditorUtility.OpenPropertyEditor(property);
    }

    public static void CreateFoldoutEditor(Editor editor, ref bool foldout, GUIContent content, GUIStyle style = null) {
        EditorGUI.indentLevel++;
        EditorGUILayout.BeginHorizontal();
        EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
        foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, content, style);
        EditorGUILayout.EndHorizontal();

        if (foldout) {
            editor.OnInspectorGUI();
        }
        EditorGUI.EndFoldoutHeaderGroup();

        EditorGUI.indentLevel--;
    }
}
