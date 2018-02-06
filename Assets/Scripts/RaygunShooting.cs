using UnityEngine;
using System.Collections;

public class RaygunShooting : BasicWeapon
{
    float fire_interval = 1.0f;
    bool currently_firing = false;
    IEnumerator firing;

    GameObject character;

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
            if (c.GetAmmoAmount() != 0)
            {
                GameObject shot = Instantiate(c.GetAmmoType()) as GameObject;
                RaygunShot s = shot.GetComponent<RaygunShot>();
                s.SetOriginTag(c.tag);

                Vector3 pos = transform.position + transform.forward;
                shot.transform.position = pos;

                s.SetDirection(c.GetTarget());
                c.DecrementAmmoAmount();

                s.enabled = true;
            }

            yield return new WaitForSeconds(fire_interval);
        }
    }

    public override void Attack() {
        firing = Fire();
        StartCoroutine(firing);
        currently_firing = true;
    }

    public override void EndAttack() {
        StopCoroutine(firing);
        currently_firing = false;
    }
}
