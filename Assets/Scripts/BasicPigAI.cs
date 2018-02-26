﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicPigAI : MonoBehaviour {
    public string desired_equipped;
    public GameObject desired_ammo;
    public float desired_ammo_amount;

    public GameObject equipped;
    public GameObject target = null;
    public float speed;

    BasicCharacter c;
    BasicWeapon w;
    Weapon.WeaponFactory f;

    // Use this for initialization
    void Start () {
        c = GetComponent<BasicCharacter>();
        f = new Weapon.WeaponFactory();

        if (desired_equipped != null) {
            equipped = f.SpawnEquipped(desired_equipped, transform, "RightArm");

            c.SetAmmoType(desired_ammo);
            c.SetAmmoAmount(desired_ammo_amount);

            w = equipped.GetComponent<BasicWeapon>();
            w.SetCharacter(c);
            w.enabled = true;
        }

        if (target != null) {
            c.SetTarget(target.transform.position);
            w.Attack();
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (target != null) {
            c.SetTarget(target.transform.position);
            Vector3 to_target = target.transform.position - transform.position;
            Vector3 direction = Vector3.RotateTowards(transform.forward, to_target, Mathf.PI, 0);
            transform.rotation = Quaternion.LookRotation(direction);

            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, step);
        } else {
            w.EndAttack();
        }
    }
}
