﻿using System.Collections.Generic;
using UnityEngine;
using Utility;
using InputEvents;
using TargetingEvents;
using AimingEvents;
using PlayerAttackEvents;
using MovementEvents;

public class XboxOneControllerThirdPersonMovement : MonoBehaviour
{
    public GameObject main_camera;
    public GameObject event_manager;

    float movement_speed = 100f;
    float jump_power = 600f;
    Vector3 desired_direction = Vector3.zero; // this is received from aiming system - if not locked on, but equipped, this determine direction
    Vector3 movement_direction = Vector3.zero;
    bool grounded = false;
    bool in_special_attack = false;

    // margin for distance to enemy - needed in some calculations
    float to_target_gap_threshold = 5f;

    Rigidbody rb;
    Rotation rt;
    GameObject current_target; // this is received from targeting system - it tells us where to point if locked_on

    MovementEventPublisher publisher;

    delegate void DoUpdate();
    DoUpdate update_non_xz_user_in_non_fixed;
    DoUpdate update_xz_user_in_non_fixed;
    DoUpdate update_non_xz_user_in_fixed;
    DoUpdate update_xz_user_in_fixed;
    DoUpdate update;

    static class MovementOverrideMap {
        static Dictionary<string, DoUpdate> weapon_name_to_override = null;

        public static Dictionary<string, DoUpdate> Instance(XboxOneControllerThirdPersonMovement parent) {
            if (weapon_name_to_override == null) {
                weapon_name_to_override = new Dictionary<string, DoUpdate>();

                weapon_name_to_override[Weapon.WeaponNames.SWORD] = parent.SwordTargetingUpdate;
            }

            return weapon_name_to_override;
        }
    }

    void Start()
    {
        publisher = event_manager.GetComponent<ComponentEventManager>().movement_publisher;

        event_manager.GetComponent<InputManager>().publisher.InputEvent += GlobalInputEventsCallback;
        event_manager.GetComponent<ComponentEventManager>().targeting_publisher.TargetingEvent += TargetingEventsCallback;
        event_manager.GetComponent<ComponentEventManager>().aiming_publisher.AimingEvent += AimingEventCallback;
        event_manager.GetComponent<ComponentEventManager>().attacks_publisher.PlayerAttackEvent += PlayerAttackEventsCallback;

        update_non_xz_user_in_non_fixed = DefaultNonFixedUpdate;
        update_xz_user_in_non_fixed = UnequippedMoveUpdate;

        update_non_xz_user_in_fixed = DefaultFixedUpdate;
        update_xz_user_in_fixed = DefaultFixedUpdate;

        update = DefaultUpdate;
    }

    void Awake()
    {
        rt = new Rotation();
        Physics.gravity = new Vector3(0, -18f, 0);
        
        rb = GetComponent<Rigidbody>();
    }

    void GlobalInputEventsCallback(object sender, INPUT_EVENT e) {
        switch (e) {
            case INPUT_EVENT.PAUSE: {
                    enabled = false;
                }
                break;
            case INPUT_EVENT.UNPAUSE: {
                    enabled = true;
                }
                break;
            default:
                break;
        }
    }

    void PlayerAttackEventsCallback(string weapon_name, PLAYER_ATTACK_EVENT e) {
        switch (e) {
            case PLAYER_ATTACK_EVENT.SPECIAL_ATTACK_START: {
                    if (!in_special_attack && MovementOverrideMap.Instance(this).ContainsKey(weapon_name)) {
                        update = MovementOverrideMap.Instance(this)[weapon_name];
                        in_special_attack = true;
                    }
                }
                break;
            case PLAYER_ATTACK_EVENT.SPECIAL_ATTACK_END: {
                    in_special_attack = false;
                }
                break;
            case PLAYER_ATTACK_EVENT.SPECIAL_ATTACK_TERMINATE: {
                    if (MovementOverrideMap.Instance(this).ContainsKey(weapon_name)) {
                        in_special_attack = false;
                        update = DefaultUpdate;
                    }
                }
                break;
            default:
                break;
        }
    }

    void TargetingEventsCallback(GameObject target, TARGETING_EVENT e) {
        switch(e) {
            case TARGETING_EVENT.LOCK_ON: {
                    update_non_xz_user_in_non_fixed = TargetingUpdate; // this is because happens in all cases and is based on target now
                    update_xz_user_in_non_fixed = DefaultNonFixedUpdate; // moving doesn't influence the rotations or movement direction now
                    current_target = target;
                }
                break;
            default:
                break;
        }
    }

    void AimingEventCallback(AimingData data, AIMING_EVENT e) {
        switch(e) {
            case AIMING_EVENT.SCANNING:
            case AIMING_EVENT.FOUND: {
                    // if we see these events, we are no longer
                    // locked_on - however in this frame, we must be
                    // equipped - else we would not receive these
                    // so it is safe to set update back to EquippedUpdate
                    update_non_xz_user_in_non_fixed = EquippedUpdate;
                    update_xz_user_in_non_fixed = EquippedMoveUpdate;
                    desired_direction = data.direction;
                }
                break;
            case AIMING_EVENT.AIM_OFF:
                update_non_xz_user_in_non_fixed = DefaultNonFixedUpdate;
                update_xz_user_in_non_fixed = UnequippedMoveUpdate;
                break;
            default:
                break;
        }
    }

