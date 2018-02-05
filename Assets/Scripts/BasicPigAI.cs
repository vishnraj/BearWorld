using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicPigAI : MonoBehaviour {
    public GameObject desired_weapon;
    public GameObject equipped;
    public GameObject player;
    public GameObject desired_ammo;
    public int desired_ammo_amount;
    public int attacking_interval;

    BasicCharacter c;
    BasicWeapon w;

    IEnumerator attacking;

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
            raygun.transform.Rotate(0, -180, 0);

            w = raygun.GetComponent<BasicWeapon>();
            w.SetCharacter(c);
            equipped = raygun;
            w.enabled = true;
        }

        //attacking = Attack();
        c.SetTarget(player.transform.position);
        w.Attack();
        //StartCoroutine(attacking);
    }
	
	// Update is called once per frame
	void Update () {
        c.SetTarget(player.transform.position);
        //w.Attack();
    }

    IEnumerator Attack() {
        w.Attack();
        w.EndAttack();

        yield return new WaitForSeconds(attacking_interval);
    }
}
