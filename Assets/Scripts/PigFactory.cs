using UnityEngine;
using System.Collections;

public class PigFactory : MonoBehaviour
{

    public GameObject desired_spawn_object;
    public string desired_weapon;
    public GameObject target;
    public GameObject enemy_master_obj;
    public int max_pigs;
    public float desired_ai_speed;
    public int spawn_interval;

    Transform door;

    IEnumerator spawner;

    // Use this for initialization
    void Start()
    {
        door = transform.Find("Portal-Door 1");

        spawner = Spawn();
        StartCoroutine(spawner);
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator Spawn() {
        while (true) {
            if (enemy_master_obj.transform.childCount >= max_pigs) {
                yield return new WaitForSeconds(spawn_interval);
            } else {
                Vector3 position = door.position + door.forward;
                GameObject pig = Instantiate(desired_spawn_object, position, transform.rotation, enemy_master_obj.transform) as GameObject;

                BasicPigAI ai = pig.GetComponent<BasicPigAI>();
                ai.desired_equipped = desired_weapon;
                ai.speed = desired_ai_speed;
                ai.target = target;
                ai.enabled = true;

                yield return new WaitForSeconds(spawn_interval);
            }
        }
    }
}
