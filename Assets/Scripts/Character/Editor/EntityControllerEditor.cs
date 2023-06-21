using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EntityController))]
public class EntityControllerEditor : Editor
{
    
    private static bool useCustomInspector;
    private static bool movementProviderEditorOpen;
    private static bool behaviourProviderEditorOpen;

    public override void OnInspectorGUI() {
        useCustomInspector = EditorGUILayout.Toggle("Use Custom Inspector?", useCustomInspector);
        EditorGUILayout.Space();

        if (useCustomInspector)
            CustomInspector();
        else
            base.OnInspectorGUI();
    }

    SerializedProperty movementProviderProperty;
    Editor movementProviderEditor;

    SerializedProperty behaviourProviderProperty;
    Editor behaviourProviderEditor;

    SerializedProperty massProperty;
    SerializedProperty useGravityProperty;
    SerializedProperty velocityProperty;
    SerializedProperty dragProperty;
    SerializedProperty ignoreForGroundedProperty;
    SerializedProperty currentSpeedProperty;
    SerializedProperty maxCollisionChecksProperty;
    SerializedProperty minimumMoveDistanceProperty;
    SerializedProperty skinWidthProperty;

    private void OnEnable() {
        movementProviderProperty = serializedObject.FindProperty("movementProvider");
        Editor.CreateCachedEditor((MovementProvider) movementProviderProperty.objectReferenceValue, null, ref movementProviderEditor);

        behaviourProviderProperty = serializedObject.FindProperty("behaviourProvider");
        Editor.CreateCachedEditor((BehaviourProvider)behaviourProviderProperty.objectReferenceValue, null, ref behaviourProviderEditor);

        massProperty = serializedObject.FindProperty("mass");
        useGravityProperty = serializedObject.FindProperty("useGravity");
        velocityProperty = serializedObject.FindProperty("velocity");
        dragProperty = serializedObject.FindProperty("drag");
        ignoreForGroundedProperty = serializedObject.FindProperty("ignoreForGrounded");
        currentSpeedProperty = serializedObject.FindProperty("currentSpeed");
        maxCollisionChecksProperty = serializedObject.FindProperty("maxCollisionChecks");
        minimumMoveDistanceProperty = serializedObject.FindProperty("minimumMoveDistance");
        skinWidthProperty = serializedObject.FindProperty("skinWidth");
    }

    private void CreateFoldoutEditor(Editor editor, ref bool foldout, string title) {
        EditorGUI.indentLevel++;
        EditorGUILayout.BeginHorizontal();
        EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
        foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, new GUIContent(title));
        EditorGUILayout.EndHorizontal();

        if (foldout) {
            editor.OnInspectorGUI();
        }
        EditorGUI.EndFoldoutHeaderGroup();

        EditorGUI.indentLevel--;
    }

    private void CustomInspector() {
        EditorGUILayout.LabelField(new GUIContent("Controlling Scripts"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(movementProviderProperty, new GUIContent("Movement Provider"));
        CreateFoldoutEditor(movementProviderEditor, ref movementProviderEditorOpen, "Edit");
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(behaviourProviderProperty, new GUIContent("Behaviour Provider"));
        CreateFoldoutEditor(behaviourProviderEditor, ref behaviourProviderEditorOpen, "Edit");
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();

        EditorGUILayout.LabelField(new GUIContent("Physics"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(useGravityProperty, new GUIContent("Use Gravity?"));
        EditorGUILayout.PropertyField(massProperty, new GUIContent("Mass"));
        EditorGUILayout.PropertyField(dragProperty, new GUIContent("Drag"));
        EditorGUILayout.PropertyField(ignoreForGroundedProperty, new GUIContent("Ignore Grounded Mask"));

        EditorGUILayout.Space();
        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField(new GUIContent("Advanced"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(velocityProperty, new GUIContent("Velocity"));
        EditorGUILayout.PropertyField(currentSpeedProperty, new GUIContent("Speed"));
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.PropertyField(skinWidthProperty, new GUIContent("Skin Width"));
        EditorGUILayout.PropertyField(minimumMoveDistanceProperty, new GUIContent("Min Move Distance"));
        EditorGUILayout.PropertyField(maxCollisionChecksProperty, new GUIContent("Max Collision Checks"));

        EditorGUI.indentLevel--;

    }
}
