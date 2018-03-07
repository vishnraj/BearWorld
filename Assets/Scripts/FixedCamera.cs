﻿using UnityEngine;
using System.Collections;
using Utility;

public class FixedCamera : MonoBehaviour
{
    public GameObject player;
    public bool targeting = false;
    public float target_rotation_x;
    public float targeting_y_diff;

    Vector3 behind_player = Vector3.zero;
    float distance = 20f;
    float y_diff = 2.0f;
    
    Rotation rt;
    ThirdPersonTargetingSystem tps;
    PlayerAttackController pac;

    // Use this for initialization
    void Start()
    {
        // Ordered intialization
        tps = player.GetComponent<ThirdPersonTargetingSystem>();
        pac = player.GetComponent<PlayerAttackController>();
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
        if (player != null && pac.enabled) {
            Weapon.WEAPON_TYPE type = pac.weapon.GetWeaponType();
            switch (type) {
                case Weapon.WEAPON_TYPE.MELEE:
                    UpdateNonRangeTargetingCamera();
                    break;
                case Weapon.WEAPON_TYPE.RANGE:
                    UpdateRangeTargetingCamera();
                    break;
                default:
                    UpdateNonRangeTargetingCamera();
                    break;
            }
        }
    }

    void RotateCameraToPlayerForward()
    {
        behind_player = -player.transform.forward;

        transform.rotation = new Quaternion(0, 0, 0, 0);

        float player_facing_angle = rt.CalculateZXRotation(player.transform.forward);
        transform.Rotate(Vector3.up, player_facing_angle);
    }

    void UpdateRangeTargetingCamera() {
        if (tps.locked_on) {
            RotateCameraToPlayerForward();
        }

        transform.position = new Vector3(player.transform.position.x + behind_player.x * distance, player.transform.position.y + y_diff, player.transform.position.z + behind_player.z * distance);
    }

    void UpdateNonRangeTargetingCamera() {
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
