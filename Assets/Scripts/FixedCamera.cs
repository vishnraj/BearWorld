using UnityEngine;
using System.Collections;
using Utility;

public class FixedCamera : MonoBehaviour
{
    public GameObject player;
    public bool targeting = false;
    public float target_rotation_x;

    Vector3 behind_player = Vector3.zero;
    float distance = 20f;
    float y_diff = 2.0f;
    float targeting_y_diff = 20.0f;

    Rotation rt;
    ThirdPersonTargetingSystem tps;

    // Use this for initialization
    void Start()
    {
        // Ordered intialization
        tps = player.GetComponent<ThirdPersonTargetingSystem>();
    }

    private void Awake() {
        rt = new Rotation();
    }

    void OnEnable()
    {
        RotateCameraToPlayerForward();

        transform.position = new Vector3(player.transform.position.x + behind_player.x * distance, player.transform.position.y + y_diff, player.transform.position.z + behind_player.z * distance);
    }

    void LateUpdate()
    {
        Weapon.WEAPON_TYPE type = player.GetComponent<ItemSystem>().equipped.GetComponent<BasicWeapon>().GetWeaponType();
        switch (type) {
            case Weapon.WEAPON_TYPE.MELEE:
                UpdateNonAimingTargetingCamera();
                break;
            case Weapon.WEAPON_TYPE.RANGE:
                UpdateAimingTargetingCamera();
                break;
        }
    }

    void RotateCameraToPlayerForward()
    {
        behind_player = -player.transform.forward;

        transform.rotation = new Quaternion(0, 0, 0, 0);

        float player_facing_angle = rt.CalculateZXRotation(player.transform.forward);
        transform.Rotate(Vector3.up, player_facing_angle);
    }

    void UpdateAimingTargetingCamera() {
        if (tps.locked_on) {
            RotateCameraToPlayerForward();
        }

        transform.position = new Vector3(player.transform.position.x + behind_player.x * distance, player.transform.position.y + y_diff, player.transform.position.z + behind_player.z * distance);
    }

    void UpdateNonAimingTargetingCamera() {
        if (tps.locked_on) {
            RotateCameraToPlayerForward();
            transform.position = new Vector3(player.transform.position.x + behind_player.x * distance, player.transform.position.y + targeting_y_diff, player.transform.position.z + behind_player.z * distance);
            transform.Rotate(target_rotation_x, 0, 0);
            targeting = true;
        } else {
            if (targeting) {
                transform.Rotate(-target_rotation_x, 0, 0);
                targeting = false;
            }
            transform.position = new Vector3(player.transform.position.x + behind_player.x * distance, player.transform.position.y + y_diff, player.transform.position.z + behind_player.z * distance);
        }
    }
}
