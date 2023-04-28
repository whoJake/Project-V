using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [Tooltip("Array of Orbits used to control the movement of the 3rd person camera")]
    public OrbitInfo[] orbits;
    [Min(4)] public int testDivisions;

    private OrbitalRail[] rings;

    private void OnDrawGizmos() {
        SetupRings();
        float step = 360f / testDivisions;
        Gizmos.color = Color.green;
        foreach (OrbitalRail r in rings) {
            for (int i = 0; i < testDivisions; i++) {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(r.Evaluate(i * step), 0.1f);
            }
        }
    }

    private void SetupRings() {
        rings = new OrbitalRail[orbits.Length];
        for(int i = 0; i < orbits.Length; i++) {
            rings[i] = new OrbitalRail(transform.position + transform.TransformDirection(orbits[i].offset), orbits[i].radius, transform.up);
        }
    }

    private void OnValidate() {
        SetupRings();
    }

    [System.Serializable]
    public class OrbitInfo {
        [Tooltip("Offset relative to transform of orbit")]
        public Vector3 offset;
        [Tooltip("Radius of orbit")]
        [Min(0)] public float radius;
    }
}
