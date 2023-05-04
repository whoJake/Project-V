using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [SerializeField] [Tooltip("Array of Orbits used to control the movement of the 3rd person camera")]
    private OrbitalInfo[] orbits;

    private OrbitalRail[] rings;

    [SerializeField] [Tooltip("The transform which the orbits will orbit around")]
    private Transform orbitAround;

    [SerializeField] [Tooltip("GameObject that will be controlled as the camera")]
    private Camera affectedCamera;

    [SerializeField]
    private Vector2 mouseSensitivity = new Vector2(1, 1);

    [SerializeField]
    private Transform focusPoint;

    [SerializeField] [Tooltip("Will this camera control the forward direction of another transform")]
    private bool willControlTransformDirection;

    [SerializeField] [Tooltip("Will control the forward direction of this transform")]
    private Transform controlTransform;

    [SerializeField] [Tooltip("Determines which parts of the rotation will be controlled by the camera")]
    private ControlTransformType controlTransformType = ControlTransformType.NoVerticalControl;

    [SerializeField] [Tooltip("This camera will avoid occluding the focus point")]
    private bool avoidOcclusion;

    [SerializeField]
    private LayerMask avoidOcclusionLayers;

    [SerializeField] [Tooltip("The time it takes for the camera move towards its un-occluded state")]
    [Min(0)] private float avoidOcclusionSmoothingTime;

    [SerializeField] [Tooltip("Buffer length between detected occlusion location and the new location of the camera")]
    [Min(0)] private float avoidOcclusionBufferLength;

    private Vector3 avoidOcclusionVelocity;

    [SerializeField] [Tooltip("Display the wireframes of setup orbits in the editor")]
    private bool showOrbitWireframes = false;


    private float currentAngle;
    private float currentHeight;

    private void Update() {
        UpdateRings();
        ReadInput();

        Vector3 updatedCameraPosition = CalculateCameraPosition();
        if (avoidOcclusion) AvoidOcclusion(ref updatedCameraPosition);
        affectedCamera.transform.position = updatedCameraPosition;

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
                controlTransform.forward = vecFromCamera;
                break;
            case ControlTransformType.NoVerticalControl:
                vecFromCamera.y = 0;
                controlTransform.forward = vecFromCamera;
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

    private void AvoidOcclusion(ref Vector3 position) {
        Vector3 rayOrigin = focusPoint.position;
        Vector3 rayDirection = (position - focusPoint.position).normalized;
        float rayLength = Vector3.Distance(position, focusPoint.position);

        Debug.DrawRay(rayOrigin, rayDirection * rayLength);

        RaycastHit hit;
        if(Physics.Raycast(rayOrigin, rayDirection, out hit, rayLength, ~avoidOcclusionLayers)) {
            Vector3 targetPosition = hit.point - (rayDirection * avoidOcclusionBufferLength);
            position = Vector3.SmoothDamp(affectedCamera.transform.position, targetPosition, ref avoidOcclusionVelocity, avoidOcclusionSmoothingTime);
        }
    }

    private void ReadInput() {
        if (Application.isFocused) Cursor.lockState = CursorLockMode.Locked;
        else {
            Cursor.lockState = CursorLockMode.None;
            return;
        }

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
