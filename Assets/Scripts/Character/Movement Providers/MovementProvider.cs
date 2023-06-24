using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementProvider : ScriptableObject
{
    protected EntityController controller;
    protected Transform target;
    public Action<float> OnJump;

    public virtual void Initialize(EntityController _controller) {
        controller = _controller;
    }

    public virtual void SetTarget(Transform _target) {
        target = _target;
    }

    public abstract MovementState GetMovementState();
}

public struct MovementState {
    public Vector2 direction;
    public float speed;
    public bool isCrouched;
}
