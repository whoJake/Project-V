using System;
using UnityEngine;

public class ChunkPointEdit : ChunkEditType {
    public Vector3 position;
    public float radius;
    public ChunkPointEdit(Vector3 _position, float _radius) {
        position = _position;
        radius = _radius;
    }

    public override void PerformEdit(RenderTexture target, TerrainChunk chunk) {
        editShader = Resources.Load<ComputeShader>("Compute/ChunkEditing/ChunkPointEdit");
        editShader.SetTexture(0, "_DensityTexture", target);
        editShader.SetVector("chunk_origin", chunk.origin);
        editShader.SetFloat("voxel_scale", chunk.voxelScale);
        editShader.SetVector("point_position", position);
        editShader.SetFloat("point_radius", radius);

        Vector3Int threads = RTUtils.CalculateThreadAmount(chunk.textureDimensions, 8);
        editShader.Dispatch(0, threads.x, threads.y, threads.z);
    }

    public override Bounds GetBounds() {
        return new Bounds(position, Vector3.one * (radius + 1) * 2);

    }

    public override ChunkEditType Clone() {
        return new ChunkPointEdit(position, radius);
    }

}