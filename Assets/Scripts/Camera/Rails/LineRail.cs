using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRail
{
    public Vector3 startPoint;
    public Vector3 endPoint;
    
    public LineRail(Vector3 _startPoint, Vector3 _endPoint) {
        startPoint = _startPoint;
        endPoint = _endPoint;
    }

    /* Returns the position of an object at a given percentage between startPoint and endPoint
     * Amount is given as a value between 0-1, 0 being startPoint and 1 being endPoint
     */
    public Vector3 Evaluate(float amount) {
        return Vector3.Lerp(startPoint, endPoint, amount);
    }

    public void DrawGizmos() {
        Gizmos.DrawLine(startPoint, endPoint);
    }

}
