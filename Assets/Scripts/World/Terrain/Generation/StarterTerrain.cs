using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/Starter")]
public class StarterTerrain : TerrainLayerGenerator
{
    private ComputeShader shader;

    [SerializeField]
    private int numOfPlatforms;
    [SerializeField]
    private Vector2 platformRadiusRange;
    [SerializeField]
    [Range(0f, 1f)]
    private float platformShapeFeatureRadius;
    [SerializeField]
    private Vector2 platformFlatnessRange;
    [SerializeField]
    private float platformTopDisplacement;

    [SerializeField]
    private Vector2 platformPathHorizontalDifferenceRange;
    [SerializeField]
    private Vector2 platformPathVerticalDifferenceRange;
    [SerializeField]
    private Vector2 platformPathDistanceFromWallRange;
    [SerializeField]
    private float platformPathSwitchDirectionChance;
    [SerializeField]
    private float bonusPlatformMinDistance;

    [SerializeField]
    private Vector2 platformStemPinchRange;
    [SerializeField]
    private Vector2 platformStemRadius;
    [SerializeField]
    private float platformStemFeatureDepth;

    [SerializeField]
    private Vector2 depthRange;
    [SerializeField]
    private Vector2 chasmRadiusRange;

    [SerializeField]
    private float upperSurfaceDepth;
    [SerializeField]
    private float upperSurfaceFeatureDepth;

    [SerializeField]
    private float upperRadius;
    [SerializeField]
    private float lowerRadius;
    [SerializeField]
    private int cliffSlopeEasePower;
    [SerializeField]
    private float cliffFeatureDepth;
    [SerializeField]
    private Vector2 cliffLedgeSize;

    [SerializeField]
    private NamedNoiseArgs[] noiseArgs;

    private float depth = -1;

    private bool layerInitialized = false;
    private ComputeBuffer platformBuffer;
    private ComputeBuffer noiseArgsBuffer;

    private void OnEnable() {
        shader = Resources.Load<ComputeShader>("Compute/Layers/StarterTerrain");
    }

    private void InitializeLayer(TerrainLayer layer, int seed) {
        Debug.Log("Initializing Starter Terrain Generator");

        Random.InitState(seed);

        CreatePlatformBuffer(layer);
        CreateNoiseArgsBuffer();
        shader.SetBuffer(0, "_PlatformBuffer", platformBuffer);
        shader.SetBuffer(0, "_NoiseArgs", noiseArgsBuffer);
        shader.SetVector("_PlatformRadiusRange", platformRadiusRange);
        shader.SetFloat("_PlatformShapeFeatureRadius", platformShapeFeatureRadius);
        shader.SetFloat("_PlatformTopDisplacement", platformTopDisplacement);
        shader.SetVector("_PlatformFlatnessRange", platformFlatnessRange);

        shader.SetVector("_PlatformStemPinchRange", platformStemPinchRange);
        shader.SetVector("_PlatformStemRadius", platformStemRadius);
        shader.SetFloat("_PlatformStemFeatureDepth", platformStemFeatureDepth);

        shader.SetFloat("_UpperSurfaceDepth", upperSurfaceDepth);
        shader.SetFloat("_UpperSurfaceFeatureDepth", upperSurfaceFeatureDepth);
        shader.SetVector("_CliffLedgeSize", cliffLedgeSize);

        shader.SetFloat("_UpperRadius", upperRadius);
        shader.SetFloat("_LowerRadius", lowerRadius);
        shader.SetInt("_CliffSlopeEasePower", cliffSlopeEasePower);
        shader.SetFloat("_CliffFeatureDepth", cliffFeatureDepth);

        shader.SetVector("_LayerOrigin", layer.origin);

        shader.SetVector("_LayerSize", layer.GetBounds().size);

        shader.SetFloat("_ChasmRadius", Random.Range(chasmRadiusRange.x, chasmRadiusRange.y));

        shader.SetFloat("_VoxelScale", layer.handler.voxelScale);
        layerInitialized = true;
    }

    public override void ReleaseBuffers() {
        platformBuffer.Release();
        noiseArgsBuffer.Release();
    }

    private float GetRadiusAtNormalizedHeight(float height) {
        return Mathf.Lerp(upperRadius, lowerRadius, 
                          Mathf.Pow(height, cliffSlopeEasePower));
    }

