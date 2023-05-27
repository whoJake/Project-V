using UnityEngine;

public abstract class ChunkEditType {
    public ComputeShader editShader;
    public abstract void PerformEdit(RenderTexture target, TerrainChunk chunk);
    public abstract Bounds GetBounds();
}