using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Behaviour Providers/Change Color")]
public class ChangeColorBehaviour : BehaviourProvider {

    [SerializeField] private Color primaryColor;
    [SerializeField] private Color secondaryColor;
    [SerializeField] private float swapPeriod;

    [SerializeField] private GameObject prefab;

    private bool internalSwap;
    private Material internalMaterial;
    private float timeSinceLast = 0f;

    public override void Initialize(EntityController _controller) {
        controller = _controller;
        internalMaterial = controller.GetComponent<MeshRenderer>().material;
        Enable();
    }

    public override void OnFrameUpdate() {
        if (Input.GetKeyDown(KeyCode.F)){
            if(Physics.Raycast(controller.transform.position, controller.transform.forward, out RaycastHit hit, 100f)) {
                Projectile.Spawn(prefab, controller.transform.position + controller.transform.forward, hit.point);
            }
        }


        timeSinceLast += Time.deltaTime / swapPeriod;
        if(timeSinceLast > 1) {
            timeSinceLast -= 1;
            internalSwap = !internalSwap;
        }

        Color a = internalSwap ? primaryColor : secondaryColor;
        Color b = internalSwap ? secondaryColor : primaryColor;

        internalMaterial.color = Color.Lerp(a, b, timeSinceLast);
    }

    public override void Disable() {
        base.Disable();
        internalMaterial.color = Color.white;
    }

}
