using UnityEngine;
using System.Collections;
using UnityStandardAssets.Utility;

public class CameraController : MonoBehaviour
{
    FixedCamera fc;
    XboxOneControllerRotatingCamera xrc;

    bool left_trigger_pressed = false;

    // Use this for initialization
    void Start()
    {
        fc = GetComponent<FixedCamera>();
        xrc = GetComponent<XboxOneControllerRotatingCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("LeftTriggerAxis") > 0 && !left_trigger_pressed)
        {
            xrc.enabled = false;
            fc.enabled = true;

            left_trigger_pressed = true;
        }

        if (Input.GetAxis("LeftTriggerAxis") == 0 && left_trigger_pressed)
        {
            if (fc.targeting) {
                transform.Rotate(-fc.target_rotation_x, 0, 0);
                fc.targeting = false;
            }

            xrc.enabled = true;
            fc.enabled = false;

            left_trigger_pressed = false;
        }
    }
}
