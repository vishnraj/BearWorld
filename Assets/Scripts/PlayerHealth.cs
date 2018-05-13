using UnityEngine;
using PlayerHealthEvents;

public class PlayerHealth : BasicHealth
{
    public GameObject event_manager;

    PlayerHealthPublisher publisher;
    bool start = false;

    // Use this for initialization
    void Start()
    {
        publisher = event_manager.GetComponent<ComponentEventManager>().health_publisher;
    }

    private void Awake() {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!start) {
            publisher.OnPlayerHealthEvent(health, PLAYER_HEALTH_EVENT.INIT);
            start = true;
        }

        if (health <= 0) {
            publisher.OnPlayerHealthEvent(health, PLAYER_HEALTH_EVENT.DEAD);
            Destroy(gameObject);
        }
    }

    public override void Notify() {
        publisher.OnPlayerHealthEvent(health, PLAYER_HEALTH_EVENT.UPDATE);
    }
}
