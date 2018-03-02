﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Utility;

// Used to deliver target(s) to other systems
public class AimingSystem : MonoBehaviour
{
    public GameObject crosshair;
    public Camera cam;
    public GameObject player;
    public float ray_radius;

    // This is hard coded, it just guarantees
    // that the part of the ray used to set direction
    // is in front of the player
    float in_front_of = 70.0f;

    public Sprite default_reticle;
    public Sprite target_reticle;

    ThirdPersonTargetingSystem tps;
    BasicCharacter c;

    // Use this for initialization
    void Start()
    {
        tps = player.GetComponent<ThirdPersonTargetingSystem>();
        c = player.GetComponent<BasicCharacter>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray ray;
        // the ray needs to be from wherever is in the current view to object
        ray = cam.ScreenPointToRay(crosshair.GetComponent<RectTransform>().position);
        tps.direction = ray.GetPoint(in_front_of);

        if (player != null && Physics.SphereCast(ray, ray_radius, out hit) && hit.collider.gameObject.GetComponent<EnemyHealth>() != null)
        {
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
        }
        else
        {
            SetDefaults();
        }
    }

    void OnEnable()
    {
        if (player != null) {
            tps = player.GetComponent<ThirdPersonTargetingSystem>();
            crosshair.GetComponent<Image>().enabled = true;
            SetDefaults();
        }
    }

    private void OnDisable() {
        crosshair.GetComponent<Image>().enabled = false;
    }

    void SetDefaults()
    {
        crosshair.GetComponent<Image>().sprite = default_reticle;
        tps.target = null;
    }
}
