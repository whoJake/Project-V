using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RTUtils
{
    //
    // Summery:
    //      Creates a render 3 dimensional render texture of type RFloat
    //
    // Parameters:
    //   dimensions:
    //     size of each dimension in the created texture
    public static RenderTexture Create3D_R8(Vector3Int dimensions) {
        RenderTexture result = new RenderTexture(dimensions.x, dimensions.y, 0);
        result.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        result.format = RenderTextureFormat.R8;
        result.volumeDepth = dimensions.z;
        result.enableRandomWrite = true;
        result.Create();
        return result;
    }

    //
    // Summery:
    //      Creates a render 2 dimensional render texture of type RFloat
    //
    // Parameters:
    //   dimensions:
    //     size of each dimension in the created texture
    public static RenderTexture Create2D_R8(Vector2Int dimensions) {
        RenderTexture result = new RenderTexture(dimensions.x, dimensions.y, 0);
        result.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        result.format = RenderTextureFormat.R8;
        result.enableRandomWrite = true;
        result.Create();
        return result;
    }

    //
    // Summery:
    //   Calculates the number of threads to dispatch for a given texture size
    //
    // Parameters:
    //   size:
    //     dimensions of texture or buffer
    //   threadAmount:
    //     number of threads per block defined in the compute shader
    public static Vector3Int CalculateThreadAmount(Vector3 size, int threadAmount) {
        return new Vector3Int {
            x = Mathf.CeilToInt(size.x / threadAmount),
            y = Mathf.CeilToInt(size.y / threadAmount),
            z = Mathf.CeilToInt(size.z / threadAmount)
        };
    }
}
