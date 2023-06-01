using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkBarrierEdit : IChunkEdit {
    public Vector3 position;
    public float radius;
    public float percFilled;
    public Vector3 openingDirection;

    public ChunkBarrierEdit(Vector3 _position, Vector3 _openingDirection, float _radius, float _percFilled) {
        position = _position;
        radius = _radius;
        percFilled = _percFilled;
        openingDirection = _openingDirection;
    }

    public void PerformEdit(RenderTexture target, TerrainChunk chunk) {
        ComputeShader editShader = Resources.Load<ComputeShader>("Compute/ChunkEditing/ChunkBarrierEdit");
        editShader.SetTexture(0, "_DensityTexture", target);
        editShader.SetVector("chunk_origin", chunk.origin);
        editShader.SetFloat("voxel_scale", chunk.handler.voxelScale);
        editShader.SetVector("point_position", position);
        editShader.SetFloat("point_radius", radius);
        editShader.SetFloat("percent_filled", percFilled);
        editShader.SetVector("opening_direction", openingDirection);

        Vector3Int threads = RTUtils.CalculateThreadAmount(chunk.handler.textureDimensions, 8);
        editShader.Dispatch(0, threads.x, threads.y, threads.z);
    }

    public Bounds GetBounds() {
        return new Bounds(position, Vector3.one * ((radius * 1.5f) + 1) * 2);

    }
}
