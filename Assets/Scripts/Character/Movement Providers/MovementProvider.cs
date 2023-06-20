using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementProvider : ScriptableObject
{
    protected GameObject target;
    public Action<float> OnJump;

    public abstract void Initialize(GameObject target);
    public abstract MovementState GetMovementState();
}

public struct MovementState {
    public Vector2 direction;
    public float speed;
    public bool isCrouched;
}
