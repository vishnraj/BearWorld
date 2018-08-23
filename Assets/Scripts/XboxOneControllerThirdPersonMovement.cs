using System.Collections.Generic;
using UnityEngine;
using Utility;
using InputEvents;
using TargetingEvents;
using AimingEvents;
using PlayerAttackEvents;

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

    Rigidbody rb;
    Rotation rt;
    GameObject current_target; // this is received from targeting system - it tells us where to point if locked_on

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

                        Debug.Log("Weapon override start for: " + weapon_name);
                    }
                }
                break;
            case PLAYER_ATTACK_EVENT.SPECIAL_ATTACK_END: {
                    if (MovementOverrideMap.Instance(this).ContainsKey(weapon_name)) {
                        // possibly something will be needed - however, the actually switch
                        // back to default update will be handled by the override function -
                        // once the override movement is complete, it then sets the update
                        // back to default and will unset the in_special_attack flag

                        Debug.Log("Weapon override end for: " + weapon_name);
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
        if (Input.GetAxis("LeftJoystickY") != 0 || Input.GetAxis("LeftJoystickX") != 0) {
            update_xz_user_in_non_fixed();

            update_xz_user_in_fixed = XZUserInFixedUpdate;
        }
        else {
            update_xz_user_in_fixed = DefaultFixedUpdate;
        }

        if (Input.GetButton("A") && grounded) {
            update_non_xz_user_in_fixed = JumpingFixedUpdate; // we will likely change this behavior
                                                              // in the case of a sword attack, we will
                                                              // launch towards the enemy
        }
        else {
            update_non_xz_user_in_fixed = DefaultFixedUpdate;
        }

        update_non_xz_user_in_non_fixed();

        // placeholder for now, but there will be additional checks,
        // regarding the above movement, which will need to take place
        // before we can switch back to default update and turn off the flag
        update = DefaultUpdate;
        in_special_attack = false;
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
