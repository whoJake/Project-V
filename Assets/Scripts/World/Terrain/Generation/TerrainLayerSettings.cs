using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName =  "Terrain/Layer Settings", order = 1)]
public class TerrainLayerSettings : ScriptableObject
{
    public static int stride { get { return sizeof(float) * 19 + sizeof(int) * 3; } }

    [HideInInspector] 
    public Vector3 origin;
    [HideInInspector]
    public float genDepth;

    [Min(0)]        [Tooltip("Target Depth of this layer")]
    public float depth;

    [Min(0)]        [Tooltip("Size of the transition into the above layer")]
    public float topTransition;

    [Min(0)]        [Tooltip("Size of the transition into the below layer")]
    public float bottomTransition;
    
    [Min(0)] 
    public float chasmRadius;

    [Min(0)] 
    public float groundThickness;

                    [Tooltip("Depth of the centre of the created ground plane")]
    public float groundDepth;

    [Min(0)]        [Tooltip("Maximum change in height from groundDepth that can occur")]
    public float groundHeightChangeMax;

    [Range(0f, 1f)] [Tooltip("Size of height change noise")]
    public float groundHeightChangeScale;

    [Min(0)]        [Tooltip("Complexity of the noise used to change the height (0 = No height change)")]
    public int groundHeightChangeComplexity;

    [Min(0)]        [Tooltip("Strength of the distortion effect applied to height change noise")]
    public float groundHeightChangeDistortionStrength;

    [Range(0f, 1f)] 
    public float surfaceRoughness;

    [Min(0)]        [Tooltip("Maximum depth of the surface roughness features")]
    public float surfaceFeatureDepth;

    [Range(0f, 1f)] [Tooltip("Density of pillar spawning")]
    public float pillarDensity;

    [Range(0f, 1f)] [Tooltip("Size of pillars spawned (Also slightly affected by density)")]
    public float pillarScale;

                    [Tooltip("What sides of the ground will pillars not be spawned on")]
    public PillarIgnoreState pillarIgnoreState;


    //Experimental Settings
    [Min(0)] 
    public int octaves;
    [Min(0)] 
    public float frequency;
    [Range(0f, 2f)] 
    public float persistance;
    [Min(0)] 
    public float lacunarity;

    //
    // Summery:
    //   Create and return a TerrainLayerSettings with all random settings within reasonable bounds
    //
    public static TerrainLayerSettings GetAllRandom() {
        TerrainLayerSettings result = ScriptableObject.CreateInstance<TerrainLayerSettings>();
        result.depth = Random.Range(128f, 256f);
        result.topTransition = Random.Range(8f, 32f);
        result.bottomTransition = 0f;
        result.chasmRadius = Random.Range(200f, 300f);

        result.groundDepth = Random.Range(32f, result.depth - 32f);

        float maxGroundThickness = Mathf.Min(result.groundDepth, result.depth - result.groundDepth) * 2f;
        result.groundThickness = Random.Range(32f, maxGroundThickness);

        result.groundHeightChangeComplexity = Random.Range(3, 6);
        result.groundHeightChangeDistortionStrength = Random.Range(0f, 5f);
        result.groundHeightChangeScale = Random.Range(0.6f, 1f);

        float maxGroundHeightChange = Mathf.Max(0f, Mathf.Min(result.groundDepth - (result.groundThickness / 2f), result.depth - result.groundDepth - (result.groundThickness / 2f)));
        maxGroundHeightChange = Mathf.Min(maxGroundHeightChange, result.groundThickness / 1.5f);
        result.groundHeightChangeMax = Random.Range(0.7f, 1f) * maxGroundHeightChange;

        result.surfaceFeatureDepth = Random.Range(0f, result.groundThickness);
        result.surfaceRoughness = Random.Range(0.3f, 0.7f);

        result.pillarDensity = Random.Range(0f, 0.8f);
        result.pillarScale = Random.Range(0.4f, 1f);

        float pisRand = Random.Range(0f, 1f);
        if(pisRand < 0.02f) {
            result.pillarIgnoreState = PillarIgnoreState.All;
        }else if(pisRand < 0.2f) {
            result.pillarIgnoreState = PillarIgnoreState.None;
        }else if(pisRand < 0.7f) {
            result.pillarIgnoreState = PillarIgnoreState.Below;
        } else {
            result.pillarIgnoreState = PillarIgnoreState.Above;
        }

        result.pillarIgnoreState = (PillarIgnoreState) Random.Range(-1, 3);

        return result;
    }

    public static TerrainLayerSettings LoadNewInstance(string resourceName) {
        return Resources.Load<TerrainLayerSettings>(resourceName);
    }

    //
    // Summery:
    //   Creates and returns this object as a struct.
    //   This is used for passing into the compute buffer the settings
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

//
// Summery:
//   Struct representation of the above class
//   Contains only values that the compute shader uses
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
