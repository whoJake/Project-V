using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [SerializeField]    private Camera controlCamera;

    [SerializeField]    private Transform focusPoint;
    [SerializeField]    private Vector2 verticalLookClamps;
    [SerializeField]    private bool invertY;

    [Space]
    [SerializeField]    private Vector2 mouseSensitivity;
    [SerializeField]    private Transform rotationPoint;
    [SerializeField]    private float length;

    [Space]
    [SerializeField]    private bool willControlTransform;
    [SerializeField]    private Transform controlTransform;
    [SerializeField]    private ControlTransformType controlTransformType;

    [Space]
    [SerializeField]    private bool shouldAvoidOcclusion;
    [SerializeField]    private LayerMask occlusionLayerMask;
    [SerializeField]    private float avoidBuffer;
    [SerializeField]    private float avoidSmoothingTime;


    private float currentLookX;
    private float currentLookY = Mathf.PI / 2f;

    private float avoidSmoothingVelocity;
    private float currentLength;


    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        currentLength = length;
        if (!controlCamera)
            controlCamera = Camera.main;
    }

    private void LateUpdate() {
        ReadInputs();

        UpdateLookDirection();
        UpdatePosition();

        if(willControlTransform) HandleTransformDirection();

        if (shouldAvoidOcclusion)
            AvoidOcclusion();
        else
            currentLength = length;
    }

    public void ManualUpdate() {
        currentLength = length;
        if (!controlCamera)
            controlCamera = Camera.main;

        UpdateLookDirection();
        UpdatePosition();
    }

    private void ReadInputs() {
        if (Application.isFocused && Cursor.lockState == CursorLockMode.Locked) {
            float tau = 2 * Mathf.PI;

            Vector2 lookInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y") * (invertY ? -1 : 1));
            lookInput *= mouseSensitivity * 0.05f;

            currentLookY = Mathf.Clamp(currentLookY - lookInput.y, verticalLookClamps.x * Mathf.Deg2Rad, Mathf.PI - verticalLookClamps.y * Mathf.Deg2Rad);
            currentLookX = Mathf.Repeat(currentLookX + lookInput.x, tau);
        }
    }

    private void UpdateLookDirection() {
        controlCamera.transform.eulerAngles = new Vector3(currentLookY * Mathf.Rad2Deg - 90f,
                                                          currentLookX * Mathf.Rad2Deg, 0);
    }

    private void UpdatePosition() {
        controlCamera.transform.position = rotationPoint.position - controlCamera.transform.forward * currentLength;
    }

    private void HandleTransformDirection() {
        Vector3 vecFromCamera = (focusPoint.position - controlCamera.transform.position).normalized;

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

    private void AvoidOcclusion() {
        Vector3 camToRotate = controlCamera.transform.position - rotationPoint.position;
        Ray occlusionRay = new Ray(rotationPoint.position, camToRotate);
        bool occluded = Physics.Raycast(occlusionRay, out RaycastHit hitInfo, length, ~occlusionLayerMask);

        float nLength = length;
        if (occluded)
            nLength = Vector3.Distance(hitInfo.point, rotationPoint.position) - avoidBuffer;

        currentLength = Mathf.SmoothDamp(currentLength, nLength, ref avoidSmoothingVelocity, avoidSmoothingTime);
    }

    public enum ControlTransformType {
        FullControl,
        NoVerticalControl
    }
}
