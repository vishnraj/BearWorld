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

            // Just to be safe, so we don't get in any weird states before we
            // exit, I am disabling everything here (the health event above is
            // intended for any systems separate to the player, but those attached
            // to the player should just get disabled here)
            MonoBehaviour[] components = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour c in components) {
                c.enabled = false;
            }

            components = GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour c in components) {
                c.enabled = false;
            }

            Destroy(gameObject);

        }
    }

    public override void Notify() {
        publisher.OnPlayerHealthEvent(health, PLAYER_HEALTH_EVENT.UPDATE);
    }
}
