using System.Collections.Generic;
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
    float jump_power = 1000f;
    Vector3 desired_direction = Vector3.zero; // this is received from aiming system - if not locked on, but equipped, this determine direction
    Vector3 movement_direction = Vector3.zero;
    bool grounded = true;

    // margin for distance to enemy - needed in some calculations
    float to_target_gap_threshold = 2.0f;
    float special_attack_start = 0.0f;
    bool in_special_attack = false;
    Vector3 snapped_start_pos_special_attack = Vector3.zero;

    // specific to special attack bools
    static class SpecialAttackFlags {
        static public bool sword_jump_attack = false;

        static public void SetAllFlagsOff() {
            sword_jump_attack = false;
        }
    }

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
    DoUpdate fixed_update;

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

        update_non_xz_user_in_non_fixed = DoNothingUpdate;
        update_xz_user_in_non_fixed = UnequippedMoveUpdate;

        update_non_xz_user_in_fixed = DoNothingFixedUpdate;
        update_xz_user_in_fixed = DoNothingFixedUpdate;

        update = DefaultUpdate;
        fixed_update = DefaultFixedUpdate;
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
                        special_attack_start = Time.time;
                        snapped_start_pos_special_attack = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                        in_special_attack = true;
                    }
                }
                break;
            case PLAYER_ATTACK_EVENT.SPECIAL_ATTACK_END: {
                    if (MovementOverrideMap.Instance(this).ContainsKey(weapon_name)) {
                        in_special_attack = false; // treat this different as a given special attack will choose its clean up process
                    }
                }
                break;
            case PLAYER_ATTACK_EVENT.SPECIAL_ATTACK_TERMINATE: {
                    if (MovementOverrideMap.Instance(this).ContainsKey(weapon_name)) {
                        UnsetSpecialAttack(); // terminates don't care about the special attack clean up process, just kill whatever is happening
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
                    update_xz_user_in_non_fixed = DoNothingUpdate; // moving doesn't influence the rotations or movement direction now
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
                update_non_xz_user_in_non_fixed = DoNothingUpdate;
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
        fixed_update();
    }

    void OnCollisionEnter(Collision collide) {
        if (collide.gameObject.tag == "Ground") {
            grounded = true;
        }
    }

    void DefaultUpdate() {
        if (Input.GetAxis("LeftJoystickY") != 0 || Input.GetAxis("LeftJoystickX") != 0) {
            update_xz_user_in_non_fixed();

            update_xz_user_in_fixed = XZUserInFixedUpdate;
        }
        else {
            update_xz_user_in_fixed = DoNothingFixedUpdate;
        }

        if (Input.GetButton("A") && grounded) {
            update_non_xz_user_in_fixed = JumpingFixedUpdate;
        }
        else {
            update_non_xz_user_in_fixed = DoNothingFixedUpdate;
        }

        update_non_xz_user_in_non_fixed();
    }

    void DefaultFixedUpdate() {
        update_non_xz_user_in_fixed();
        update_xz_user_in_fixed();
    }

    void SwordTargetingUpdate() {
        if (!in_special_attack) {
            SpecialAttackFlags.SetAllFlagsOff();
            update = DefaultUpdate;
            fixed_update = DefaultFixedUpdate;
            return;
        }

        float to_target_y_threshold = 1.0f;
        if (!grounded && !SpecialAttackFlags.sword_jump_attack && ((current_target.transform.position.y - transform.position.y) > to_target_y_threshold)) {
            SpecialAttackFlags.sword_jump_attack = true; // matters that it gets set in the first update
            fixed_update = JumpingTowardsTargetFixedUpdate;
            // update_non_xz_user_in_fixed = JumpOverTarget;
        } else if (SpecialAttackFlags.sword_jump_attack) {
            // fixed_update = JumpingTowardsTargetFixedUpdate;
            // update_non_xz_user_in_fixed = DoNothingFixedUpdate;
        }

        if (!SpecialAttackFlags.sword_jump_attack) {
            update_xz_user_in_fixed = FlyTowardsTargetFixedUpdate;
            update_non_xz_user_in_fixed = DoNothingFixedUpdate;
        }

        // update_xz_user_in_fixed = FlyTowardsTargetFixedUpdate;
        update_non_xz_user_in_non_fixed();
    }

    void DoNothingFixedUpdate() {
        // NOOP
    }

    void XZUserInFixedUpdate() {
        rb.AddForce(movement_direction * movement_speed);
    }

    void JumpingFixedUpdate() {
        rb.AddForce(transform.up * jump_power);
        grounded = false;
    }

    void FlyTowardsTargetFixedUpdate() {
        if (in_special_attack) {
            Vector2 player_xz_pos = new Vector2(transform.position.x, transform.position.z);
            Vector2 xz_target_pos = new Vector2(current_target.transform.position.x, current_target.transform.position.z);

            if (Mathf.Abs(Vector3.Magnitude(xz_target_pos - player_xz_pos)) <= to_target_gap_threshold) {
                publisher.OnMovementEvent(MOVEMENT_EVENT.SPECIAL_ATTACK_END);
                return;
            }

            float sword_attack_speed = 200f;
            // the relative force will take care of moving towards the target by passing foward
            rb.AddRelativeForce(Vector3.forward * sword_attack_speed, ForceMode.Force);
        }
    }

    void JumpOverTarget() {

    }

    void JumpingTowardsTargetFixedUpdate() {
        if (in_special_attack) {
            if (Mathf.Abs(Vector3.Magnitude(current_target.transform.position - transform.position)) <= to_target_gap_threshold) {
                publisher.OnMovementEvent(MOVEMENT_EVENT.SPECIAL_ATTACK_END);
                return;
            }

            float time_elapsed = Time.time - special_attack_start;
            float percent_time = time_elapsed / 1.0f;

            Vector3 dest = new Vector3(current_target.transform.position.x, current_target.transform.position.y + 1, current_target.transform.position.z);
            transform.position = Vector3.Slerp(snapped_start_pos_special_attack, dest, percent_time);
        }
    }

    private float CalculateJumpSpeed(float jumpHeight, float gravity) {
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }

    void DoNothingUpdate() {
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

    void UnsetSpecialAttack() {
        in_special_attack = false;
        SpecialAttackFlags.SetAllFlagsOff();
        update = DefaultUpdate;
        fixed_update = DefaultFixedUpdate;
    }
}
