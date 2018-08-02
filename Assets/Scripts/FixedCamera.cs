using UnityEngine;
using Utility;
using InventoryEvents;
using TargetingEvents;
using Weapon;

public class FixedCamera : MonoBehaviour
{
    public GameObject player;

    public bool targeting = false;
    public float target_rotation_x;

    public float melee_y_diff;
    public float default_y_diff;

    Vector3 behind_player = Vector3.zero;
    float distance = 20f;

    GameObject target;
    Rotation rt;

    delegate void DoUpdate();
    DoUpdate targeting_update = null;

    public void InventoryEventCallback(object data, INVENTORY_EVENT e) {
        switch(e) {
            case INVENTORY_EVENT.EQUIP: {
                    GameObject equipped = (GameObject)data;
                    if (equipped == null) {
                        Debug.Log("Event not for us.");
                    }

                    WEAPON_TYPE w = equipped.GetComponent<BasicWeapon>().GetWeaponType();

                    switch(w) {
                        case WEAPON_TYPE.RANGE: {
                                targeting_update = RangeUpdate;
                            }
                            break;
                        case WEAPON_TYPE.MELEE: {
                                targeting_update = MeeleUpdate;
                            }
                            break;
                        default: {
                                targeting_update = DefaultUpdate;
                                Debug.Log("How did we get here?");
                            }
                            break;
                    }
                }
                break;
            default:
                break;
        }
    }

    public void TargetingEventCallback(GameObject _target, TARGETING_EVENT e) {
        switch(e) {
            case TARGETING_EVENT.LOCK_ON: {
                    target = _target;
                    targeting = true;
                }
                break;
            case TARGETING_EVENT.CAN_LOCK:
            case TARGETING_EVENT.FREE: {
                    targeting = false;
                }
                break;
            default:
                break;
        }
    }
 
    private void Awake() {
        rt = new Rotation();
    }

    void OnEnable() {
        RotateCameraToPlayerForward(); // required at start, targeting will update after if needed
    }

    void LateUpdate()
    {
        if (player != null) {
            if (targeting) {
                targeting_update();
            } else {
                DefaultUpdate();
            }
        }
    }

    void DefaultUpdate() {
        RotateCameraToPlayerForward();
        if (transform.rotation.x != 0) {
            transform.Rotate(-target_rotation_x, 0, 0);
        }

        UpdatePos(default_y_diff);
    }

    void RangeUpdate() {
        RotateCameraToPlayerForward();
        if (transform.rotation.x != 0) {
            transform.Rotate(-target_rotation_x, 0, 0);
        }

        //behind_player = -player.transform.forward;

        UpdatePos(default_y_diff);

        //RotateToTarget();
    }

    void MeeleUpdate() {
        RotateCameraToPlayerForward();
        if (transform.rotation.x == 0) {
            transform.Rotate(target_rotation_x, 0, 0);
        }

        //behind_player = -player.transform.forward;

        UpdatePos(melee_y_diff);

        //RotateToTarget();
    }

    void UpdatePos(float y_diff) {
        transform.position = new Vector3(player.transform.position.x + behind_player.x * distance, player.transform.position.y + y_diff, player.transform.position.z + behind_player.z * distance);
    }

    void RotateToTarget() {
        Vector3 to_target = target.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(to_target);
    }

    void RotateCameraToPlayerForward() {
        behind_player = -player.transform.forward;

        transform.rotation = new Quaternion(0, 0, 0, 0);

        float player_facing_angle = rt.CalculateZXRotation(player.transform.forward);
        transform.Rotate(Vector3.up, player_facing_angle);
    }
}
