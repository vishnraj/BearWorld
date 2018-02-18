using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PigFactory : MonoBehaviour
{

    public GameObject desired_spawn_object;
    public string desired_weapon;
    public GameObject desired_ammo;
    public GameObject target;
    public GameObject enemy_master_obj;
    public int max_pigs;
    public float desired_ai_speed;
    public int spawn_interval;

    List<GameObject> pigs;
    Transform door;

    IEnumerator spawner;

    // Use this for initialization
    void Start()
    {
        pigs = new List<GameObject>();

        door = transform.Find("Portal-Door");

        spawner = Spawn();
        //StartCoroutine(spawner);
    }

    // Update is called once per frame
    void Update()
    {
        pigs.RemoveAll(item => item == null);
    }

    IEnumerator Spawn() {
        while (true) {
            if (pigs.Count >= max_pigs) {
                yield return new WaitForSeconds(spawn_interval);
            } else {
                Vector3 position = door.position + door.forward;
                GameObject pig = Instantiate(desired_spawn_object, position, transform.rotation) as GameObject;
                enemy_master_obj.GetComponent<EnemyTracker>().AddEnemy(pig);
                pigs.Add(pig);

                BasicPigAI ai = pig.GetComponent<BasicPigAI>();
                ai.desired_equipped = desired_weapon;
                ai.desired_ammo = desired_ammo;
                ai.desired_ammo_amount = Mathf.Infinity;
                ai.speed = desired_ai_speed;
                ai.target = target;
                ai.enabled = true;

                yield return new WaitForSeconds(spawn_interval);
            }
        }
    }
}
