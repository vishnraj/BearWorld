using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;

    FixedCamera fc;
    XboxOneControllerRotatingCamera xrc;
    PlayerAttackController pac;

    bool left_trigger_pressed = false;

    // Use this for initialization
    void Start()
    {
        
    }

    private void Awake() {
        fc = GetComponent<FixedCamera>();
        xrc = GetComponent<XboxOneControllerRotatingCamera>();
        pac = player.GetComponent<PlayerAttackController>();        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("LeftTriggerAxis") > 0 && !left_trigger_pressed && pac.enabled)
        {
            xrc.enabled = false;
            fc.enabled = true;

            left_trigger_pressed = true;
        }

        if (Input.GetAxis("LeftTriggerAxis") == 0 && left_trigger_pressed)
        {
            xrc.enabled = true;
            fc.enabled = false;

            left_trigger_pressed = false;
        }
    }
}
