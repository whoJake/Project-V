using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkLineEdit : IChunkEdit {
    public Vector3 start;
    public Vector3 end;
    public float startRadius;
    public float endRadius;
    public int resolution;
    public float timeToFire;

    public MonoBehaviour owner;

    public ChunkLineEdit(Vector3 _start, Vector3 _end, float _startRadius, float _endRadius, int _resolution, float _timeToFire, MonoBehaviour _owner) {
        start = _start;
        startRadius = _startRadius;
        end = _end;
        endRadius = _endRadius;

        resolution = _resolution;
        timeToFire = _timeToFire;
        owner = _owner;
    }

    public void PerformEdit(RenderTexture target, TerrainChunk chunk) {
        owner.StartCoroutine(FireLine(chunk));
    }

    private IEnumerator FireLine(TerrainChunk chunk) {
        float timeBetweenSteps = timeToFire / resolution;

        Vector3 step = (end - start) / resolution;
        for(int i = 0; i < resolution; i++) {
            Vector3 position = start + step * i;
            float radius = Mathf.Lerp(startRadius, endRadius, (float)i / resolution);
            chunk.layer.handler.DistributeEditRequest(new ChunkEditRequest(new ChunkPointEdit(position, radius, false)));

            yield return new WaitForSecondsRealtime(timeBetweenSteps);
        }

    }

    public Bounds GetBounds() {
        Vector3 centre = start;
        Vector3 size = Vector3.one;

        return new Bounds(centre, size);
    }
}
