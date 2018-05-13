﻿using UnityEngine;

namespace InventoryEvents {
    public enum INVENTORY_EVENT { INIT, EQUIP, UNEQUIP, SET_AMMO, UNSET_AMMO, UPDATE_AMMO_AMOUNT };

    public class InventoryPublisher {
        public delegate void InventoryEventHandler(object data, INVENTORY_EVENT e);
        public event InventoryEventHandler InventoryEvent;

        public void OnInventoryEvent(object data, INVENTORY_EVENT e) {
            if (InventoryEvent != null) {
                InventoryEvent(data, e);
            } else {
                Debug.Log("NOOP");
            }
        }
    }
}

namespace TargetingEvents {
    public enum TARGETING_EVENT { INIT, FREE, CAN_LOCK, LOCK_ON };

    public class TargetingPublisher {
        public delegate void TargetingEventHandler(object sender, TARGETING_EVENT e);
        public event TargetingEventHandler TargetingEvent;

        public void OnTargetingEvent(TARGETING_EVENT e) {
            if (TargetingEvent != null) {
                TargetingEvent(this, e);
            } else {
                Debug.Log("NOOP");
            }
        }
    }
}

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

namespace EnemyHealthEvents {
    public enum ENEMY_HEALTH_EVENT { INIT, UPDATE, DESTROY };

    public class EnemyHealthData {
        public int id = -1;
        public float health = -1;
        public Vector3 pos;

        public EnemyHealthData(int _id, float _health, Vector3 _pos) {
            id = _id;
            health = _health;
            pos = _pos;
        }
    }

    public class EnemyHealthEventPublisher {
        public delegate void EnemyHealthEventHandler(EnemyHealthData data, ENEMY_HEALTH_EVENT e);
        public event EnemyHealthEventHandler EnemyHealthEvent;

        public void OnEnemyHealthEvent(EnemyHealthData data, ENEMY_HEALTH_EVENT e) {
            if (EnemyHealthEvent != null) {
                EnemyHealthEvent(data, e);
            } else {
                Debug.Log("NOOP");
            }
        }
    }
}

public class ComponentEventManager : MonoBehaviour {
    public InventoryEvents.InventoryPublisher inventory_publisher;
    public TargetingEvents.TargetingPublisher targeting_publisher;
    public PlayerHealthEvents.PlayerHealthPublisher health_publisher;
    public EnemyHealthEvents.EnemyHealthEventPublisher enemy_health_publisher;

    // Use this for initialization
    void Start () {
		
	}

    void Awake() {
        inventory_publisher = new InventoryEvents.InventoryPublisher();
        targeting_publisher = new TargetingEvents.TargetingPublisher();
        health_publisher = new PlayerHealthEvents.PlayerHealthPublisher();
        enemy_health_publisher = new EnemyHealthEvents.EnemyHealthEventPublisher();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
