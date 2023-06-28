using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EntityController))]
public class EntityControllerEditor : Editor
{
    protected static bool useCustomInspector = true;

    public override void OnInspectorGUI() {
        useCustomInspector = EditorGUILayout.Toggle("Use Custom Inspector?", useCustomInspector);
        EditorGUILayout.Space();

        if (useCustomInspector)
            CustomInspector();
        else
            base.OnInspectorGUI();
    }

    SerializedProperty movementProviderProperty;
    SerializedProperty behaviourProviderProperty;

    SerializedProperty massProperty;
    SerializedProperty useGravityProperty;
    SerializedProperty lockonTargetProperty;
    SerializedProperty velocityProperty;
    SerializedProperty groundDragProperty;
    SerializedProperty airDragProperty;
    SerializedProperty ignoreForGroundedProperty;
    SerializedProperty maxStepHeightProperty;
    SerializedProperty maxSlopeAngleProperty;
    SerializedProperty currentSpeedProperty;
    SerializedProperty maxCollisionChecksProperty;
    SerializedProperty minimumMoveDistanceProperty;
    SerializedProperty skinWidthProperty;

    protected virtual void OnEnable() {
        movementProviderProperty = serializedObject.FindProperty("movementProvider");

        behaviourProviderProperty = serializedObject.FindProperty("behaviourProvider");

        massProperty = serializedObject.FindProperty("mass");
        useGravityProperty = serializedObject.FindProperty("useGravity");
        lockonTargetProperty = serializedObject.FindProperty("lockonTarget");
        velocityProperty = serializedObject.FindProperty("velocityDisplay");
        groundDragProperty = serializedObject.FindProperty("groundDrag");
        airDragProperty = serializedObject.FindProperty("airDrag");
        ignoreForGroundedProperty = serializedObject.FindProperty("ignoreForGrounded");
        maxStepHeightProperty = serializedObject.FindProperty("maxStepHeight");
        maxSlopeAngleProperty = serializedObject.FindProperty("maxSlopeAngle");
        currentSpeedProperty = serializedObject.FindProperty("currentSpeedDisplay");
        maxCollisionChecksProperty = serializedObject.FindProperty("maxCollisionChecks");
        minimumMoveDistanceProperty = serializedObject.FindProperty("minimumMoveDistance");
        skinWidthProperty = serializedObject.FindProperty("skinWidth");
    }

    protected virtual void CustomInspector() {
        serializedObject.Update();

        EditorGUILayout.LabelField(new GUIContent("Controlling Scripts"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(movementProviderProperty, new GUIContent("Movement Provider"));
        EditorExtras.OpenPropertyButton(movementProviderProperty.objectReferenceValue, new GUIContent("Edit"));
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(behaviourProviderProperty, new GUIContent("Behaviour Provider"));
        EditorExtras.OpenPropertyButton(behaviourProviderProperty.objectReferenceValue, new GUIContent("Edit"));
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();

        EditorGUILayout.LabelField(new GUIContent("Physics"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(useGravityProperty, new GUIContent("Use Gravity?"));
        EditorGUILayout.PropertyField(massProperty, new GUIContent("Mass"));
        EditorGUILayout.PropertyField(groundDragProperty, new GUIContent("Ground Drag"));
        EditorGUILayout.PropertyField(airDragProperty, new GUIContent("Air Drag"));
        EditorGUILayout.PropertyField(ignoreForGroundedProperty, new GUIContent("Ignore Grounded Mask"));

        EditorGUILayout.Space();
        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField(new GUIContent("Advanced"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(velocityProperty, new GUIContent("Velocity"));
        
        EditorGUILayout.PropertyField(currentSpeedProperty, new GUIContent("Speed"));
        EditorGUILayout.PropertyField(lockonTargetProperty, new GUIContent("Lockon Target"));
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.PropertyField(skinWidthProperty, new GUIContent("Skin Width"));
        EditorGUILayout.PropertyField(minimumMoveDistanceProperty, new GUIContent("Min Move Distance"));
        EditorGUILayout.PropertyField(maxStepHeightProperty, new GUIContent("Max Step Height"));
        EditorGUILayout.PropertyField(maxSlopeAngleProperty, new GUIContent("Max Slope Angle"));
        EditorGUILayout.PropertyField(maxCollisionChecksProperty, new GUIContent("Max Collision Checks"));

        EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();
    }
}
