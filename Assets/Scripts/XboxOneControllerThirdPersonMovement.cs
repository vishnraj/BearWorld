using UnityEngine;
using Utility;
using InputEvents;
using TargetingEvents;
using InventoryEvents;
using AimingEvents;

public class XboxOneControllerThirdPersonMovement : MonoBehaviour
{
    public GameObject main_camera;
    public GameObject event_manager;

    public delegate void DoUpdate();

    DoUpdate update_non_move_non_fixed;
    DoUpdate update_move_non_fixed;
    DoUpdate update_non_move_fixed;
    DoUpdate update_move_fixed;

    float movementSpeed = 100f;
    float jumpPower = 600f;
    Vector3 desired_direction = Vector3.zero; // this is received from aiming system - if not locked on, but equipped, this determine direction
    Vector3 movement_direction = Vector3.zero;
    bool isGrounded = false;

    Rigidbody rb;
    Rotation rt;
    GameObject current_target; // this is received from targeting system - it tells us where to point if locked_on

    void Start()
    {
        event_manager.GetComponent<InputManager>().publisher.InputEvent += GlobalInputEventsCallback;
        event_manager.GetComponent<ComponentEventManager>().targeting_publisher.TargetingEvent += TargetingEventsCallback;
        event_manager.GetComponent<ComponentEventManager>().aiming_publisher.AimingEvent += AimingEventCallback;

        update_non_move_non_fixed = DefaultUpdate;
        update_move_non_fixed = UnequippedMoveUpdate;

        update_non_move_fixed = DefaultFixedUpdate;
        update_move_fixed = DefaultFixedUpdate;
    }

    void Awake()
    {
        rt = new Rotation();
        Physics.gravity = new Vector3(0, -18f, 0);
        
        rb = GetComponent<Rigidbody>();
    }

    void GlobalInputEventsCallback(object sender, InputEvents.INPUT_EVENT e) {
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

    void TargetingEventsCallback(GameObject target, TARGETING_EVENT e) {
        switch(e) {
            case TARGETING_EVENT.LOCK_ON: {
                    update_non_move_non_fixed = TargetingUpdate; // this is because happens in all cases and is based on target now
                    update_move_non_fixed = DefaultUpdate; // moving doesn't influence the rotations or movement direction now
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
                    update_non_move_non_fixed = EquippedUpdate;
                    update_move_non_fixed = EquippedMoveUpdate;
                    desired_direction = data.direction;
                }
                break;
            case AIMING_EVENT.AIM_OFF:
                update_non_move_non_fixed = DefaultUpdate;
                update_move_non_fixed = UnequippedMoveUpdate;
                break;
            default:
                break;
        }
    }

    void Update()
    {
        if (Input.GetAxis("LeftJoystickY") != 0 || Input.GetAxis("LeftJoystickX") != 0) {
            update_move_non_fixed();
            
            update_move_fixed = MoveFixedUpdate;
        } else {
            update_move_fixed = DefaultFixedUpdate;
        }

        if (Input.GetButton("A") && isGrounded) {
            update_non_move_fixed = JumpingFixedUpdate;
        } else {
            update_non_move_fixed = DefaultFixedUpdate;
        }

        update_non_move_non_fixed();
    }

    void FixedUpdate()
    {
        update_move_fixed();
        update_non_move_fixed();
    }

    void OnCollisionEnter(Collision collide) {
        if (collide.collider.tag == "Ground") {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collide) {
        if (collide.collider.tag == "Ground") {
            isGrounded = false;
        }
    }

    void DefaultFixedUpdate() {
        // NOOP
    }

    void MoveFixedUpdate() {
        rb.AddForce(movement_direction * movementSpeed);
    }

    void JumpingFixedUpdate() {
        rb.AddForce(transform.up * jumpPower);
    }

    void DefaultUpdate() {
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
