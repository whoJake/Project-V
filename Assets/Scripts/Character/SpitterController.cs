using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpitterController : EntityController
{
    [SerializeField] private Transform attackTarget;

    protected override void Awake() {
        base.Awake();
        movementProvider.SetTarget(attackTarget);
    }
}
