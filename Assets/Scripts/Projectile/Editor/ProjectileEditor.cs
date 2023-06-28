using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Projectile))]
public class ProjectileEditor : Editor
{
    private static bool useCustomInspector;

    public override void OnInspectorGUI() {
        useCustomInspector = EditorGUILayout.Toggle("Use Custom Inspector?", useCustomInspector);
        EditorGUILayout.Space();

        if (useCustomInspector)
            CustomInspector();
        else
            base.OnInspectorGUI();
    }

    private SerializedProperty useGravityProperty;
    private SerializedProperty setGravityProperty;
    private SerializedProperty gravityProperty;
    private SerializedProperty speedProperty;
    private SerializedProperty heightOfApexProperty;
    private SerializedProperty timeToReachApexProperty;
    private SerializedProperty lifetimeProperty;
    private SerializedProperty damageProperty;
    private SerializedProperty destructionRadiusProperty;
    private SerializedProperty destroyOnCollisionProperty;
    private SerializedProperty verticalVelocityProperty;
    private SerializedProperty horizontalVelocityProperty;

    private void OnEnable() {
        useGravityProperty = serializedObject.FindProperty("useGravity");
        setGravityProperty = serializedObject.FindProperty("setGravity");
        gravityProperty = serializedObject.FindProperty("gravity");
        speedProperty = serializedObject.FindProperty("speed");
        heightOfApexProperty = serializedObject.FindProperty("heightOfApex");
        timeToReachApexProperty = serializedObject.FindProperty("timeToReachApex");
        lifetimeProperty = serializedObject.FindProperty("lifetime");
        damageProperty = serializedObject.FindProperty("damage");
        destructionRadiusProperty = serializedObject.FindProperty("destructionRadius");
        destroyOnCollisionProperty = serializedObject.FindProperty("destroyOnCollision");
        verticalVelocityProperty = serializedObject.FindProperty("verticalVelocity");
        horizontalVelocityProperty = serializedObject.FindProperty("horizontalVelocity");
    }

    private void CustomInspector() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(useGravityProperty, new GUIContent("Use Gravity?"));
        if (useGravityProperty.boolValue) {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(timeToReachApexProperty, new GUIContent("Time to reach apex"));
            EditorGUILayout.PropertyField(setGravityProperty, new GUIContent("Set Gravity?"));
            if (setGravityProperty.boolValue) {
                EditorGUILayout.PropertyField(gravityProperty, new GUIContent("Gravity"));
            } else {
                EditorGUILayout.PropertyField(heightOfApexProperty, new GUIContent("Height of apex"));
            }
            EditorGUI.indentLevel--;
        } else {
            EditorGUILayout.PropertyField(speedProperty, new GUIContent("Speed"));
        }
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(lifetimeProperty, new GUIContent("Lifetime"));
        EditorGUILayout.PropertyField(damageProperty, new GUIContent("Damage Dealt"));
        EditorGUILayout.PropertyField(destroyOnCollisionProperty, new GUIContent("Destroy On Collision"));
        EditorGUILayout.PropertyField(destructionRadiusProperty, new GUIContent("Terrain Destruction Radius"));

        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(horizontalVelocityProperty, new GUIContent("Horizontal Velocity"));
        EditorGUILayout.PropertyField(verticalVelocityProperty, new GUIContent("Vertical Velocity"));
        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();
    }
}