    void Update()
    {
        update();
    }

    void FixedUpdate()
    {
        update_xz_user_in_fixed();
        update_non_xz_user_in_fixed();
    }

    void OnCollisionEnter(Collision collide) {
        if (collide.collider.tag == "Ground") {
            grounded = true;
        }
    }

    void OnCollisionExit(Collision collide) {
        if (collide.collider.tag == "Ground") {
            grounded = false;
        }
    }

    void DefaultUpdate() {
        if (Input.GetAxis("LeftJoystickY") != 0 || Input.GetAxis("LeftJoystickX") != 0) {
            update_xz_user_in_non_fixed();

            update_xz_user_in_fixed = XZUserInFixedUpdate;
        }
        else {
            update_xz_user_in_fixed = DefaultFixedUpdate;
        }

        if (Input.GetButton("A") && grounded) {
            update_non_xz_user_in_fixed = JumpingFixedUpdate;
        }
        else {
            update_non_xz_user_in_fixed = DefaultFixedUpdate;
        }

        update_non_xz_user_in_non_fixed();
    }

    void SwordTargetingUpdate() {
        if (!in_special_attack) {
            update = DefaultUpdate;
            return;
        }

        if (Mathf.Abs(Vector3.Magnitude(transform.position - current_target.transform.position)) <= to_target_gap_threshold) {
            publisher.OnMovementEvent(MOVEMENT_EVENT.SPECIAL_ATTACK_END);
            return;
        }

        update_xz_user_in_non_fixed();

        update_xz_user_in_fixed = FlyTowardsTargetFixedUpdate;
        update_non_xz_user_in_fixed = DefaultFixedUpdate;

        update_non_xz_user_in_non_fixed();
    }

    void DefaultFixedUpdate() {
        // NOOP
    }

    void XZUserInFixedUpdate() {
        rb.AddForce(movement_direction * movement_speed);
    }

    void JumpingFixedUpdate() {
        rb.AddForce(transform.up * jump_power);
    }

    void FlyTowardsTargetFixedUpdate() {
        if (in_special_attack) {
            rb.AddRelativeForce(Vector3.forward * movement_speed, ForceMode.Force);
        }
    }

    void DefaultNonFixedUpdate() {
        // NOOP
    }

    void UnequippedMoveUpdate() {
        FreeRoamRotationUnequipped();
    }

    void EquippedUpdate() {
        if (Input.GetAxis("RightTriggerAxis") > 0) {
            TargetingRotation(desired_direction);
        }
    }

    void EquippedMoveUpdate() {
        if (Input.GetAxis("RightTriggerAxis") == 0) {
            FreeRoamRotationEquipped();
        }
    }

    void TargetingUpdate() {
        TargetingRotation(current_target.transform.position);
    }

    float CalculateThirdPersonZXRotation()
    {
        // Rotation to camera
        float theta_1 = rt.CalculateZXRotation(new Vector3(main_camera.transform.forward.x, 0, main_camera.transform.forward.z));
        // Rotation to new direction
        float theta_2 = rt.CalculateZXRotation(new Vector3(Input.GetAxis("LeftJoystickX"), 0, -Input.GetAxis("LeftJoystickY")));
        // Rotation to player's forward vector
        float theta_3 = rt.CalculateZXRotation(new Vector3(transform.forward.x, 0, transform.forward.z));
        // Rotation movement direction
        float theta_final = theta_1 + theta_2 - theta_3;

        return theta_final;
    }

    // Calculates movement direction
    void FreeRoamRotationUnequipped()
    {
        float theta_final = CalculateThirdPersonZXRotation();
        transform.Rotate(Vector3.up, theta_final);
        movement_direction = transform.forward;
    }

    void FreeRoamRotationEquipped() {
        float theta_final = CalculateThirdPersonZXRotation();

        // For when in fixed camera view
        if (Input.GetAxis("LeftTriggerAxis") > 0) {
            movement_direction = Quaternion.Euler(0, theta_final, 0) * transform.forward;
        } else {
            transform.Rotate(Vector3.up, theta_final);
            movement_direction = transform.forward;
        }
    }

    void TargetingRotation(Vector3 target) {
        // Rotation to target
        Vector3 to_target = target - transform.position;
        float theta_to_target = rt.CalculateZXRotation(new Vector3(to_target.x, 0, to_target.z));
        float theta_to_forward = rt.CalculateZXRotation(new Vector3(transform.forward.x, 0, transform.forward.z));
        float theta_to_rotate = theta_to_target - theta_to_forward;

        transform.Rotate(Vector3.up, theta_to_rotate);

        float theta_final = CalculateThirdPersonZXRotation();
        movement_direction = Quaternion.Euler(0, theta_final, 0) * transform.forward;
    }
}
