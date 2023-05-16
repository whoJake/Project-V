using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/Settings", order = 0)]
public class TerrainSettings : ScriptableObject
{
    public int seed;
    public TerrainLayerSettings[] layers;

    public TerrainLayerSettingsStruct[] layersStruct { get {
            TerrainLayerSettingsStruct[] result = new TerrainLayerSettingsStruct[layers.Length];
            for(int i = 0; i < layers.Length; i++) {
                result[i] = layers[i].AsStruct();
            }
            return result;
        } }
}
