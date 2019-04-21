using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerHealthEvents;
using InputEvents;

public class BasicEnemyFactory : MonoBehaviour {
    [SerializeField]
    GameObject m_stage;
    [SerializeField]
    float ray_radius;
    [SerializeField]
    float spawn_range; // distance from player to begin spawning
    [SerializeField]
    GameObject m_event_manager;

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

    void GlobalInputEventsCallback(object sender, INPUT_EVENT e) {
        switch (e) {
            case INPUT_EVENT.PAUSE: {
                    enabled = false;
                }
                break;
            case INPUT_EVENT.UNPAUSE: {
                    enabled = true;
                }
                break;
            default:
                break;
        }
    }

    void PlayerHealthEventsCallback(float health, PLAYER_HEALTH_EVENT e) {
        switch (e) {
            case PLAYER_HEALTH_EVENT.DEAD: {
                    enabled = false;
                    StopCoroutine(spawner);
                }
                break;
            default:
                break;
        }
    }

    private void OnDestroy() {
        m_event_manager.GetComponent<ComponentEventManager>().health_publisher.PlayerHealthEvent -= PlayerHealthEventsCallback;
        m_event_manager.GetComponent<InputManager>().publisher.InputEvent -= GlobalInputEventsCallback;
    }

    // Use this for initialization
    void Start() {
        enemies = new List<GameObject>();

        portal = transform.Find(portal_name);
        m_event_manager.GetComponent<ComponentEventManager>().health_publisher.PlayerHealthEvent += PlayerHealthEventsCallback;
        m_event_manager.GetComponent<InputManager>().publisher.InputEvent += GlobalInputEventsCallback;

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
            if (target != null) {
                Vector3 to_target = target.transform.position - portal.transform.position;
                Ray ray = new Ray(portal.transform.position, to_target);

                int layer = 1 << LayerMask.NameToLayer("Current_Realm"); // check if the factory is within sight of player
                if (Physics.SphereCast(ray, ray_radius, out hit, spawn_range, layer)) {
                    if (hit.collider.gameObject.tag != "Player") {
                        yield return new WaitForSeconds(spawn_interval);
                        continue;
                    }
                }
                else {
                    yield return new WaitForSeconds(spawn_interval);
                    continue;
                }

                if (enemies.Count >= max_enemies) {
                    yield return new WaitForSeconds(spawn_interval);
                }
                else {
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
}
