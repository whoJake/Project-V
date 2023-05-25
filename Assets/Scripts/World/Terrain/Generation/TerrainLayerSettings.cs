using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(menuName =  "Terrain/Layer Settings", order = 1)]
public class TerrainLayerSettings : ScriptableObject
{
    public static int stride { get { return sizeof(float) * 19 + sizeof(int) * 3; } }

    [HideInInspector] 
    public Vector3 origin;

    [Tooltip("Target Depth of this layer")]
    [Min(0)] public float depth;

    [HideInInspector] 
    public float genDepth;

    [Tooltip("Size of the transition into the above layer")]
    [Min(0)] public float topTransition;
    [Tooltip("Size of the transition into the below layer")]
    [Min(0)] public float bottomTransition;
    
    [Min(0)] public float chasmRadius;

    [Min(0)] public float groundThickness;
    [Tooltip("Depth of the centre of the created ground plane")]
    public float groundDepth;

    [Tooltip("Maximum change in height from groundDepth that can occur")]
    [Min(0)] public float groundHeightChangeMax;
    [Tooltip("Size of height change noise")]
    [Range(0f, 1f)] public float groundHeightChangeScale;
    [Tooltip("Complexity of the noise used to change the height (0 = No height change)")]
    [Min(0)] public int groundHeightChangeComplexity;
    [Tooltip("Strength of the distortion effect applied to height change noise")]
    [Min(0)] public float groundHeightChangeDistortionStrength;

    [Range(0f, 1f)] public float surfaceRoughness;
    [Tooltip("Maximum depth of the surface roughness features")]
    [Min(0)] public float surfaceFeatureDepth;

    [Tooltip("Density of pillar spawning")]
    [Range(0f, 1f)] public float pillarDensity;
    [Tooltip("Size of pillars spawned (Also slightly affected by density)")]
    [Range(0f, 1f)] public float pillarScale;
    [Tooltip("What sides of the ground will pillars not be spawned on")]
    public PillarIgnoreState pillarIgnoreState;

    [Min(0)] public int octaves;
    [Min(0)] public float frequency;
    [Range(0f, 2f)] public float persistance;
    [Min(0)] public float lacunarity;

    public static TerrainLayerSettings GetAllRandom() {
        TerrainLayerSettings result = ScriptableObject.CreateInstance<TerrainLayerSettings>();
        result.depth = Random.Range(128f, 256f);
        result.topTransition = Random.Range(4f, 16f);
        result.bottomTransition = 0f;
        result.chasmRadius = Random.Range(200f, 300f);

        result.groundDepth = Random.Range(32f, result.depth - 32f);

        float maxGroundThickness = Mathf.Min(result.groundDepth, result.depth - result.groundDepth) * 2f;
        result.groundThickness = Random.Range(16f, maxGroundThickness);

        result.groundHeightChangeComplexity = Random.Range(0, 5);
        result.groundHeightChangeDistortionStrength = Random.Range(0f, 2f);
        result.groundHeightChangeScale = Random.Range(0f, 1f);

        float maxGroundHeightChange = Mathf.Max(0f, Mathf.Min(result.groundDepth - (result.groundThickness / 2f), result.depth - result.groundDepth - (result.groundThickness / 2f)));
        result.groundHeightChangeMax = Random.Range(0f, maxGroundHeightChange);

        result.surfaceFeatureDepth = Random.Range(0f, result.groundThickness);
        result.surfaceRoughness = Random.Range(0f, 0.7f);

        result.pillarDensity = Random.Range(0f, 0.6f);
        result.pillarScale = Random.Range(0f, 1f);

        float pillarIgnoreStateRandom = Random.Range(0f, 1f);
        if(pillarIgnoreStateRandom < 0.1f) {
            result.pillarIgnoreState = PillarIgnoreState.All;
        }else if(pillarIgnoreStateRandom < 0.4f) {
            result.pillarIgnoreState = PillarIgnoreState.None;
        }else if(pillarIgnoreStateRandom < 0.7f) {
            result.pillarIgnoreState = PillarIgnoreState.Below;
        } else {
            result.pillarIgnoreState = PillarIgnoreState.Above;
        }

        result.pillarIgnoreState = (PillarIgnoreState) Random.Range(-1, 3);

        return result;
    }

    public static TerrainLayerSettings GetRandom() {
        string name = "layer" + Random.Range(1, 6);
        TerrainLayerSettings target = Resources.Load<TerrainLayerSettings>(name);
        TerrainLayerSettings result = ScriptableObject.CreateInstance<TerrainLayerSettings>();
        result.depth = target.depth;
        result.topTransition = target.topTransition;
        result.bottomTransition = target.bottomTransition;
        result.groundThickness = target.groundThickness;
        result.chasmRadius = target.chasmRadius;
        result.groundDepth = target.groundDepth;
        result.groundHeightChangeComplexity = target.groundHeightChangeComplexity;
        result.groundHeightChangeDistortionStrength = target.groundHeightChangeDistortionStrength;
        result.groundHeightChangeMax = target.groundHeightChangeMax;
        result.groundHeightChangeScale = target.groundHeightChangeScale;
        result.surfaceFeatureDepth = target.surfaceFeatureDepth;
        result.surfaceRoughness = target.surfaceRoughness;
        result.pillarDensity = target.pillarDensity;
        result.pillarIgnoreState = target.pillarIgnoreState;
        result.pillarScale = target.pillarScale;

        return result;
    }


    public TerrainLayerSettingsStruct AsStruct() {
        return new TerrainLayerSettingsStruct {
            origin = origin,
            depth = genDepth,

            topTransition = topTransition,
            bottomTransition = bottomTransition,
            chasmRadius = chasmRadius,

            groundThickness = groundThickness,
            groundDepth = groundDepth,

            groundHeightChangeMax = groundHeightChangeMax,
            groundHeightChangeScale = groundHeightChangeScale,
            groundHeightChangeComplexity = groundHeightChangeComplexity,
            groundHeightChangeDistortionStrength = groundHeightChangeDistortionStrength,

            surfaceRoughness = surfaceRoughness,
            surfaceFeatureDepth = surfaceFeatureDepth,

            pillarDensity = pillarDensity,
            pillarScale = pillarScale,
            pillarIgnoreState = (int) pillarIgnoreState,

            octaves = octaves,
            frequency = frequency,
            persistance = persistance,
            lacunarity = lacunarity
        };
    }

    public enum PillarIgnoreState {
        None = 0,
        Below = -1,
        Above = 1,
        All = 2
    }

}

public struct TerrainLayerSettingsStruct {
    public Vector3 origin;
    public float depth;

    public float topTransition;
    public float bottomTransition;
    public float chasmRadius;

    public float groundThickness;
    public float groundDepth;

    public float groundHeightChangeMax;
    public float groundHeightChangeScale;
    public int groundHeightChangeComplexity;
    public float groundHeightChangeDistortionStrength;

    public float surfaceRoughness;
    public float surfaceFeatureDepth;

    public float pillarDensity;
    public float pillarScale;
    public int pillarIgnoreState;

    public int octaves;
    public float frequency;
    public float persistance;
    public float lacunarity;
}
