using UnityEngine;
using UnityEngine.UI;
using TargetingEvents;
using InventoryEvents;
using AimingEvents;
using PlayerHealthEvents;

// Used to deliver target(s) to other systems
public class AimingSystem : MonoBehaviour
{
    public GameObject crosshair;
    public Camera cam;
    public float ray_radius;
    public float desired_scale = .4f;
    public GameObject event_manager;

    // This is hard coded, it just guarantees
    // that the part of the ray used to set direction
    // is in front of the player
    float in_front_of = 70.0f;

    public Sprite default_reticle;
    public Sprite target_reticle;

    AimingPublisher publisher;
    GameObject targeting_icon;
    GameObject target;

    delegate void DoUpdate();
    DoUpdate update;

    // Use this for initialization
    void Start()
    {
        // this is something that doesn't exist in the editor
        // mainly because it's only created internally when targeting
        // is enabled vs needing to be present at all times
        targeting_icon = new GameObject();

        targeting_icon.AddComponent<Image>();
        targeting_icon.GetComponent<Image>().sprite = target_reticle;
        targeting_icon.name = "TargetingIcon";

        targeting_icon.GetComponent<Image>().transform.localScale = new Vector3(desired_scale, desired_scale, desired_scale);

        publisher = event_manager.GetComponent<ComponentEventManager>().aiming_publisher;

        event_manager.GetComponent<ComponentEventManager>().targeting_publisher.TargetingEvent += TargetingEventCallback;
        event_manager.GetComponent<ComponentEventManager>().inventory_publisher.InventoryEvent += InventoryEventCallback;
        event_manager.GetComponent<ComponentEventManager>().health_publisher.PlayerHealthEvent += PlayerHealthEventsCallback;

        update = null;
        enabled = false; // seems a little messy , but we need this to run after Awake for some of the above
    }

    // Update is called once per frame
    void Update()
    {
        if (update != null) {
            update(); // this can only get called after it is set in the callback below
        } else {
            Debug.Log("How did we get here?");
        }
    }

    void LockOnUpdate() {
        Vector2 target_screen_point = cam.WorldToViewportPoint(target.transform.position);
        RectTransform hud_rect = transform.parent.GetComponent<RectTransform>();
        Vector2 targeting_icon_position = new Vector2(
        ((target_screen_point.x * hud_rect.sizeDelta.x) - (hud_rect.sizeDelta.x * 0.5f)),
        ((target_screen_point.y * hud_rect.sizeDelta.y) - (hud_rect.sizeDelta.y * 0.5f)));

        if (!(targeting_icon.transform.parent == transform.parent)) {
            targeting_icon.transform.SetParent(transform.parent);
        }

        targeting_icon.GetComponent<RectTransform>().anchoredPosition = targeting_icon_position;
    }

    void DefaultUpdate() {
        RaycastHit hit;
        Ray ray;
        // the ray needs to be from wherever is in the current view to object
        ray = cam.ScreenPointToRay(crosshair.GetComponent<RectTransform>().position);

        int enemy_layer = 1 << LayerMask.NameToLayer("Enemy_Layer");
        if (Physics.SphereCast(ray, ray_radius, out hit, Mathf.Infinity, enemy_layer) && hit.collider.gameObject.GetComponent<EnemyHealth>() != null) {
            publisher.OnAimingEvent(new AimingData(ray.GetPoint(in_front_of), hit.collider.gameObject), AIMING_EVENT.FOUND);
        } else {
            crosshair.GetComponent<Image>().sprite = default_reticle;
            publisher.OnAimingEvent(new AimingData(ray.GetPoint(in_front_of), null), AIMING_EVENT.SCANNING);
        }
    }

    void TargetingEventCallback(GameObject _target, TARGETING_EVENT e) {
        switch (e) {
            case TARGETING_EVENT.FREE: {
                    if (enabled) {
                        crosshair.GetComponent<Image>().enabled = true;
                        targeting_icon.GetComponent<Image>().enabled = false;
                        update = DefaultUpdate;
                    }
                }
                break;
            case TARGETING_EVENT.CAN_LOCK: {
                    if (enabled) {
                        if (targeting_icon.GetComponent<Image>().enabled) {
                            crosshair.GetComponent<Image>().enabled = true;
                            targeting_icon.GetComponent<Image>().enabled = false;
                            update = DefaultUpdate;
                        }

                        crosshair.GetComponent<Image>().sprite = target_reticle;
                    }
                }
                break;
            case TARGETING_EVENT.LOCK_ON: {
                    if (enabled) {
                        crosshair.GetComponent<Image>().enabled = false;
                        targeting_icon.GetComponent<Image>().enabled = true;
                        target = _target;
                        update = LockOnUpdate;
                    }
                }
                break;
            default:
                break;
        }
    }

    void InventoryEventCallback(object data, INVENTORY_EVENT e) {
        switch (e) {
            case INVENTORY_EVENT.EQUIP: {
                    // we won't do anything if we are already enabled
                    if (!enabled) {
                        crosshair.GetComponent<Image>().enabled = true;
                        crosshair.GetComponent<Image>().sprite = default_reticle;
                        update = DefaultUpdate;
                        enabled = true;
                    }
                }
                break;
            case INVENTORY_EVENT.UNEQUIP: {
                    crosshair.GetComponent<Image>().enabled = false;
                    targeting_icon.GetComponent<Image>().enabled = false;
                    enabled = false; // must set before
                    publisher.OnAimingEvent(null, AIMING_EVENT.AIM_OFF);
                }
                break;
            default:
                break;
        }
    }

    void PlayerHealthEventsCallback(float health, PLAYER_HEALTH_EVENT e) {
        switch (e) {
            case PLAYER_HEALTH_EVENT.DEAD: {
                    enabled = false; // it's extremely important that this stop sending out updates as soon as the player dies - it seems to lead to bad state later if not
                }
                break;
            default:
                break;
        }
    }
}
