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
    public bool move = false;

    public delegate void DoUpdate();
    DoUpdate update;

    float movementSpeed = 100f;
    float jumpPower = 600f;
    Vector3 desired_direction = Vector3.zero; // this is received from aiming system - if not locked on, but equipped, this determine direction
    Vector3 movement_direction = Vector3.zero;
    bool jump = false;
    bool isGrounded = false;

    Rigidbody rb;
    Rotation rt;
    GameObject current_target; // this is received from targeting system - it tells us where to point if locked_on

    void Start()
    {
        event_manager.GetComponent<InputManager>().publisher.InputEvent += GlobalInputEventsCallback;
        event_manager.GetComponent<ComponentEventManager>().targeting_publisher.TargetingEvent += TargetingEventsCallback;
        event_manager.GetComponent<ComponentEventManager>().aiming_publisher.AimingEvent += AimingEventCallback;
        update = DefaultUpdate;
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
                    update = TargetingUpdate;
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
                    update = EquippedUpdate;
                    desired_direction = data.direction;
                }
                break;
            case AIMING_EVENT.AIM_OFF:
                update = DefaultUpdate;
                break;
            default:
                break;
        }
    }

    void Update()
    {
        // the input we gather here is also for FixedUpdate on the next frame
        if (Input.GetAxis("LeftJoystickY") != 0 || Input.GetAxis("LeftJoystickX") != 0) {
            move = true;
        }
        if (Input.GetButton("A") && isGrounded) {
            jump = true;
        }

        update();
    }

    void DefaultUpdate() {
        if (move) {
            CalculateFreeRoamRotationUnequipped();
        }
    }

    void EquippedUpdate() {
        if (Input.GetAxis("RightTriggerAxis") == 0 && move) {
            CalculateFreeRoamRotationEquipped();
        } else if (Input.GetAxis("RightTriggerAxis") > 0) {
            CalculateTargetingRotation(desired_direction);
        }
    }

    void TargetingUpdate() {
        CalculateTargetingRotation(current_target.transform.position);
    }

    void FixedUpdate()
    {
        if (move)
        {
            rb.AddForce(movement_direction * movementSpeed);
            move = false;
        }

        if (jump)
        {
            rb.AddForce(transform.up * jumpPower);
            jump = false;
        }
    }

    void OnCollisionEnter(Collision collide)
    {
        if (collide.collider.tag == "Ground")
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collide)
    {
        if (collide.collider.tag == "Ground")
        {
            isGrounded = false;
        }
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
    void CalculateFreeRoamRotationUnequipped()
    {
        float theta_final = CalculateThirdPersonZXRotation();
        transform.Rotate(Vector3.up, theta_final);
        movement_direction = transform.forward;
    }

    void CalculateFreeRoamRotationEquipped() {
        float theta_final = CalculateThirdPersonZXRotation();

        // For when in fixed camera view
        if (Input.GetAxis("LeftTriggerAxis") > 0) {
            movement_direction = Quaternion.Euler(0, theta_final, 0) * transform.forward;
        } else {
            transform.Rotate(Vector3.up, theta_final);
            movement_direction = transform.forward;
        }
    }

    void CalculateTargetingRotation(Vector3 target) {
        // Rotation to target
        Vector3 to_target = target - transform.position;
        float theta_to_target = rt.CalculateZXRotation(new Vector3(to_target.x, 0, to_target.z));
        float theta_to_forward = rt.CalculateZXRotation(new Vector3(transform.forward.x, 0, transform.forward.z));
        float theta_to_rotate = theta_to_target - theta_to_forward;

        transform.Rotate(Vector3.up, theta_to_rotate);

        if (move) {
            float theta_final = CalculateThirdPersonZXRotation();
            movement_direction = Quaternion.Euler(0, theta_final, 0) * transform.forward;
        } else {
            movement_direction = Vector3.zero;
        }
    }
}
