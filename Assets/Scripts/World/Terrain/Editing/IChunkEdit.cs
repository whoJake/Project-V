using UnityEngine;

public interface IChunkEdit {
    public abstract void PerformEdit(RenderTexture target, TerrainChunk chunk);
    public abstract Bounds GetBounds();
}