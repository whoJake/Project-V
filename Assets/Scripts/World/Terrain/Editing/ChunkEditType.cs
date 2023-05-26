using UnityEngine;

public abstract class ChunkEditType {
    public ComputeShader editShader;
    public abstract void PerformEdit(RenderTexture target, TerrainChunk chunk);
    public abstract ChunkEditType Clone();
    public abstract Bounds GetBounds();
}