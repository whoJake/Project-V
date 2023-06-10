using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/Settings", order = 0)]
public class TerrainSettings : ScriptableObject
{
    [Range(0, 2048)] public int seed;
    public TerrainLayerGenerator[] layers;
}
