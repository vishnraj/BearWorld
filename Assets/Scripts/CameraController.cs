using UnityEngine;
using InputEvents;
using InventoryEvents;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public GameObject event_manager;

    FixedCamera fc;
    XboxOneControllerRotatingCamera xrc;

    bool left_trigger_pressed = false;

    public delegate void DoUpdate();
    DoUpdate update;

    // Use this for initialization
    void Start()
    {
        fc = GetComponent<FixedCamera>();
        xrc = GetComponent<XboxOneControllerRotatingCamera>();

        event_manager.GetComponent<InputManager>().publisher.InputEvent += GlobalInputEventsCallback;
        player.GetComponent<Inventory>().publisher.InventoryEvent += InventoryEventCallback;

        update = DefaultUpdate;
    }

    private void Awake() {
        
    }

    // Update is called once per frame
    void Update()
    {
        update();
    }

    void DefaultUpdate() {
        if (!xrc.enabled) {
            xrc.enabled = true;
        }
    }

    void EquippedUpdate() {
        if (Input.GetAxis("LeftTriggerAxis") > 0 && !left_trigger_pressed) {
            xrc.enabled = false;
            fc.enabled = true;

            left_trigger_pressed = true;
        }

        if (Input.GetAxis("LeftTriggerAxis") == 0 && left_trigger_pressed) {
            xrc.enabled = true;
            fc.enabled = false;

            left_trigger_pressed = false;
        }
    }

    void InventoryEventCallback(object sender, InventoryEvents.INVENTORY_EVENT e) {
        switch (e) {
            case INVENTORY_EVENT.EQUIP: {
                    update = EquippedUpdate;
                }
                break;
            case INVENTORY_EVENT.UNEQUIP: {
                    update = DefaultUpdate;
                    left_trigger_pressed = false;
                }
                break;
        }
    }

    void GlobalInputEventsCallback(object sender, InputEvents.INPUT_EVENT e) {
        switch (e) {
            case INPUT_EVENT.PAUSE: {
                    if (left_trigger_pressed) {
                        fc.enabled = false;
                    }
                    
                    enabled = false;
                }
                break;
            case INPUT_EVENT.UNPAUSE: {
                    if (left_trigger_pressed) {
                        fc.enabled = true;
                    }

                    enabled = true;
                }
                break;
            default:
                break;
        }
    }
}
