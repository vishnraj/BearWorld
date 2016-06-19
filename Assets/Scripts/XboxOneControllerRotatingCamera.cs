using UnityEngine;
using System.Collections;
using Utility;

public class XboxOneControllerRotatingCamera : MonoBehaviour
{
    public Transform target;
    public float distance;
    public float x = 0.0f;
    public float y = 0.0f;

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
    }

    static float ClampAngle(float angle, float min, float max)
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
