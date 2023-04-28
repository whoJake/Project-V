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

    //To remove
    private OrbitalRail[] rings;

    private float currentAngle;
    private float currentHeight;

    private void Update() {
        ReadInput();
        cameraHolder.transform.position = CalculateCameraPosition();
        cameraHolder.transform.LookAt(transform);
    }

    private Vector3 CalculateCameraPosition() {
        //These cases had issues so patchwork fix here
        if(currentHeight == 0) {
            OrbitalRail rail = new OrbitalRail(transform.position + transform.TransformDirection(orbits[^1].offset), orbits[^1].radius, transform.up);
            return rail.Evaluate(currentAngle);
        }
        if (currentHeight == 1) {
            OrbitalRail rail = new OrbitalRail(transform.position + transform.TransformDirection(orbits[0].offset), orbits[0].radius, transform.up);
            return rail.Evaluate(currentAngle);
        }

        float currentHeightScaled = orbits[^1].offset.y + currentHeight * (orbits[0].offset.y - orbits[^1].offset.y);
        OrbitInfo lower = new OrbitInfo(new Vector3(0, float.NegativeInfinity, 0), 0);
        OrbitInfo upper = new OrbitInfo(new Vector3(0, float.PositiveInfinity, 0), 0);

        //Calculate the orbit above and below the currentHeight
        foreach (OrbitInfo o in orbits) {
            if(o.offset.y < currentHeightScaled && o.offset.y > lower.offset.y) {
                lower = o;
            }
            if (o.offset.y > currentHeightScaled && o.offset.y < upper.offset.y) {
                upper = o;
            }
        }

        float gap = upper.offset.y - lower.offset.y;
        float lerp = (currentHeightScaled - lower.offset.y) / gap;

        OrbitalRail lowerRail = new OrbitalRail(transform.position + transform.TransformDirection(lower.offset), lower.radius, transform.up);
        OrbitalRail upperRail = new OrbitalRail(transform.position + transform.TransformDirection(upper.offset), upper.radius, transform.up);

        return Vector3.Lerp(lowerRail.Evaluate(currentAngle), upperRail.Evaluate(currentAngle), lerp);
    }

    private void ReadInput() {
        Cursor.lockState = CursorLockMode.Locked;
        Vector2 lookInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        lookInput *= mouseSensitivity;

        currentAngle = Mathf.Repeat(currentAngle + (lookInput.x * Time.deltaTime), 1);
        currentHeight = Mathf.Clamp01(currentHeight + (lookInput.y * Time.deltaTime));
    }

    private void SetupRings() {
        rings = new OrbitalRail[orbits.Length];
        for(int i = 0; i < orbits.Length; i++) {
            rings[i] = new OrbitalRail(transform.position + transform.TransformDirection(orbits[i].offset), orbits[i].radius, transform.up);
        }
    }

    [System.Serializable]
    public class OrbitInfo {
        [Tooltip("Offset relative to transform of orbit")]
        public Vector3 offset;
        [Tooltip("Radius of orbit")]
        [Min(0)] public float radius;

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
