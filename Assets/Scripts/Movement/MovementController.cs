using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class MovementController : MonoBehaviour
{
    //TODO
    //Basic collider constricted movement
    //Max slope height
    //Step handling?
    //Speed setting
    //Acceleration setting
    //Jumping
    //Jump height

    //Struct to hold ray origins? Rays themselves?
    //Is a struct even nessisary
    //Do rays really need to be calculated in a grid array (could be a lot of rays)

    [Tooltip("Max speed the body can reach through controlled movement")]
    [Min(0)] public float maxSpeed;
    [Tooltip("How fast the body will reach max speed")]
    [Min(0)] public float acceleration;

    [Tooltip("Distance of collision rays start point from underlying Collider")]
    [Min(0)] public float skinWidth;
    [Tooltip("How many rays per axis will check for collisions")]
    [Min(3)] public Vector2Int rayCount;

    private RayOrigins origins;
    private new CapsuleCollider collider;

    private Vector3 velocity;
    
    /* Move the body along a given vector
     * Vector components will be clamped between 0-1
     * Returns true if the body changed position as a result of the given input
     */
    protected bool Move(Vector2 input) {
        return true;
    }

    private void Start() {
        collider = gameObject.GetComponent<CapsuleCollider>();
    }

    private void OnDrawGizmos() {
        Start();
        CalculateOrigins();
        foreach (Vector3 centre in origins.vertical) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + centre, 0.1f);
        }
        foreach(Vector3 centre in origins.horizontal) {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position + centre, 0.1f);
        }
    }

    private void CalculateOrigins() {
        origins.horizontal = new Vector3[rayCount.x * rayCount.y];
        origins.vertical = new Vector3[rayCount.x * rayCount.x];

        //Vertical rays
        //Not perfect distribution but should work fine
        float v_increment = 2f / (rayCount.x - 1);

        for(int x = 0; x < rayCount.x; x++) {
            for(int z = 0; z < rayCount.x; z++) {
                float currentX = (-1f) + x * v_increment;
                float currentZ = (-1f) + z * v_increment;

                float xYPart = (float)x / (rayCount.x - 1f);
                xYPart = xYPart < 0.5f ? xYPart : 1f - xYPart;
                xYPart *= 2f;

                float zYPart = (float)z / (rayCount.x - 1f);
                zYPart = zYPart < 0.5f ? zYPart : 1f - zYPart;
                zYPart *= 2f; 

                float currentY = Mathf.Max(0f, xYPart * zYPart);

                Vector3 position = new Vector3(currentX, currentY, currentZ).normalized * collider.radius;
                position -= position.normalized * skinWidth;
                position.y += collider.radius * 2 < collider.height ? collider.height / 2f - collider.radius : 0f;
                position.y *= Mathf.Sign(velocity.y);

                origins.vertical[x + z * rayCount.x] = position;
            }
        }

        //Horizontal Rays
        float hx_increment = 2f / (rayCount.x - 1);
        float hy_increment = 2f / (rayCount.y - 1);

        for(int x = 0; x < rayCount.x; x++) {
            for(int y = 0; y < rayCount.y; y++) {
                float currentX = (-1f) + x * hx_increment;
                float currentY = (-1f) + y * hy_increment;

                Vector3 position = new Vector3(currentX * collider.radius, currentY * Mathf.Max(collider.radius, collider.height / 2f), 0f);
                origins.horizontal[x + y * rayCount.x] = position;

                //Curve around the top
                //Curve around the velocity direction

            }
        }

    }

    struct RayOrigins {
        public Vector3[] horizontal;
        public Vector3[] vertical;
    }

}
