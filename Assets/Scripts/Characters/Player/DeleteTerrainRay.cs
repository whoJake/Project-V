using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteTerrainRay : MonoBehaviour
{
    public Camera mcamera;
    public TerrainHandler terrainHandler;
    public LayerMask mask;

    public float startRadius;
    public float endRadius;
    public float timeToTravel;
    public int resolution;

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = mcamera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit,  250, ~mask)) {
                Vector3 direction = (hit.point - transform.position);
                Vector3 beamDir = new Vector3(direction.x, 0, direction.z).normalized;
                ChunkEditRequest request = new ChunkEditRequest(new ChunkBeamEdit(hit.point, hit.point + beamDir * 150f, startRadius, endRadius, resolution, timeToTravel, this));
                terrainHandler.DistributeEditRequest(request);
            }
        }

        else if (Input.GetMouseButtonDown(0)) {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = mcamera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, 250, ~mask)) {
                ChunkEditRequest request = new ChunkEditRequest(new ChunkPointEdit(hit.point, 10f, true));
                terrainHandler.DistributeEditRequest(request);
            }
        }
    }
}
