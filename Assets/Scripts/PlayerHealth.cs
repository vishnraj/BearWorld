using UnityEngine;

namespace PlayerHealthEvents {
    public enum PLAYER_HEALTH_EVENT { INIT, UPDATE, DEAD };

    public class PlayerHealthPublisher {
        public delegate void PlayerHealthEventHandler(float health, PLAYER_HEALTH_EVENT e);
        public event PlayerHealthEventHandler PlayerHealthEvent;

        public void OnPlayerHealthEvent(float health, PLAYER_HEALTH_EVENT e) {
            if (PlayerHealthEvent != null) {
                PlayerHealthEvent(health, e);
            } else {
                Debug.Log("NOOP");
            }
        }
    }
}

public class PlayerHealth : BasicHealth
{
    public PlayerHealthEvents.PlayerHealthPublisher publisher;
    bool start = false;

    // Use this for initialization
    void Start()
    {
       
    }

    private void Awake() {
        publisher = new PlayerHealthEvents.PlayerHealthPublisher();
    }

    // Update is called once per frame
    void Update()
    {
        if (!start) {
            publisher.OnPlayerHealthEvent(health, PlayerHealthEvents.PLAYER_HEALTH_EVENT.INIT);
            start = true;
        }

        if (health <= 0) {
            publisher.OnPlayerHealthEvent(health, PlayerHealthEvents.PLAYER_HEALTH_EVENT.DEAD);
            Destroy(gameObject);
        }
    }

    public override void Notify() {
        publisher.OnPlayerHealthEvent(health, PlayerHealthEvents.PLAYER_HEALTH_EVENT.UPDATE);
    }
}