    public void CreatePlatformBuffer(TerrainLayer layer) {
        List<Vector3> platformPositions = new List<Vector3>();
        Vector3 layerSize = layer.GetBounds().size;

        float minDepth = (upperSurfaceDepth + upperSurfaceFeatureDepth) / layerSize.y;
        float maxDepth = 1f - ((Random.Range(platformPathVerticalDifferenceRange.x, platformPathVerticalDifferenceRange.y) + platformRadiusRange.y) / layerSize.y);

        float currentDepth = minDepth;
        float currentAngle = 0f;
        bool switchedLast = false;

        while (currentDepth <= maxDepth) { 
            float radius = GetRadiusAtNormalizedHeight(currentDepth);
            Vector2 radialDir = new Vector2(Mathf.Sin(currentAngle), Mathf.Cos(currentAngle));

            float inset_t = Random.Range(switchedLast ? 0.5f : 0f, 1f);
            radius -= Mathf.Lerp(platformPathDistanceFromWallRange.x, platformPathDistanceFromWallRange.y, inset_t);

            Vector3 pos = new Vector3(radialDir.x * radius, layer.origin.y - currentDepth * layerSize.y, radialDir.y * radius);
            platformPositions.Add(pos);

            bool switchDir = Random.Range(0f, 1f) < platformPathSwitchDirectionChance;
            currentDepth += Random.Range(platformPathVerticalDifferenceRange.x, platformPathVerticalDifferenceRange.y) / layerSize.y;
            float hDst = Random.Range(platformPathHorizontalDifferenceRange.x, platformPathHorizontalDifferenceRange.y);
            currentAngle += (hDst / radius) * (switchDir ? -1 : 1);
            switchedLast = switchDir;
        }

        //Bonus platforms
        int numOfBonusPlatforms = 0;
        int tries = 0;
        while(numOfBonusPlatforms < numOfPlatforms && tries < 200) {
            float depth_t = Mathf.Pow( Random.Range(0f, 1f), 1 / 2f );
            float depth = Mathf.Lerp(minDepth, maxDepth, depth_t);
            float radius = GetRadiusAtNormalizedHeight(depth);
            float angle = Random.Range(0f, Mathf.PI * 2f);

            Vector2 radialDir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
            radius *= Random.Range(0f, 1.1f);

            Vector3 possiblePosition = new Vector3(radialDir.x * radius, layer.origin.y - depth * layerSize.y, radialDir.y * radius);

            bool valid = true;
            foreach(Vector3 platform in platformPositions) {
                float horizontalDistance = Vector2.Distance(new Vector2(platform.x, platform.z), new Vector2(possiblePosition.x, possiblePosition.z));
                float verticalDistance = Mathf.Abs(platform.y - possiblePosition.y);
                float pureDistance = Vector3.Distance(platform, possiblePosition);
                float centerDistance = Vector2.Distance(new Vector2(possiblePosition.x, possiblePosition.z), new Vector2(layer.origin.x, layer.origin.z));

                if(pureDistance < bonusPlatformMinDistance 
                    || ( horizontalDistance < platformRadiusRange.y && verticalDistance < bonusPlatformMinDistance ) 
                    || centerDistance <= lowerRadius * 0.8) {
                    valid = false;
                    tries++;
                    break;
                }
            }

            if (!valid) continue;

            platformPositions.Add(possiblePosition);
            numOfBonusPlatforms++;
        }

        Debug.Log(numOfBonusPlatforms + " bonus platforms spawned");

        platformBuffer = new ComputeBuffer(platformPositions.Count, 12);
        platformBuffer.SetData(platformPositions.ToArray());
    }

    public void CreateNoiseArgsBuffer() {
        noiseArgsBuffer = new ComputeBuffer(noiseArgs.Length, NoiseArgs.stride);
        NoiseArgs[] sNoiseArgs = new NoiseArgs[noiseArgs.Length];
        //Convert to struct
        for(int i = 0; i < noiseArgs.Length; i++) {
            sNoiseArgs[i] = (NoiseArgs) noiseArgs[i];
        }
        noiseArgsBuffer.SetData(sNoiseArgs);
    }

    public override void Generate(ref RenderTexture target, TerrainChunk chunk, int seed) {
        if (!layerInitialized) InitializeLayer(chunk.layer, seed);

        shader.SetTexture(0, "_Target", target);
        shader.SetFloat("_Seed", seed);
        shader.SetVector("_ChunkOrigin", chunk.origin);
        shader.SetVector("_ChunkSize", chunk.GetBounds().size);

        Vector3Int threads = RTUtils.CalculateThreadAmount(chunk.handler.textureDimensions, 8);
        shader.Dispatch(0, threads.x, threads.y, threads.z);
    }

    public override float GetDepth(float chunkHeight) {
        if (depth == -1.0) {
            depth = Random.Range(depthRange.x, depthRange.y);
            depth = Mathf.Round(depth / chunkHeight) * chunkHeight;
        }

        return depth;
    }
}

