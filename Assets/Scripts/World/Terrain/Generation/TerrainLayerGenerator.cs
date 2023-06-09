using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface TerrainLayerGenerator
{
    public void Generate(ref RenderTexture target, TerrainChunk chunk);
}
