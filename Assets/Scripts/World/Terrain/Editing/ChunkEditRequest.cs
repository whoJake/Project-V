using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChunkEditRequest
{
    private ChunkEditType type;
    private bool inProgress;
    public bool InProgress { get { return inProgress; } }

    public ChunkEditRequest(ChunkEditType _type) {
        type = _type;
        inProgress = false;
    }

    public void Process(RenderTexture target, TerrainChunk chunk) {
        if (inProgress) throw new Exception("Request is already in progress");

        Debug.Log("Processing request...");
        type.PerformEdit(target, chunk);
        inProgress = true;
    }

    public Bounds GetBounds() {
        return type.GetBounds();
    }

    public ChunkEditRequest Clone() {
        return new ChunkEditRequest(type.Clone());
    }
}

