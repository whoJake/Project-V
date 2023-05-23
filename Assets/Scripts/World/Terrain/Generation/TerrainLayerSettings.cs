using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName =  "Terrain/Layer Settings", order = 1)]
public class TerrainLayerSettings : ScriptableObject
{
    public static int stride { get { return sizeof(float) * 14 + sizeof(int) * 1; } }

    [HideInInspector] public Vector3 origin;

    [Tooltip("Depth of this layer")]
    [Min(0)] public float depth;
    [HideInInspector] public float genDepth;

    [Tooltip("Transition margin at the top")]
    [Min(0)] public float topTransition;

    [Tooltip("Transition margin at bottom")]
    [Min(0)] public float bottomTransition;

    [Min(0)] public float chasmRadius;

    public float groundThickness;
    public float groundDepth;

    [Range(0f, 1f)] public float surfaceRoughness;
    public float surfaceFeatureDepth;

    //Probably temp
    [Min(0)] public int octaves;
    [Min(0)] public float frequency;
    [Range(0f, 2f)] public float persistance;
    [Min(0)] public float lacunarity;

    public TerrainLayerSettingsStruct AsStruct() {
        return new TerrainLayerSettingsStruct {
            origin = origin,
            depth = genDepth,

            topTransition = topTransition,
            bottomTransition = bottomTransition,
            chasmRadius = chasmRadius,

            groundThickness = groundThickness,
            groundDepth = groundDepth,

            surfaceRoughness = surfaceRoughness,
            surfaceFeatureDepth = surfaceFeatureDepth,

            octaves = octaves,
            frequency = frequency,
            persistance = persistance,
            lacunarity = lacunarity
        };
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

    public float surfaceRoughness;
    public float surfaceFeatureDepth;

    public int octaves;
    public float frequency;
    public float persistance;
    public float lacunarity;
}
