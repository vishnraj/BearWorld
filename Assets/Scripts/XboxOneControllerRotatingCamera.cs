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

    float x_speed = 250.0f;
    float y_speed = 120.0f;

    float y_min_limit = -20;
    float y_max_limit = 80;

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
        if (target == null) return;

        Reposition();
    }

    void Reposition() {
        Vector3 vec = new Vector3(0.0f, 0.0f, -distance);

        x += (float)(Input.GetAxis("RightJoystickX") * x_speed * 0.02);
        y -= (float)(-Input.GetAxis("RightJoystickY") * y_speed * 0.02);

        y = ClampAngle(y, y_min_limit, y_max_limit);

        Quaternion rotation = Quaternion.Euler(y, x, 0);

        var position = rotation * vec + target.position;

        transform.rotation = rotation;
        transform.position = position;
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
