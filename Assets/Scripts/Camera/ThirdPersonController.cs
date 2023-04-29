using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [Tooltip("Array of Orbits used to control the movement of the 3rd person camera")]
    public OrbitInfo[] orbits;
    public GameObject cameraHolder;
    [Min(4)] public int testDivisions;

    public Vector2 mouseSensitivity;

    private OrbitalRail[] rings;

    private float currentAngle;
    private float currentHeight;

    private void Update() {
        ReadInput();
        cameraHolder.transform.position = CalculateCameraPosition();
        cameraHolder.transform.LookAt(transform);
    }

    private Vector3 CalculateCameraPosition() {
        //Brings height between 0->Difference between first and last orbit
        float scaledHeight = currentHeight * orbits[^1].height;
        for(int i = 0; i < orbits.Length; i++) {

            //Most likely camera is on bottom or top rail
            if (orbits[i].height == scaledHeight) {
                return rings[i].Evaluate(currentAngle);
            }else if (orbits[i].height > scaledHeight) {
                //Find lerp
                float diff = orbits[i].height - orbits[i-1].height;
                float start = orbits[i-1].height;
                float lerp = (scaledHeight - start) / diff;

                return Vector3.Lerp(rings[i - 1].Evaluate(currentAngle), rings[i].Evaluate(currentAngle), lerp);
            }
        }
        Debug.Log("Error in Camera Position Evalutation");
        return Vector3.one; //Shouldn't reach here
    }

    private void ReadInput() {
        Cursor.lockState = CursorLockMode.Locked;
        Vector2 lookInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        lookInput *= mouseSensitivity * 0.01f;

        currentAngle = Mathf.Repeat(currentAngle + lookInput.x, 1);
        currentHeight = Mathf.Clamp01(currentHeight + lookInput.y);
    }

    private void SetupRings() {
        rings = new OrbitalRail[orbits.Length];
        float height = 0;
        for(int i = 0; i < orbits.Length; i++) {
            rings[i] = new OrbitalRail(transform.position + transform.TransformDirection(orbits[i].offset), orbits[i].radius, transform.up);
            orbits[i].height = height;
            if (i == orbits.Length - 1) continue;
            height += orbits[i].offset.y - orbits[i + 1].offset.y;
        }
    }

    [System.Serializable]
    public class OrbitInfo {
        [Tooltip("Offset relative to transform of orbit")]
        public Vector3 offset;
        [Tooltip("Radius of orbit")]
        [Min(0)] public float radius;

        [HideInInspector] 
        public float height; //Height starts at 0 at the top

        public OrbitInfo(Vector3 offset, float radius) {
            this.offset = offset;
            this.radius = radius;
        }
    }

    private void OnDrawGizmosSelected() {
        SetupRings();
        Gizmos.color = Color.blue;
        foreach (OrbitalRail r in rings) {
            r.DrawGizmos(testDivisions);
        }
    }

    private void OnValidate() {
        SetupRings();
    }
}
