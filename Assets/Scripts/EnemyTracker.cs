using System.Collections.Generic;
using UnityEngine;
using EnemyHealthEvents;

public class EnemyTracker : MonoBehaviour {
    Dictionary<int, GameObject> enemies;
    Dictionary<int, GameObject> m_rendered_enemies;

    List<GameObject> current_enemies;
    List<GameObject> current_rendered;

    [SerializeField]
    GameObject m_event_manager;
    EnemyHealthEventPublisher m_publisher;

    // Use this for initialization
    void Start () {
        
    }

    private void Awake() {
        enemies = new Dictionary<int, GameObject>();
        m_rendered_enemies = new Dictionary<int, GameObject>();

        m_publisher = m_event_manager.GetComponent<ComponentEventManager>().enemy_health_publisher;
    }

    // Update is called once per frame
    void Update () {
        
    }
    
    public void RemoveEnemy(GameObject enemy) {
        enemies.Remove(enemy.GetInstanceID());
        m_rendered_enemies.Remove(enemy.GetInstanceID()); // this should get removed when the enemies are destroyed
    }

    public void AddEnemy(GameObject enemy) {
        enemies[enemy.GetInstanceID()] = enemy;
    }

    // because this is called in LateUpdate
    // we don't need to worry about the ordering
    // issues that we generally need to in regards
    // to when m_rendered_enemies is checked
    public void UpdateRenderedEnemies() {
        foreach (KeyValuePair<int, GameObject> e in enemies) {
            if (e.Value.layer == LayerMask.NameToLayer("Enemy_Layer") && !FindRenderedEnemy(e.Key)) {
                m_rendered_enemies[e.Key] = e.Value;
                m_publisher.OnEnemyHealthEvent(new EnemyHealthData(e.Key, e.Value.GetComponent<EnemyHealth>().health,
                        transform.position), ENEMY_HEALTH_EVENT.INIT);
            } else if (e.Value.layer != LayerMask.NameToLayer("Enemy_Layer") && FindRenderedEnemy(e.Key)) {
                m_rendered_enemies.Remove(e.Key);
                m_publisher.OnEnemyHealthEvent(new EnemyHealthData(e.Key, e.Value.GetComponent<EnemyHealth>().health,
                    e.Value.transform.position), ENEMY_HEALTH_EVENT.DESTROY);
            }
        }
    }

    // one of the few places where we need to check null, given order of updates, we cannot always expect the call when enemy is valid
    public bool FindEnemy(int key) {
        if (enemies.ContainsKey(key)) {
            return true;
        }

        return false;
    }

    // one of the few places where we need to check null, given order of updates, we cannot always expect the call when enemy is valid
    public bool FindRenderedEnemy(int key) {
        if (m_rendered_enemies.ContainsKey(key)) {
            return true;
        }

        return false;
    }

    public List<GameObject> GetAllEnemies() {
        current_enemies = new List<GameObject>(enemies.Values);
        return current_enemies;
    }

    public List<GameObject>GetOnlyRenderedEnemies() {
        current_rendered = new List<GameObject>(m_rendered_enemies.Values);
        return current_rendered;
    }
}
