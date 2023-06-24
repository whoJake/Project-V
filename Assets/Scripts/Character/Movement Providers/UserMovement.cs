using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Movement Providers/User Movement")]
public class UserMovement : MovementProvider {
    [SerializeField]
    private float movementSpeed;

    [SerializeField]
    private float jumpPower;

    public override MovementState GetMovementState() {
        if (Input.GetButtonDown("Jump") && controller.isGrounded) {
            OnJump?.Invoke(jumpPower);
        }

        return new MovementState {  direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized,
                                    speed = movementSpeed,
                                    isCrouched = false
                                    };
    }
}
