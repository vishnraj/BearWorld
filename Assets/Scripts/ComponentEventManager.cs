using UnityEngine;

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
        public delegate void TargetingEventHandler(GameObject target, TARGETING_EVENT e);
        public event TargetingEventHandler TargetingEvent;

        public void OnTargetingEvent(GameObject target, TARGETING_EVENT e) {
            if (TargetingEvent != null) {
                TargetingEvent(target, e);
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

namespace AimingEvents {
    public enum AIMING_EVENT { SCANNING, FOUND, AIM_OFF };

    public class AimingData {
        public Vector3 direction;
        public GameObject target = null;

        public AimingData(Vector3 _direction, GameObject _target) {
            direction = _direction;
            target = _target;
        }
    }

    public class AimingPublisher {
        public delegate void AimingEventHandler(AimingData data, AIMING_EVENT e);
        public event AimingEventHandler AimingEvent;

        public void OnAimingEvent(AimingData data, AIMING_EVENT e) {
            if (AimingEvent != null) {
                AimingEvent(data, e);
            } else {
                Debug.Log("NOOP");
            }
        }
    }
}

namespace EnemyHealthEvents {
    public enum ENEMY_HEALTH_EVENT { INIT, UPDATE, FREE, DESTROY };

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

namespace PlayerAttackEvents {
    public enum PLAYER_ATTACK_EVENT { SPECIAL_ATTACK_START, SPECIAL_ATTACK_END, SPECIAL_ATTACK_COMPLETE, SPECIAL_ATTACK_TERMINATE };

    public class PlayerAttackControllerPublisher {
        public delegate void PlayerAttackControllerHandler(string weapon_name, PLAYER_ATTACK_EVENT e);
        public event PlayerAttackControllerHandler PlayerAttackEvent;

        public void OnPlayerAttackEvent(string weapon_name, PLAYER_ATTACK_EVENT e) {
            if (PlayerAttackEvent != null) {
                PlayerAttackEvent(weapon_name, e);
            }
            else {
                Debug.Log("NOOP");
            }
        }
    }
}

namespace MovementEvents {
    public enum MOVEMENT_EVENT { SPECIAL_ATTACK_END, SPECIAL_ATTACK_COMPLETE }

    public class MovementEventPublisher {
        public delegate void MovementEventHandler(MOVEMENT_EVENT e);
        public event MovementEventHandler MovementEvent;

        public void OnMovementEvent(MOVEMENT_EVENT e) {
            if (MovementEvent != null) {
                MovementEvent(e);
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
    public AimingEvents.AimingPublisher aiming_publisher;
    public PlayerAttackEvents.PlayerAttackControllerPublisher attacks_publisher;
    public MovementEvents.MovementEventPublisher movement_publisher;

    // Use this for initialization
    void Start () {
		
	}

    void Awake() {
        inventory_publisher = new InventoryEvents.InventoryPublisher();
        targeting_publisher = new TargetingEvents.TargetingPublisher();
        health_publisher = new PlayerHealthEvents.PlayerHealthPublisher();
        enemy_health_publisher = new EnemyHealthEvents.EnemyHealthEventPublisher();
        aiming_publisher = new AimingEvents.AimingPublisher();
        attacks_publisher = new PlayerAttackEvents.PlayerAttackControllerPublisher();
        movement_publisher = new MovementEvents.MovementEventPublisher();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
