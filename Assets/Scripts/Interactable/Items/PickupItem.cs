using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItem : Interactable
{
    public StatBundle itemStats;
    //Item model
    //Item tier
    //Item element

    public override void OnInteract(GameObject target) {
        if (!active) return;

        StatHandler targetHandler;
        if (target.TryGetComponent<StatHandler>(out targetHandler)) {
            active = false;

            itemStats.ApplyToStatHandler(targetHandler);

            //Play some pickup animation
            //Play some destroy animation

            Destroy(this.gameObject);
        }
    }
}
