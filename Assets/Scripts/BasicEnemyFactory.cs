using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyFactory : MonoBehaviour {
    [SerializeField]
    GameObject m_stage;
    [SerializeField]
    float ray_radius;
    [SerializeField]
    float spawn_range; // distance from player to begin spawning

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
            RaycastHit hit;
            Vector3 to_target = target.transform.position - portal.transform.position;
            Ray ray = new Ray(portal.transform.position, to_target);

            int layer = 1 << LayerMask.NameToLayer("Current_Realm"); // check if the factory is within sight of player
            if (Physics.SphereCast(ray, ray_radius, out hit, spawn_range, layer)) {
                if (hit.collider.gameObject.tag != "Player") {
                    yield return new WaitForSeconds(spawn_interval);
                    continue;
                }
            } else {
                yield return new WaitForSeconds(spawn_interval);
                continue;
            }

            if (enemies.Count >= max_enemies) {
                yield return new WaitForSeconds(spawn_interval);
            } else {
                Vector3 portal_position = portal.position + portal.forward;
                GameObject enemy = Instantiate(desired_spawn_object, portal_position, transform.rotation) as GameObject;
                enemies.Add(enemy);
                enemy.transform.SetParent(m_stage.transform);

                // this way we can spawn dummies when needed
                if (target) {
                    BasicEnemyAI ai = enemy.GetComponent<BasicEnemyAI>();
                    ai.Setup(desired_weapon, desired_ammo, Mathf.Infinity, ref target);
                    ai.enabled = true;
                }

                yield return new WaitForSeconds(spawn_interval);
            }
        }
    }
}
