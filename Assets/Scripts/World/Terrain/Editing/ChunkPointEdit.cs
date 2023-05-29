using System;
using UnityEngine;

public class ChunkPointEdit : IChunkEdit {
    public Vector3 position;
    public float radius;
    public bool add;
    public ChunkPointEdit(Vector3 _position, float _radius, bool _add) {
        position = _position;
        radius = _radius;
        add = _add;
    }

    public void PerformEdit(RenderTexture target, TerrainChunk chunk) {
        ComputeShader editShader = Resources.Load<ComputeShader>("Compute/ChunkEditing/ChunkPointEdit");
        editShader.SetTexture(0, "_DensityTexture", target);
        editShader.SetVector("chunk_origin", chunk.origin);
        editShader.SetFloat("voxel_scale", chunk.handler.voxelScale);
        editShader.SetVector("point_position", position);
        editShader.SetFloat("point_radius", radius);
        editShader.SetInt("multiplier", add ? -1 : 1);

        Vector3Int threads = RTUtils.CalculateThreadAmount(chunk.handler.textureDimensions, 8);
        editShader.Dispatch(0, threads.x, threads.y, threads.z);
    }

    public Bounds GetBounds() {
        return new Bounds(position, Vector3.one * (radius + 1) * 2);

    }
}