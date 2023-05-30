using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class ComputeUtils
{
    public static IEnumerator WaitForResource(ComputeBuffer buffer) {
        AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(buffer);
        while (!request.done) {
            if (request.hasError) {
                Debug.Log("Request had error");
                yield break;
            }
            yield return null;
        }
    }

    public static IEnumerator WaitForResource(RenderTexture texture) {
        AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(texture, 0);
        while (!request.done) {
            if (request.hasError) {
                Debug.Log("Request had error");
                yield break;
            }
            yield return null;
        }
    }
}
