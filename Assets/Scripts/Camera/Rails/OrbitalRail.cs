using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalRail
{
    public Vector3 centre;
    public float radius;
    public Vector3 upAxis;
    public Vector3 rightAxis;
    public Vector3 forwardAxis;

    public OrbitalRail(Vector3 _centre, float _radius, Vector3 _upAxis) {
        centre = _centre;
        radius = _radius;
        upAxis = _upAxis;
    }

    /*
     * Returns the position of an object at a given percentage along the rail starting from rightAxis
     */
    public Vector3 Evaluate(float position) {

        float angle = position * 2 * Mathf.PI;

        Vector3 offset = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));

        //Simple circle on xz plane, no transformation needed
        if (upAxis == Vector3.up) {
            return centre + offset * radius;
        }

        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, upAxis);
        offset = rotation * offset;

        return centre + offset * radius;
    }

    public void DrawGizmos(int resolution) {
        float step = 1f / resolution;
        for (int i = 1; i < resolution + 1; i++) {
            Gizmos.DrawLine(Evaluate(i * step), Evaluate((i - 1) * step));
        }
    }

}
