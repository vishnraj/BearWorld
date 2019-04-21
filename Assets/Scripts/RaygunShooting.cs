using UnityEngine;
using System.Collections;
using PlayerHealthEvents;

public class RaygunShooting : BasicWeapon
{
    public float fire_interval;
    public float frac_to_target;
    public float lock_on_shot_delay;

    bool currently_firing = false;

    bool can_fire = false;

    Vector3 target_position = Vector3.zero;

    IEnumerator firing = null;
    IEnumerator end_step = null;

    GameObject m_event_manager;

    void PlayerHealthEventsCallback(float health, PLAYER_HEALTH_EVENT e) {
        switch (e) {
            case PLAYER_HEALTH_EVENT.DEAD: {
                    enabled = false;
                    // so basically, like with enemy factory, anytime we have have coroutines running
                    // we need to end them when the player dies to avoid bad things happening
                    // this is a good reason to avoid them if possible, but in this case, the timer
                    // for firing needs to be indepedent of the game loop, so I'm using one
                    StopCoroutine(firing);
                    StopCoroutine(end_step);
                }
                break;
            default:
                break;
        }
    }

    // Use this for initialization
    void Start()
    {
        m_event_manager = GameObject.Find("GlobalEventManager");
        m_event_manager.GetComponent<ComponentEventManager>().health_publisher.PlayerHealthEvent -= PlayerHealthEventsCallback;
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
        while (can_fire)
        {
            if (c.GetAmmoAmount() != 0 && !currently_firing) {
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

            yield return new WaitForSeconds(0.0f); // we want to start as quickly as possible, but we need to yield to prevent a hang
        }
    }

    IEnumerator FinishFiring() {
        yield return new WaitForSeconds(fire_interval);
        currently_firing = false;

        end_step = null;
        yield break; // stop this coroutine
    }

    public override void Attack() {
        if (can_fire) return; // protect against just stopping and starting for no reason
                              // generally can only happen with AI
        can_fire = true;
        firing = Fire();
        StartCoroutine(firing);
    }

    public override void EndAttack() {
        can_fire = false;
        if (firing != null) {
            StopCoroutine(firing);
            firing = null;
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
