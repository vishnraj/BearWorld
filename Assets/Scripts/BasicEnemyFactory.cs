using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyFactory : MonoBehaviour {
    public GameObject desired_spawn_object;
    public string desired_weapon;
    public GameObject desired_ammo;
    public GameObject target;
    public int max_enemies;
    public int spawn_interval;
    public string portal_name;

    List<GameObject> enemies;
    Transform portal = null;

    IEnumerator spawner;

    // Use this for initialization
    void Start() {
        enemies = new List<GameObject>();

        portal = transform.Find(portal_name);

        if (portal == null) {
            Debug.LogError("We require a portal to spawn enemies. Exiting.");
            return;
        }

        spawner = Spawn();
        StartCoroutine(spawner);
    }

    // Update is called once per frame
    void Update() {
        enemies.RemoveAll(item => item == null);
    }

    IEnumerator Spawn() {
        while (true) {
            if (enemies.Count >= max_enemies) {
                yield return new WaitForSeconds(spawn_interval);
            } else {
                Vector3 portal_position = portal.position + portal.forward;
                GameObject enemy = Instantiate(desired_spawn_object, portal_position, transform.rotation) as GameObject;
                enemies.Add(enemy);

                BasicEnemyAI ai = enemy.GetComponent<BasicEnemyAI>();
                ai.Setup(desired_weapon, desired_ammo, Mathf.Infinity, ref target);
                ai.enabled = true;

                yield return new WaitForSeconds(spawn_interval);
            }
        }
    }
}
