using UnityEngine;
using System.Collections;
using Utility;

public class XboxOneControllerRotatingCamera : MonoBehaviour
{
    [SerializeField]
    Transform target;
    [SerializeField]
    float distance;
    [SerializeField]
    float x = 0.0f;
    [SerializeField]
    float y = 0.0f;

    float xSpeed = 250.0f;
    float ySpeed = 120.0f;

    float yMinLimit = -20;
    float yMaxLimit = 80;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void Awake()
    {
    }

    void LateUpdate()
    {
        Vector3 vec = new Vector3(0.0f, 0.0f, -distance);

        if (target)
        {
            x += (float)(Input.GetAxis("RightJoystickX") * xSpeed * 0.02);
            y -= (float)(-Input.GetAxis("RightJoystickY") * ySpeed * 0.02);

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);

            var position = rotation * vec + target.position;

            transform.rotation = rotation;
            transform.position = position;
        }

        // iterate the objects in stage that are considered barriers/ground - if the slice that is the viewport
        // is outside the bounds of these, then shrink viewport to fit inside
        // if the bounds are greater than the viewport at 1W and 1H, then don't adjust
        // We also need the camera to come closer to the player when it's going to be outside the bounds, not sure how to do this
            // It's likely that this will require viewport manipulation as well - 
            // basically something that says if the view is going to contain things outside the boundaries, don't allow camera to move to that location
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
        {
            angle += 360;
        }

        if (angle > 360)
        {
            angle -= 360;
        }

        return Mathf.Clamp(angle, min, max);
    }
}
