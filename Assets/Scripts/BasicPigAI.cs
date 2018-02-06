using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicPigAI : MonoBehaviour {
    public GameObject desired_weapon;
    public GameObject equipped;
    public GameObject player;
    public GameObject desired_ammo;
    public int desired_ammo_amount;
    public float speed;

    BasicCharacter c;
    BasicWeapon w;

    // Use this for initialization
    void Start () {
        c = GetComponent<BasicCharacter>();
        c.SetAmmoType(desired_ammo);
        c.SetAmmoAmount(desired_ammo_amount);

        if (desired_weapon != null) {
            GameObject raygun = Instantiate(desired_weapon) as GameObject;
            Transform right_arm = transform.Find("RightArm");
            Vector3 pos = right_arm.position;
            pos.x += 0f;
            pos.y += .2f;
            pos.z += -.4f;
            raygun.transform.position = pos;
            raygun.transform.parent = right_arm;
            Vector3 direction = Vector3.RotateTowards(raygun.transform.forward, transform.forward, Mathf.PI, 0);
            raygun.transform.rotation = Quaternion.LookRotation(direction);

            w = raygun.GetComponent<BasicWeapon>();
            w.SetCharacter(c);
            equipped = raygun;
            w.enabled = true;
        }

        if (player != null) {
            c.SetTarget(player.transform.position);
            w.Attack();
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (player != null) {
            c.SetTarget(player.transform.position);
            Vector3 to_player = player.transform.position - transform.position;
            Vector3 direction = Vector3.RotateTowards(transform.forward, to_player, Mathf.PI, 0);
            transform.rotation = Quaternion.LookRotation(direction);
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, step);
        } else {
            w.EndAttack();
        }
    }
}
