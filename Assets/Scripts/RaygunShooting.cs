using UnityEngine;
using System.Collections;
using Utility;

public class RaygunShooting : MonoBehaviour
{
    public GameObject ray_blast;
    float fire_interval = 1.0f;
    bool currently_firing = false;
    IEnumerator firing;

    GameObject player;
    ItemSystem player_item_system;
    ThirdPersonTargetingSystem tps;

    // Use this for initialization
    void Start()
    {
        if (transform.parent != null && transform.parent.transform.parent != null)
        {
            player = transform.parent.transform.parent.gameObject;
            player_item_system = player.GetComponent<ItemSystem>();
            tps = player.GetComponent<ThirdPersonTargetingSystem>();
        }

        if (player != null)
        {
            if (player_item_system.current_ammo_amount != 0)
            {
                ray_blast = player_item_system.current_ammo_type;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("RightTriggerAxis") > 0 && !currently_firing)
        {
            firing = Fire();
            StartCoroutine(firing);
            currently_firing = true;
        }
        else if (Input.GetAxis("RightTriggerAxis") == 0 && currently_firing)
        {
            StopCoroutine(firing);
            currently_firing = false;
        }
    }

    IEnumerator Fire()
    {
        while (true)
        {
            if (player_item_system.current_ammo_amount != 0)
            {
                GameObject shot = Instantiate(ray_blast) as GameObject;
                Vector3 pos = transform.position;
                shot.transform.position = pos;

                if (tps.locked_on || tps.target != null)
                {
                    shot.GetComponent<RaygunShot>().SetDirection(tps.target.transform.position);
                }
                else
                {
                    shot.GetComponent<RaygunShot>().SetDirection(tps.direction);
                }

                player_item_system.current_ammo_amount -= 1;
                player_item_system.UpdateAmmoAmountOnGUI();
            }

            yield return new WaitForSeconds(fire_interval);
        }
    }
}
