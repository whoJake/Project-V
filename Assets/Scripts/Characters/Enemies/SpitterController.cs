using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpitterController : EntityController
{
    [SerializeField] private GameObject attackTarget;

    protected override void Awake() {
        base.Awake();
        //lockonTarget = attackTarget;
    }
}
