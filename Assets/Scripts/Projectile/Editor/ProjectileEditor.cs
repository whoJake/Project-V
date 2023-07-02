using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Projectile))]
public class ProjectileEditor : Editor
{
    private static bool useCustomInspector = true;

    public override void OnInspectorGUI() {
        useCustomInspector = EditorGUILayout.Toggle("Use Custom Inspector?", useCustomInspector);
        EditorGUILayout.Space();

        if (useCustomInspector)
            CustomInspector();
        else
            base.OnInspectorGUI();
    }

    private SerializedProperty ignoreTagsProperty;
    private SerializedProperty useGravityProperty;
    private SerializedProperty setGravityProperty;
    private SerializedProperty gravityProperty;
    private SerializedProperty speedProperty;
    private SerializedProperty apex2targetHeightProperty;
    private SerializedProperty apex2targetTimeProperty;
    private SerializedProperty lifetimeProperty;
    private SerializedProperty damageProperty;
    private SerializedProperty destructionRadiusProperty;
    private SerializedProperty destroyOnCollisionProperty;
    private SerializedProperty verticalVelocityProperty;
    private SerializedProperty horizontalVelocityProperty;

    private void OnEnable() {
        ignoreTagsProperty = serializedObject.FindProperty("ignoreTags");
        useGravityProperty = serializedObject.FindProperty("useGravity");
        setGravityProperty = serializedObject.FindProperty("setGravity");
        gravityProperty = serializedObject.FindProperty("gravity");
        speedProperty = serializedObject.FindProperty("speed");
        apex2targetHeightProperty = serializedObject.FindProperty("apex2targetHeight");
        apex2targetTimeProperty = serializedObject.FindProperty("apex2targetTime");
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

            EditorGUILayout.PropertyField(apex2targetTimeProperty, new GUIContent("Time from apex to target"));
            EditorGUILayout.PropertyField(setGravityProperty, new GUIContent("Set Gravity?"));
            if (setGravityProperty.boolValue) {
                EditorGUILayout.PropertyField(gravityProperty, new GUIContent("Gravity"));
            } else {
                EditorGUILayout.PropertyField(apex2targetHeightProperty, new GUIContent("Height of apex to target"));
            }
            EditorGUI.indentLevel--;
        } else {
            EditorGUILayout.PropertyField(speedProperty, new GUIContent("Speed"));
        }
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(lifetimeProperty, new GUIContent("Lifetime"));
        EditorGUILayout.PropertyField(damageProperty, new GUIContent("Damage Dealt"));
        EditorGUILayout.PropertyField(destroyOnCollisionProperty, new GUIContent("Destroy On Collision"));
        if (destroyOnCollisionProperty.boolValue) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(ignoreTagsProperty, new GUIContent("Ignore Collision Tags"));
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.PropertyField(destructionRadiusProperty, new GUIContent("Terrain Destruction Radius"));

        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(horizontalVelocityProperty, new GUIContent("Horizontal Velocity"));
        EditorGUILayout.PropertyField(verticalVelocityProperty, new GUIContent("Vertical Velocity"));
        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();
    }
}
