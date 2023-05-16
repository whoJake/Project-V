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
    public static RenderTexture Create3D_RFloat(Vector3Int dimensions) {
        RenderTexture result = new RenderTexture(dimensions.x, dimensions.y, 0);
        result.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        result.format = RenderTextureFormat.RFloat;
        result.volumeDepth = dimensions.z;
        result.enableRandomWrite = true;
        result.Create();
        return result;
    }
}
