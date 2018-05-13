using UnityEngine;
using UnityEngine.UI;
using TargetingEvents;
using InventoryEvents;

// Used to deliver target(s) to other systems
public class AimingSystem : MonoBehaviour
{
    public GameObject crosshair;
    public Camera cam;
    public GameObject player;
    public float ray_radius;
    public float desired_scale = .4f;
    public GameObject event_manager;

    // This is hard coded, it just guarantees
    // that the part of the ray used to set direction
    // is in front of the player
    float in_front_of = 70.0f;

    public Sprite default_reticle;
    public Sprite target_reticle;

    GameObject targeting_icon;
    ThirdPersonTargetingSystem tps;

    public delegate void DoUpdate();
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

        tps = player.GetComponent<ThirdPersonTargetingSystem>();

        event_manager.GetComponent<ComponentEventManager>().targeting_publisher.TargetingEvent += TargetingEventCallback;
        event_manager.GetComponent<ComponentEventManager>().inventory_publisher.InventoryEvent += InventoryEventCallback;

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
        Vector2 target_screen_point = cam.WorldToViewportPoint(tps.target.transform.position);
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
        tps.direction = ray.GetPoint(in_front_of); // this may better communicated via event

        int enemy_layer = 1 << LayerMask.NameToLayer("Enemy_Layer");
        if (player != null && Physics.SphereCast(ray, ray_radius, out hit, Mathf.Infinity, enemy_layer) && hit.collider.gameObject.GetComponent<EnemyHealth>() != null) {
            Vector3 direction = player.transform.position - hit.transform.position;

            if (direction.magnitude <= tps.current_weapon_range) {
                if (!tps.locked_on) {
                    // changing state of objects should rely on message passing
                    tps.target = hit.collider.gameObject;
                }

                crosshair.GetComponent<Image>().sprite = target_reticle;
            } else {
                SetDefaults();
            }
        } else {
            SetDefaults();
        }
    }

    void TargetingEventCallback(object sender, TARGETING_EVENT e) {
        switch (e) {
            case TARGETING_EVENT.CAN_LOCK:
            case TARGETING_EVENT.FREE: {
                    // as a fallback, unless this is enabled,
                    // don't do anything - this is because there is a function
                    // in third person targeting that may still send this out
                    // and in general every componenent must handle it
                    // the way that is best for them
                    if (enabled) {
                        crosshair.GetComponent<Image>().enabled = true;
                        targeting_icon.GetComponent<Image>().enabled = false;
                        update = DefaultUpdate;
                    }
                }
                break;
            case TARGETING_EVENT.LOCK_ON: {
                    // as a fallback, unless this is enabled,
                    // don't do anything
                    if (enabled) {
                        crosshair.GetComponent<Image>().enabled = false;
                        targeting_icon.GetComponent<Image>().enabled = true;
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
                        SetDefaults();
                        update = DefaultUpdate;
                        enabled = true;
                    }
                }
                break;
            case INVENTORY_EVENT.UNEQUIP: {
                    crosshair.GetComponent<Image>().enabled = false;
                    targeting_icon.GetComponent<Image>().enabled = false;
                    tps.target = null; // possibly communicate via an event
                    enabled = false;
                }
                break;
            default:
                break;
        }
    }

    void SetDefaults()
    {
        crosshair.GetComponent<Image>().sprite = default_reticle;
        tps.target = null;
    }
}
