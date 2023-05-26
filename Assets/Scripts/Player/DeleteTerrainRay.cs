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
        if (Input.GetMouseButtonDown(0)) {
            Debug.Log("Mouse clicked");
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = mcamera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit,  250, ~mask)) {
                ChunkEditRequest request = new ChunkEditRequest(new ChunkPointEdit(hit.point, 20f));
                terrainHandler.MakeEditRequest(request);
            }
        }
    }
}
