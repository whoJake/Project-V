using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [Tooltip("Array of Orbits used to control the movement of the 3rd person camera")]
    public OrbitalInfo[] orbits;

    [Tooltip("The transform which the orbits will orbit around")]
    public Transform orbitAround;

    [Tooltip("GameObject that will be controlled as the camera")]
    public Camera affectedCamera;

    [Tooltip("Mouse sensitivity of this camera controller")]
    public Vector2 mouseSensitivity = new Vector2(1, 1);

    [Tooltip("The transform that the camera will focus on and always face towards")]
    public Transform focusPoint;

    [Tooltip("Will this camera control the forward direction of another transform")]
    public bool willControlTransformDirection;

    [Tooltip("Will control the forward direction of this transform")]
    public Transform controlTransform;

    [Tooltip("Determines which parts of the rotation will be controlled by the camera")]
    public ControlTransformType controlTransformType = ControlTransformType.NoVerticalControl;

    [Tooltip("Display the wireframes of setup orbits in the editor")]
    public bool showOrbitWireframes = false;

    private OrbitalRail[] rings;

    private float currentAngle;
    private float currentHeight;

    private void Update() {
        UpdateRings();
        ReadInput();

        affectedCamera.transform.position = CalculateCameraPosition();

        FocusOnTransform();

        if (willControlTransformDirection) ControlTransformDirection();
    }

    private void FocusOnTransform() {
        if(focusPoint == null) {
            Debug.Log("Focus Transform is not set");
            return;
        }
        affectedCamera.transform.LookAt(focusPoint.position);
    }

    private void ControlTransformDirection() {
        Vector3 vecFromCamera = (controlTransform.position - affectedCamera.transform.position).normalized;

        switch (controlTransformType) {
            case ControlTransformType.FullControl:
                controlTransform.right = vecFromCamera;
                break;
            case ControlTransformType.NoVerticalControl:
                vecFromCamera.y = 0;
                controlTransform.right = vecFromCamera;
                break;
            default:
                Debug.Log("ControlTransformType is not set");
                break;
        }
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

    private void UpdateRings() {
        rings = new OrbitalRail[orbits.Length];
        float height = 0;
        for(int i = 0; i < orbits.Length; i++) {
            rings[i] = new OrbitalRail(orbitAround.TransformPoint(orbits[i].offset), orbits[i].radius, orbitAround.transform.up);
            orbits[i].height = height;
            if (i == orbits.Length - 1) continue;
            height += orbits[i].offset.y - orbits[i + 1].offset.y;
        }
    }

    [System.Serializable]
    public class OrbitalInfo {
        [Tooltip("Offset relative to transform that is being orbitted around")]
        public Vector3 offset;
        [Tooltip("Radius of orbit")]
        [Min(0)] public float radius;

        [HideInInspector] 
        public float height; //Height starts at 0 at the top

        public OrbitalInfo(Vector3 offset, float radius) {
            this.offset = offset;
            this.radius = radius;
        }
    }

    public enum ControlTransformType {
        FullControl,
        NoVerticalControl
    }

    private void OnDrawGizmosSelected() {
        if (showOrbitWireframes) {
            UpdateRings();
            Gizmos.color = Color.blue;
            foreach (OrbitalRail r in rings) {
                r.DrawGizmos(50);
            }
        }
    }
}
