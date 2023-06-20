using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Behaviour Providers/Change Color")]
public class ChangeColorBehaviour : BehaviourProvider {

    [SerializeField] private Color primaryColor;
    [SerializeField] private Color secondaryColor;
    [SerializeField] private float swapPeriod;

    private bool internalSwap;
    private Material internalMaterial;
    private float timeSinceLast = 0f;

    public override void Initialize(GameObject target) {
        this.target = target;
        internalMaterial = target.GetComponent<MeshRenderer>().material;
    }

    public override void OnFrameUpdate() {
        timeSinceLast += Time.deltaTime / swapPeriod;
        if(timeSinceLast > 1) {
            timeSinceLast -= 1;
            internalSwap = !internalSwap;
        }

        Color a = internalSwap ? primaryColor : secondaryColor;
        Color b = internalSwap ? secondaryColor : primaryColor;

        internalMaterial.color = Color.Lerp(a, b, timeSinceLast);
    }

    public override void OnPhysicsUpdate() {
        
    }
}
