using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Movement Providers/User Movement")]
public class UserMovement : MovementProvider {
    [SerializeField]
    private float movementSpeed;
    [SerializeField]
    private int waitForLayer = -1;
    private bool active = false;

    [SerializeField]
    private float jumpPower;

    public override void Initialize(EntityController _controller) {
        base.Initialize(_controller);
        if (waitForLayer != -1) {
            controller.useGravity = false;
            TerrainHandler.OnLayerGenerated += (int index) => { if (index == waitForLayer) { active = true; controller.useGravity = true; } };
        } else {
            active = true;
        }
    }

    public override MovementState GetMovementState() {
        if (!active) {
            return new MovementState();
        }

        if (Input.GetButtonDown("Jump") && controller.isGrounded) {
            OnJump?.Invoke(jumpPower);
        }

        return new MovementState {  direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized,
                                    speed = movementSpeed,
                                    isCrouched = false
                                    };
    }
}
