﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputEvents;

public abstract class BasicEnemyAI : MonoBehaviour {
    public string desired_equipped;
    public GameObject desired_ammo;
    public float desired_ammo_amount; // this would mostly be set by the specific factory
                                      // but for enemies is usually just positive infinity
    public GameObject target = null;

    public string equip_body_part; // mainly so we can set through the editor

    protected GameObject equipped;
    protected BasicCharacter c;
    protected BasicWeapon w;

    protected GameObject m_event_manager;

    Weapon.WeaponFactory f;

    public void GlobalInputEventsCallback(object sender, INPUT_EVENT e) {
        switch (e) {
            case INPUT_EVENT.PAUSE: {
                    enabled = false;
                }
                break;
            case INPUT_EVENT.UNPAUSE: {
                    enabled = true;
                }
                break;
            default:
                break;
        }
    }

    private void OnDestroy() {
        if (m_event_manager != null) {
            m_event_manager.GetComponent<InputManager>().publisher.InputEvent -= GlobalInputEventsCallback;
        }
    }

    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    protected virtual void Init() {
        c = GetComponent<BasicCharacter>();
        f = new Weapon.WeaponFactory();

        if (desired_equipped != null) {
            if (equip_body_part != "") {
                equipped = f.SpawnEquipped(desired_equipped, transform, equip_body_part);
            } else {
                equipped = f.SpawnEquipped(desired_equipped, transform);
            }

            if (equipped == null) {
                Debug.LogError("We had a problem initializing the object. Exiting.");
                return;
            }


            c.SetAmmoType(desired_ammo);
            c.SetAmmoAmount(desired_ammo_amount);

            w = equipped.GetComponent<BasicWeapon>();
            w.SetCharacter(c);
            w.enabled = true;
        }

        if (target != null) {
            c.SetTarget(target);
            c.SetAimingDirection(target.transform.position);
        }

        m_event_manager = GameObject.Find("GlobalEventManager");
        m_event_manager.GetComponent<InputManager>().publisher.InputEvent += GlobalInputEventsCallback;
    }

    public void Setup(string _desired_equipped, GameObject _desired_ammo, float _desired_ammo_amount, ref GameObject _target) {
        desired_equipped = _desired_equipped;
        desired_ammo = _desired_ammo;
        desired_ammo_amount = _desired_ammo_amount;
        target = _target;
    }
}
