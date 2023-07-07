using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StarterTerrain))]
public class StarterTerrainEditor : Editor 
{
    private bool useCustomInspector = true;

    SerializedProperty numOfPlatformsProperty;
    SerializedProperty platformRadiusRangeProperty;
    SerializedProperty platformShapeFeatureStrengthProperty;
    SerializedProperty platformFlatnessRangeProperty;
    SerializedProperty platformTopDisplacementProperty;

    SerializedProperty platformStemPinchRangeProperty;
    SerializedProperty platformStemRadiusProperty;
    SerializedProperty platformStemFeatureDepthProperty;

    SerializedProperty platformPathHorizontalDifferenceRangeProperty;
    SerializedProperty platformPathVerticalDifferenceRangeProperty;
    SerializedProperty platformPathDistanceFromWallRangeProperty;
    SerializedProperty platformPathSwitchDirectionChanceProperty;
    SerializedProperty bonusPlatformMinDistanceProperty;


    SerializedProperty platformPathConnectingCountRangeProperty;
    SerializedProperty platformPathConnectorsRadiusRangeProperty;
    SerializedProperty platformPathConnectorsFlatnessRangeProperty;

    SerializedProperty depthRangeProperty;
    SerializedProperty chasmRadiusRangeProperty;

    SerializedProperty upperSurfaceDepthProperty;
    SerializedProperty upperSurfaceFeatureDepthProperty;

    SerializedProperty upperRadiusProperty;
    SerializedProperty lowerRadiusProperty;
    SerializedProperty cliffSlopeEasePowerProperty;
    SerializedProperty cliffFeatureDepthProperty;
    SerializedProperty cliffLedgeSizeProperty;

    SerializedProperty starterPlatformProperty;

    SerializedProperty noiseArgsProperty;

    public override void OnInspectorGUI() {
        useCustomInspector = EditorGUILayout.Toggle("Use Custom Inspector?", useCustomInspector);
        EditorGUILayout.Space();

        if (useCustomInspector) CustomInspector();
        else                    base.OnInspectorGUI();
    }

    private void OnEnable() {
        numOfPlatformsProperty = serializedObject.FindProperty("numOfPlatforms");
        platformRadiusRangeProperty = serializedObject.FindProperty("platformRadiusRange");
        platformShapeFeatureStrengthProperty = serializedObject.FindProperty("platformShapeFeatureStrength");
        platformFlatnessRangeProperty = serializedObject.FindProperty("platformFlatnessRange");
        platformTopDisplacementProperty = serializedObject.FindProperty("platformTopDisplacement");

        platformStemPinchRangeProperty = serializedObject.FindProperty("platformStemPinchRange");
        platformStemRadiusProperty = serializedObject.FindProperty("platformStemRadius");
        platformStemFeatureDepthProperty = serializedObject.FindProperty("platformStemFeatureDepth");

        platformPathHorizontalDifferenceRangeProperty = serializedObject.FindProperty("platformPathHorizontalDifferenceRange");
        platformPathVerticalDifferenceRangeProperty = serializedObject.FindProperty("platformPathVerticalDifferenceRange");
        platformPathDistanceFromWallRangeProperty = serializedObject.FindProperty("platformPathDistanceFromWallRange");
        platformPathSwitchDirectionChanceProperty = serializedObject.FindProperty("platformPathSwitchDirectionChance");
        bonusPlatformMinDistanceProperty = serializedObject.FindProperty("bonusPlatformMinDistance");

        platformPathConnectingCountRangeProperty = serializedObject.FindProperty("platformPathConnectingCountRange");
        platformPathConnectorsRadiusRangeProperty = serializedObject.FindProperty("platformPathConnectorsRadiusRange");
        platformPathConnectorsFlatnessRangeProperty = serializedObject.FindProperty("platformPathConnectorsFlatnessRange");

        depthRangeProperty = serializedObject.FindProperty("depthRange");
        chasmRadiusRangeProperty = serializedObject.FindProperty("chasmRadiusRange");

        upperSurfaceDepthProperty = serializedObject.FindProperty("upperSurfaceDepth");
        upperSurfaceFeatureDepthProperty = serializedObject.FindProperty("upperSurfaceFeatureDepth");
        
        upperRadiusProperty = serializedObject.FindProperty("upperRadius");
        lowerRadiusProperty = serializedObject.FindProperty("lowerRadius");
        cliffSlopeEasePowerProperty = serializedObject.FindProperty("cliffSlopeEasePower");
        cliffFeatureDepthProperty = serializedObject.FindProperty("cliffFeatureDepth");
        cliffLedgeSizeProperty = serializedObject.FindProperty("cliffLedgeSize");

        starterPlatformProperty = serializedObject.FindProperty("starterPlatform");

        noiseArgsProperty = serializedObject.FindProperty("noiseArgs");
    }

