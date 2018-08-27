using UnityEngine;
using System.Collections;

public class RaygunShooting : BasicWeapon
{
    public float fire_interval;
    public float frac_to_target;
    public float lock_on_shot_delay;

    bool currently_firing = false;

    Vector3 target_position = Vector3.zero;

    IEnumerator firing;
    IEnumerator end_step = null;

    // Use this for initialization
    void Start()
    {

    }

    private void Awake() {
        type = Weapon.WEAPON_TYPE.RANGE;
        weapon_name = Weapon.WeaponNames.RAYGUN;
    }

    // Update is called once per frame
    void Update()
    {
        if (c is Player) {
            Player p = (Player)c;
            if (p.InLockOn()) {
                target_position = c.GetTarget().transform.position;
            }
        }
    }


    void DefaultShooting() {
        GameObject shot = Instantiate(c.GetAmmoType()) as GameObject;
        RaygunShot s = shot.GetComponent<RaygunShot>();
        s.SetOriginTag(c.tag);

        Vector3 pos = transform.position;
        shot.transform.position = pos;

        s.SetDirection(c.GetAimingDirection(), range);
        c.DecrementAmmoAmount();

        s.enabled = true;
    }

    void PlayerShooting() {
        Player p = (Player)c;

        if (p.InLockOn()) {
            RaycastHit hit;
            Vector3 to_target = target_position - transform.position;

            // The two layers that are basically important for the game, regarding what can be hit and
            // what can't be (enemies, players and other objects in the scene that are collidable)
            int layers = (1 << LayerMask.NameToLayer("Enemy_Layer")) | (1 << LayerMask.NameToLayer("Current_Realm"));

            if (Physics.Raycast(transform.position, to_target, out hit, range, layers) && hit.collider.gameObject.GetComponent<BasicHealth>() != null) {
                GameObject hit_obj = hit.collider.gameObject;

                // effectively no friendly fire
                if (c.tag != hit_obj.tag) {
                    GameObject shot = Instantiate(c.GetAmmoType()) as GameObject;
                    shot.GetComponent<SphereCollider>().enabled = false; // we don't want a trigger
                    shot.transform.position = transform.position + to_target * frac_to_target;

                    RaygunShot s = shot.GetComponent<RaygunShot>();
                    hit_obj.GetComponent<BasicHealth>().Damage(s.damage);
                    c.DecrementAmmoAmount();

                    Destroy(shot, lock_on_shot_delay);
                }
            }
            else {
                DefaultShooting(); // simulate an actual gun fire; even if it can't hit, it still would fly in that general direction
            }
        }
        else {
            DefaultShooting(); // simulate an actual gun fire; even if it can't hit, it still would fly in that general direction
        }
    }

    IEnumerator Fire()
    {
        while (true)
        {
            if (c.GetAmmoAmount() != 0 && !currently_firing)
            {
                if (c is Player) {
                    PlayerShooting();           
                } else {
                    DefaultShooting();
                }

                currently_firing = true;
            }

            if (currently_firing && end_step == null) {
                end_step = FinishFiring();
                yield return StartCoroutine(end_step);
            }

            yield return new WaitForSeconds(0.0f);
        }
    }

    IEnumerator FinishFiring() {
        yield return new WaitForSeconds(fire_interval);
        currently_firing = false;

        end_step = null;
        yield return new WaitForSeconds(0.0f);
    }

    public override void Attack() {
        firing = Fire();
        StartCoroutine(firing);
    }

    public override void EndAttack() {
        if (firing != null) {
            StopCoroutine(firing);
        }
    }

    public override void Init() {
        base.Init();

        type = Weapon.WEAPON_TYPE.RANGE;
        weapon_name = Weapon.WeaponNames.RAYGUN;

        Vector3 pos = transform.position;
        pos += transform.forward * .3f;
        pos += transform.up * .2f;
        transform.position = pos;
    }
}
