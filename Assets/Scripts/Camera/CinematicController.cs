using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicController : MonoBehaviour
{
    [SerializeField]
    private Camera linkedCamera;

    [SerializeField]
    private Transform startPoint;

    [SerializeField]
    private Transform endPoint;

    [SerializeField]
    private float timeToComplete;

    [SerializeField]
    private float rotationSpeed;

    private bool begun = false;
    private float timeElapsed = 0;

    private LineRail rail;

    private void Start() {
        rail = new LineRail(startPoint.position, endPoint.position);
    }

    private void Update() {
        if (!begun) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                begun = true;
            }
            return;
        }

        linkedCamera.transform.position = rail.Evaluate(Mathf.InverseLerp(0, timeToComplete, timeElapsed));
        linkedCamera.transform.rotation = Quaternion.Euler(linkedCamera.transform.rotation.eulerAngles.x, linkedCamera.transform.rotation.eulerAngles.y + rotationSpeed * Time.deltaTime, 0);
        timeElapsed += Time.deltaTime;
    }

}
