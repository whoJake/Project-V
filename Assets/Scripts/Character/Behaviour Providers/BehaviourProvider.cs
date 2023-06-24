using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BehaviourProvider : ScriptableObject
{
    protected EntityController controller;
    public bool IsActive { get; private set; } = true;

    public abstract void Initialize(EntityController _controller);
    public virtual void OnFrameUpdate() { }
    public virtual void OnPhysicsUpdate() { }

    public virtual void Enable() {
        IsActive = true;
    }

    public virtual void Disable() {
        IsActive = false;
    }
}
