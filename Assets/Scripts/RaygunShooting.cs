using UnityEngine;
using System.Collections;

public class RaygunShooting : BasicWeapon
{
    public float fire_interval;
    bool currently_firing = false;
    IEnumerator firing;
    IEnumerator end_step = null;

    // Use this for initialization
    void Start()
    {

    }

    private void Awake() {
        type = Weapon.WEAPON_TYPE.RANGE;
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    IEnumerator Fire()
    {
        while (true)
        {
            if (c.GetAmmoAmount() != 0 && !currently_firing)
            {
                GameObject shot = Instantiate(c.GetAmmoType()) as GameObject;
                RaygunShot s = shot.GetComponent<RaygunShot>();
                s.SetOriginTag(c.tag);

                Vector3 pos = transform.position + transform.forward;
                shot.transform.position = pos;

                s.SetDirection(c.GetTarget(), range);
                c.DecrementAmmoAmount();

                s.enabled = true;

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

        Vector3 pos = transform.position;
        pos += transform.forward * .3f;
        pos += transform.up * .2f;
        transform.position = pos;
    }
}
