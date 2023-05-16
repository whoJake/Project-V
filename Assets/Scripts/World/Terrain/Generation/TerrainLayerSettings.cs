using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName =  "Terrain/Layer Settings", order = 1)]
public class TerrainLayerSettings : ScriptableObject
{
    public static int stride { get { return sizeof(float) * 6 + sizeof(int); } }

    [Tooltip("Depth of this layer")]
    [Min(0)] public float depth;

    [Tooltip("Transition margin at the top")]
    [Min(0)] public float topTransition;

    [Tooltip("Transition margin at bottom")]
    [Min(0)] public float bottomTransition;

    [Min(0)] public float chasmRadius;

    //Probably temp
    [Min(0)] public int octaves;
    [Min(0)] public float frequency;
    [Range(0f, 2f)] public float persistance;
    [Min(0)] public float lacunarity;

    public TerrainLayerSettingsStruct AsStruct() {
        return new TerrainLayerSettingsStruct {
            topTransition = topTransition,
            bottomTransition = bottomTransition,
            chasmRadius = chasmRadius,
            octaves = octaves,
            frequency = frequency,
            persistance = persistance,
            lacunarity = lacunarity
        };
    }

}

public struct TerrainLayerSettingsStruct {
    public float topTransition;
    public float bottomTransition;
    public float chasmRadius;
    public int octaves;
    public float frequency;
    public float persistance;
    public float lacunarity;
}
