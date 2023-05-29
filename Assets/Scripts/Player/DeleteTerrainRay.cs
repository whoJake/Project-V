using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteTerrainRay : MonoBehaviour
{
    public Camera mcamera;
    public TerrainHandler terrainHandler;
    public LayerMask mask;

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = mcamera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit,  250, ~mask)) {
                ChunkEditRequest request = new ChunkEditRequest(new ChunkLineEdit(hit.point, ray.origin + ray.direction * 200f, 10f, 40f, 20, 0.75f, this));
                terrainHandler.DistributeEditRequest(request);
            }
        }

        else if (Input.GetMouseButtonDown(0)) {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = mcamera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, 250, ~mask)) {
                ChunkEditRequest request = new ChunkEditRequest(new ChunkPointEdit(hit.point, 15f, true));
                terrainHandler.DistributeEditRequest(request);
            }
        }
    }
}
