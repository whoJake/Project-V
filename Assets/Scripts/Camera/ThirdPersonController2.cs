using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController2 : MonoBehaviour
{
    [SerializeField]
    private Transform focusPoint;

    [SerializeField]
    private Transform rotationPoint;

    [SerializeField]
    private Vector2 verticalLookClamps;

    [SerializeField]
    private Vector2 mouseSensitivity;

    [SerializeField]
    private float length;

    [SerializeField]
    private float currentLookX;

    [SerializeField]
    private float currentLookY = Mathf.PI / 2f;

    [SerializeField]
    private Transform controlTransform;

    [SerializeField]
    private bool invertY;

    [SerializeField]
    private ControlTransformType controlTransformType;

    [SerializeField]
    private Camera controlCamera;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        if (!controlCamera)
            controlCamera = Camera.main;
    }

    private void LateUpdate() {
        ReadInputs();

        UpdateLookDirection();
        UpdatePosition();

        HandleTransformDirection();
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
        controlCamera.transform.position = rotationPoint.position - controlCamera.transform.forward * length;
    }

    private void HandleTransformDirection() {
        Vector3 vecFromCamera = (focusPoint.position - controlCamera.transform.position).normalized;

        switch (controlTransformType) {
            case ControlTransformType.None:
                return;
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

    public enum ControlTransformType {
        FullControl,
        NoVerticalControl,
        None
    }
}
