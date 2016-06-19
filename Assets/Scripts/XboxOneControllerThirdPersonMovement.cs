using UnityEngine;
using System.Collections;
using Utility;

public class XboxOneControllerThirdPersonMovement : MonoBehaviour
{
    public GameObject main_camera;
    public bool move = false;
    Rigidbody rb;
    Rotation rt;

    float movementSpeed = 100f;
    float jumpPower = 600f;

    Vector3 movement_direction = Vector3.zero;
    bool jump = false;
    bool isGrounded = false;

    AimingSystem aiming_system;
    ThirdPersonTargetingSystem tps;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        tps = GetComponent<ThirdPersonTargetingSystem>();
    }

    void Awake()
    {
        rt = new Rotation();
        Physics.gravity = new Vector3(0, -18f, 0);
    }

    void Update()
    {
        if (Input.GetAxis("LeftJoystickY") != 0 || Input.GetAxis("LeftJoystickX") != 0)
        {
            move = true;
        }

        if (Input.GetAxis("RightTriggerAxis") == 0 && !tps.locked_on && move)
        {
            CalculateFreeRoamRotation();
        }
        else if (Input.GetAxis("RightTriggerAxis") > 0 && !tps.locked_on)
        {
            CalculateTargetingRotation(tps.direction);
        }
        else if (tps.locked_on)
        {
            //to account for race condition with target object and locked_on variables
            if (tps.target != null)
                CalculateTargetingRotation(tps.target.transform.position);
        }

        if (Input.GetButton("A") && isGrounded)
        {
            jump = true;
        }
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

    float CalculateThirdPersonXZRotation()
    {
        // Rotation to camera
        float theta_1 = rt.CalculateXZRotation(new Vector3(main_camera.transform.forward.x, 0, main_camera.transform.forward.z));
        // Rotation to new direction
        float theta_2 = rt.CalculateXZRotation(new Vector3(Input.GetAxis("LeftJoystickX"), 0, -Input.GetAxis("LeftJoystickY")));
        // Rotation to player's forward vector
        float theta_3 = rt.CalculateXZRotation(new Vector3(transform.forward.x, 0, transform.forward.z));
        // Rotation movement direction
        float theta_final = theta_1 + theta_2 - theta_3;

        return theta_final;
    }

    // Calculates movement direction
    void CalculateFreeRoamRotation()
    {
        float theta_final = CalculateThirdPersonXZRotation();

        if (Input.GetAxis("LeftTriggerAxis") > 0)
        {
            movement_direction = Quaternion.Euler(0, theta_final, 0) * transform.forward;
        }
        else
        {
            transform.Rotate(Vector3.up, theta_final);
            movement_direction = transform.forward;
        }
    }

    void CalculateTargetingRotation(Vector3 target)
    {
        // Rotation to target
        Vector3 to_target = target - transform.position;
        float theta_to_target = rt.CalculateXZRotation(new Vector3(to_target.x, 0, to_target.z));
        float theta_to_forward = rt.CalculateXZRotation(new Vector3(transform.forward.x, 0, transform.forward.z));
        float theta_to_rotate = theta_to_target - theta_to_forward;

        // add code to rotate around player's x axis to face the target

        transform.Rotate(Vector3.up, theta_to_rotate);

        if (Input.GetAxis("LeftJoystickY") != 0 || Input.GetAxis("LeftJoystickX") != 0)
        {
            float theta_final = CalculateThirdPersonXZRotation();
            movement_direction = Quaternion.Euler(0, theta_final, 0) * transform.forward;
        }
        else
        {
            movement_direction = Vector3.zero;
        }
    }
}
