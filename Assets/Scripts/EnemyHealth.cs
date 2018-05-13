using UnityEngine;
using System.Collections.Generic;
using TargetingEvents;
using EnemyHealthEvents;

public class EnemyHealth : BasicHealth
{
    public List<string> random_drop_list;
    bool start = false;
    public bool player_locked_on = false;

    EnemyHealthEventPublisher publisher;
    GameObject event_manager;
    GameObject enemies;

    TargetingPublisher targeting_publisher;

    // Use this for initialization
    void Start()
    {
        // enemies are prefabs, so prior to spawning we cannot know
        // about other objects in the scene - we grab revelant ones here
        event_manager = GameObject.Find("GlobalEventManager");
        enemies = GameObject.Find("Enemies");

        enemies.GetComponent<EnemyTracker>().AddEnemy(gameObject);

        publisher = event_manager.GetComponent<ComponentEventManager>().enemy_health_publisher;

        targeting_publisher = event_manager.GetComponent<ComponentEventManager>().targeting_publisher;
        targeting_publisher.TargetingEvent += TargetingEventCallback;
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0) {
            if (GetComponent<EnemyExplosion>() != null) {
                // from this point (unless we have reform script)
                // we will delagate death of the enemy to the
                // enemy explosion script - but we don't destroy
                // this object - CompositeEnemy will take care of this
                if (player_locked_on && start) {
                    publisher.OnEnemyHealthEvent(new EnemyHealthData(GetInstanceID(), health, transform.position), ENEMY_HEALTH_EVENT.DESTROY);
                }
                
                enemies.GetComponent<EnemyTracker>().RemoveEnemy(gameObject); // we don't want this to be tracked (unless reformed)
                GetComponent<EnemyExplosion>().Explode();
                enabled = false;
                return;
            }

            if (player_locked_on && start) {
                publisher.OnEnemyHealthEvent(new EnemyHealthData(GetInstanceID(), health, transform.position), ENEMY_HEALTH_EVENT.DESTROY);
            }

            enemies.GetComponent<EnemyTracker>().RemoveEnemy(gameObject);

            if (random_drop_list.Count != 0) {
                Items.DropFactory f = new Items.DropFactory();
                f.Drop(random_drop_list, transform.position);
            }

            targeting_publisher.TargetingEvent -= TargetingEventCallback;
            Destroy(gameObject);
            return;
        }

        if (player_locked_on) {
            // mainly for explosion objects, because they may inherit targeting
            // state from parent, we need to create UI element the first time through
            if (!start) {
                publisher.OnEnemyHealthEvent(new EnemyHealthData(GetInstanceID(), health,
                    transform.position), ENEMY_HEALTH_EVENT.INIT);
                start = true;
            }

            // This is a somewhat unforunate side effect - only the enemy actually knows where it is, so only it
            // can communicate this with the UI - thus we need to do this during each update if player is locked on
            publisher.OnEnemyHealthEvent(new EnemyHealthData(GetInstanceID(), health, transform.position), ENEMY_HEALTH_EVENT.UPDATE);
        }
    }

    public override void Notify() {
        if (player_locked_on && start) {
            publisher.OnEnemyHealthEvent(new EnemyHealthData(GetInstanceID(), health, transform.position), ENEMY_HEALTH_EVENT.UPDATE);
        }
    }

    void TargetingEventCallback(object sender, TARGETING_EVENT e) {
        switch (e) {
            case TARGETING_EVENT.FREE: {
                    if (player_locked_on) {
                        publisher.OnEnemyHealthEvent(new EnemyHealthData(GetInstanceID(), health, transform.position), ENEMY_HEALTH_EVENT.DESTROY);
                        player_locked_on = false;
                    }
                }
                break;
            case TARGETING_EVENT.LOCK_ON: {
                    publisher.OnEnemyHealthEvent(new EnemyHealthData(GetInstanceID(), health, transform.position), ENEMY_HEALTH_EVENT.INIT);
                    player_locked_on = true;
                    start = true; // so we don't create ui_element again
                }
                break;
            default:
                break;
        }
    }    
}
