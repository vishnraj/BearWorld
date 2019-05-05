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
        event_manager.GetComponent<ComponentEventManager>().inventory_publisher.InventoryEvent += InventoryEventCallback;

        // This has to manage FixedCamera's events setup, because FixedCamera is disabled at start
        event_manager.GetComponent<ComponentEventManager>().inventory_publisher.InventoryEvent += fc.InventoryEventCallback;
        event_manager.GetComponent<ComponentEventManager>().targeting_publisher.TargetingEvent += fc.TargetingEventCallback;

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
        if (left_trigger_pressed) {
            xrc.enabled = false;
            fc.enabled = true;
        }

        if (!left_trigger_pressed) {
            xrc.enabled = true;
            fc.enabled = false;
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
                    // attempting to maintain camera on target
                    // through the pause menu
                    if (left_trigger_pressed) {
                        fc.enabled = true;
                    }

                    enabled = true;
                }
                break;
            case INPUT_EVENT.L_TRIGGER_SET: {
                    left_trigger_pressed = true;
                }
                break;
            case INPUT_EVENT.L_TRIGGER_UNSET: {
                    left_trigger_pressed = false;
                }
                break;
            default:
                break;
        }
    }
}
