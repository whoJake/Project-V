using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BehaviourProvider : ScriptableObject
{
    protected EntityController controller;
    public bool IsActive { get; private set; } = false;

    public virtual void Initialize(EntityController _controller) {
        controller = _controller;
        Enable();
    }
    public virtual void OnFrameUpdate() { }
    public virtual void OnPhysicsUpdate() { }

    public virtual void Enable() {
        IsActive = true;
    }

    public virtual void Disable() {
        IsActive = false;
    }
}
