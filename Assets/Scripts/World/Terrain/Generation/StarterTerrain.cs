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
    private float platformShapeFeatureStrength;
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
    private Vector2Int platformPathConnectingCountRange;
    [SerializeField]
    private Vector2 platformPathConnectorsRadiusRange;
    [SerializeField]
    private Vector2 platformPathConnectorsFlatnessRange;

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
    private Platform starterPlatform;

    [SerializeField]
    private NamedNoiseArgs[] noiseArgs;

    private bool layerInitialized = false;
    private ComputeBuffer platformBuffer;
    private ComputeBuffer noiseArgsBuffer;

    private void OnEnable() {
        //shader = Resources.Load<ComputeShader>("Compute/Layers/StarterTerrain");
    }

    private void InitializeLayer(TerrainLayer layer, int seed) {
        Debug.Log("Initializing Starter Terrain Generator");

        shader = Resources.Load<ComputeShader>("Compute/Layers/StarterTerrain");
        Random.InitState(seed);

        CreatePlatformBuffer(layer);
        CreateNoiseArgsBuffer();
        shader.SetBuffer(0, "_PlatformBuffer", platformBuffer);
        shader.SetBuffer(0, "_NoiseArgs", noiseArgsBuffer);

        shader.SetFloat("_UpperSurfaceDepth", upperSurfaceDepth);
        shader.SetFloat("_UpperSurfaceFeatureDepth", upperSurfaceFeatureDepth);
        shader.SetVector("_CliffLedgeSize", cliffLedgeSize);

        shader.SetFloat("_UpperRadius", upperRadius);
        shader.SetFloat("_LowerRadius", lowerRadius);

        shader.SetInt("_CliffSlopeEasePower", cliffSlopeEasePower);
        shader.SetFloat("_CliffFeatureDepth", cliffFeatureDepth);

        shader.SetVector("_LayerOrigin", layer.oldOrigin);
        shader.SetVector("_LayerSize", layer.bounds.size);

        shader.SetFloat("_ChasmRadius", Random.Range(chasmRadiusRange.x, chasmRadiusRange.y));

        shader.SetFloat("_VoxelScale", layer.handler.voxelScale);
        layerInitialized = true;
    }

    public override void Reset() {
        layerInitialized = false;
        platformBuffer?.Release();
        noiseArgsBuffer?.Release();
        shader = null;
    }

    public override void ReleaseBuffers() {
        platformBuffer?.Release();
        noiseArgsBuffer?.Release();
    }

    private float GetRadiusAtNormalizedHeight(float height) {
        return Mathf.Lerp(upperRadius, lowerRadius, 
                          Mathf.Pow(height, cliffSlopeEasePower));
    }

    public Platform CreatePlatform(Vector3 position, float radius, float shapeFeatureScale, bool hasStem) {
        Platform result = new Platform {
            position = position,
            radius = radius,
            flatness = Random.Range(platformFlatnessRange.x, platformFlatnessRange.y),
            shapeFeatureStrength = platformShapeFeatureStrength * shapeFeatureScale,
            surfaceFeatureDepth = platformTopDisplacement,
            hasStem = hasStem ? 1 : 0,

            stemRadius = platformStemRadius,
            stemPinchRange = platformStemPinchRange,
            stemFeatureStrength = platformStemFeatureDepth,

            radiusNoiseArgID = 0u,
            abovePlatformNoiseArgID = 3u,
            platform3DShapeNoiseArgID = 1u,
            stemRadiusNoiseArgID = 6u
        };
        
        return result;
    }

    private bool IsBelowExistingPlatform(Platform checkPlatform, List<Platform> existingPlatforms) {
        foreach(Platform platform in existingPlatforms) {
            Vector2 checkHorizontal = new Vector2(checkPlatform.position.x, checkPlatform.position.z);
            Vector2 existingHorizontal = new Vector2(platform.position.x, platform.position.z);

            bool inHorizontalRange = Vector2.Distance(checkHorizontal, existingHorizontal) < (checkPlatform.radius * 0.8f);
            bool lessThanHeight = checkPlatform.position.y < platform.position.y;

            if (inHorizontalRange && lessThanHeight && platform.hasStem == 1) 
                return true;
        }
        return false;
    }

    private void AddPathPlatforms(ref List<Platform> platforms, Vector3 lSize, Vector3 lOrigin) {
        float minDepth = (upperSurfaceDepth + cliffLedgeSize.y) / lSize.y;
        float maxDepth = 0.9f;// - (platformRadiusRange.y / lSize.y);

        float curDepth = minDepth;
        float curAngle = 0f;
        float direction = Random.Range(0, 2) * 2 - 1;
        int connectingPlatformCount = 0;
        float prevPlacementRadius = 0;
        bool firstPlatform = true;

        while (curDepth <= maxDepth) {
            bool isConnector = connectingPlatformCount != 0;

            float pRadius = isConnector ? Random.Range(platformPathConnectorsRadiusRange.x, platformPathConnectorsRadiusRange.y) :
                                          Random.Range(platformRadiusRange.x, platformRadiusRange.y);
            float radiusAtDepth = GetRadiusAtNormalizedHeight(curDepth);
            float t = Mathf.InverseLerp(maxDepth - 0.2f, maxDepth, curDepth);
            float flatnessMultiplier = Mathf.Lerp(0f, 1f, t);
            curAngle += (pRadius / radiusAtDepth) * direction;

            Vector2 vecToCurAngle = new Vector2(Mathf.Sin(curAngle), Mathf.Cos(curAngle));

            bool switchDirection = Random.Range(0f, 1f) < platformPathSwitchDirectionChance && !isConnector;
            direction = switchDirection ? -direction : direction;

            float placementRadius = radiusAtDepth - pRadius;

            float dstFromWall;
            float dstFromWall_t = Random.Range(0f, 1f);

            Platform result = new Platform(); //Logic shows it should always be initialized but wont compile without first initializing
            if(firstPlatform) {
                result = starterPlatform;
                dstFromWall = -20f;
            }else if (isConnector) {
                dstFromWall = 0;
                placementRadius = prevPlacementRadius + Random.Range(-pRadius / 2f, +pRadius / 2f);
            } else {
                //Makes sure that when switching direction, the platform is always more outdented than not, helps with stopping overlapping a little bit
                if (switchDirection) 
                    dstFromWall_t = Mathf.Max(dstFromWall_t, 1 - dstFromWall_t);
                dstFromWall = Mathf.Lerp(platformPathDistanceFromWallRange.x, platformPathDistanceFromWallRange.y, dstFromWall_t);
            }

            placementRadius -= dstFromWall;

            Vector3 pPosition = new Vector3(vecToCurAngle.x * placementRadius, lOrigin.y - curDepth * lSize.y, vecToCurAngle.y * placementRadius);
            if (!firstPlatform)
                result = CreatePlatform(pPosition, pRadius, 1, !isConnector);
            else
                result.position = pPosition;

            if (!isConnector) {
                bool underPlatform = IsBelowExistingPlatform(result, platforms);
                if (underPlatform)
                    result.hasStem = 0;
            }
            result.flatness += flatnessMultiplier;
            platforms.Add(result);

            if (isConnector)
                connectingPlatformCount--;
            else
                connectingPlatformCount = Random.Range(platformPathConnectingCountRange.x, platformPathConnectingCountRange.y + 1);

            curDepth += Random.Range(platformPathVerticalDifferenceRange.x, platformPathVerticalDifferenceRange.y) / lSize.y;
            curAngle += (result.radius + Random.Range(platformPathHorizontalDifferenceRange.x, platformPathHorizontalDifferenceRange.y)) / radiusAtDepth * direction;
            prevPlacementRadius = placementRadius;
            firstPlatform = false;
        }
    }

    private void DisplayPlatformPositions(List<Platform> platforms) {
        float colorStep = 1f / platforms.Count;
        for(int i = 0; i < platforms.Count; i++) {
            Vector3 pos = platforms[i].position;
            float radius = platforms[i].radius;

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(radius * 2, 1, radius * 2);
            float greyScale = colorStep * i;
            go.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(greyScale, greyScale, greyScale);
        }
    }

    private void AddBonusPlatforms(ref List<Platform> platforms, Vector3 lSize, Vector3 lOrigin) {

    }

    private void CreatePlatformBuffer(TerrainLayer layer) {
        List<Platform> platforms = new List<Platform>();
        Vector3 layerSize = layer.bounds.size;
        Vector3 layerOrigin = layer.oldOrigin;

        AddPathPlatforms(ref platforms, layerSize, layerOrigin);
        //AddBonusPlatforms(ref platforms, layerSize, layerOrigin);

        float minDepth = (upperSurfaceDepth + upperSurfaceFeatureDepth) / layerSize.y;
        float maxDepth = 1f - ((Random.Range(platformPathVerticalDifferenceRange.x, platformPathVerticalDifferenceRange.y) + platformRadiusRange.y) / layerSize.y);

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
            foreach(Platform platform in platforms) {
                Vector3 platformPosition = platform.position;

                float horizontalDistance = Vector2.Distance(new Vector2(platformPosition.x, platformPosition.z), new Vector2(possiblePosition.x, possiblePosition.z));
                float verticalDistance = Mathf.Abs(platformPosition.y - possiblePosition.y);
                float pureDistance = Vector3.Distance(platformPosition, possiblePosition);
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

            Platform result = CreatePlatform(possiblePosition, 1, 1, true);
            platforms.Add(result);
            numOfBonusPlatforms++;
        }

        Debug.Log(platforms.Count + " platforms spawned\n" + numOfBonusPlatforms + " of them are bonus platforms");
        platformBuffer = new ComputeBuffer(Mathf.Max(platforms.Count, 1), Platform.stride);
        platformBuffer.SetData(platforms.ToArray());
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
        shader.SetVector("_ChunkSize", chunk.bounds.size);

        Vector3Int threads = RTUtils.CalculateThreadAmount(chunk.handler.textureDimensions, 8);
        shader.Dispatch(0, threads.x, threads.y, threads.z);
    }

    public override void SetDepth(float voxelsPerY, float voxelScale) {
        float desiredDepth = Random.Range(depthRange.x, depthRange.y);
        float chunkCount = Mathf.FloorToInt(desiredDepth / voxelsPerY / voxelScale);
        depth = chunkCount * voxelsPerY * voxelScale;
    }
}

[System.Serializable]
public struct Platform {
    public static int stride = sizeof(float) * 13 + sizeof(uint) * 4;
    public Vector3 position;
    public float radius;
    public float flatness;

    public float surfaceFeatureDepth;
    public float shapeFeatureStrength;
    public float hasStem;
    public Vector2 stemRadius;
    public Vector2 stemPinchRange;
    public float stemFeatureStrength;

    public uint radiusNoiseArgID;
    public uint abovePlatformNoiseArgID;
    public uint platform3DShapeNoiseArgID;
    public uint stemRadiusNoiseArgID;
}