    private void CustomInspector() {
        serializedObject.Update();

        EditorGUILayout.LabelField(new GUIContent("Platform Generation"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(numOfPlatformsProperty, new GUIContent("Count"));
        EditorGUILayout.PropertyField(platformRadiusRangeProperty, new GUIContent("Radius Range"));
        EditorGUILayout.PropertyField(platformShapeFeatureStrengthProperty, new GUIContent("Feature Strength"));
        EditorGUILayout.PropertyField(platformFlatnessRangeProperty, new GUIContent("Flatness Range"));
        EditorGUILayout.PropertyField(platformTopDisplacementProperty, new GUIContent("Surface Max Displacement"));
        EditorGUILayout.LabelField(new GUIContent("Stem"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(platformStemPinchRangeProperty, new GUIContent("Pinch Range"));
        platformStemPinchRangeProperty.vector2Value = new Vector2(Mathf.Clamp(platformStemPinchRangeProperty.vector2Value.x, -0.5f, 1.5f),
                                                                  Mathf.Clamp(platformStemPinchRangeProperty.vector2Value.y, -0.5f, 1.5f));
        EditorGUILayout.PropertyField(platformStemRadiusProperty, new GUIContent("Radius"));
        EditorGUILayout.PropertyField(platformStemFeatureDepthProperty, new GUIContent("Feature Depth"));
        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField(new GUIContent("Placement"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(bonusPlatformMinDistanceProperty, new GUIContent("Absolute Minimum Distance"));
        EditorGUILayout.PropertyField(platformPathHorizontalDifferenceRangeProperty, new GUIContent("Horizontal Distance Range"));
        EditorGUILayout.PropertyField(platformPathVerticalDifferenceRangeProperty, new GUIContent("Veritcal Distance Range"));
        EditorGUILayout.PropertyField(platformPathDistanceFromWallRangeProperty, new GUIContent("Distance From Wall Range"));
        EditorGUILayout.PropertyField(platformPathSwitchDirectionChanceProperty, new GUIContent("Switch Direction Chance"));
        EditorGUILayout.LabelField(new GUIContent("Connectors"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(platformPathConnectingCountRangeProperty, new GUIContent("Count Range"));
        EditorGUILayout.PropertyField(platformPathConnectorsRadiusRangeProperty, new GUIContent("Radius Range"));
        EditorGUILayout.PropertyField(platformPathConnectorsFlatnessRangeProperty, new GUIContent("Flatness Range"));
        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();

        EditorGUILayout.LabelField(new GUIContent("Top Surface"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(upperSurfaceDepthProperty, new GUIContent("Depth"));
        EditorGUILayout.PropertyField(upperSurfaceFeatureDepthProperty, new GUIContent("Feature Depth"));
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();

        EditorGUILayout.LabelField(new GUIContent("Main Shape"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(upperRadiusProperty, new GUIContent("Upper Radius"));
        EditorGUILayout.PropertyField(lowerRadiusProperty, new GUIContent("Lower Radius"));
        EditorGUILayout.PropertyField(cliffSlopeEasePowerProperty, new GUIContent("Slope Ease Power"));
        EditorGUILayout.PropertyField(cliffLedgeSizeProperty, new GUIContent("Cliff Ledge Size"));
        EditorGUILayout.PropertyField(cliffFeatureDepthProperty, new GUIContent("Cliff Face Feature Depth"));
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(depthRangeProperty, new GUIContent("Depth Range"));
        EditorGUILayout.PropertyField(chasmRadiusRangeProperty, new GUIContent("Chasm Radius Range"));
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(starterPlatformProperty, new GUIContent("Starter Platform Settings"));
        EditorGUILayout.Space();

        EditorGUILayout.LabelField(new GUIContent("Noise Arguments"), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(noiseArgsProperty, new GUIContent("List"));
        EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();
    }
}