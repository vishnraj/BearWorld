using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utility;
using InputEvents;
using TargetingEvents;
using AimingEvents;
using EnemyHealthEvents;

public class ThirdPersonTargetingSystem : MonoBehaviour
{
    public float current_weapon_range;
    public bool switching = false;
    public bool forward_facing = true;

    public Vector3 direction;
    public GameObject target = null;
    public GameObject Enemies;
    public GameObject event_manager;
    public SortedList sorted_by_chosen_direction; // targets sorted by closest distance to current target
                                                  // as well as closeness of the angle of the vector to new target
                                                  // to the angle created by the joystick's coordinates
                                                  // if there is a target, otherwise just sorts using InstanceIDs
                                                  // in ascending order

    bool paused = false;
    bool start = false;
    float input_x = 0.0f;
    float input_y = 0.0f;
    float current_joystick_angle = 0.0f;
    float margin_of_error = 5.0f;

    Vector3 previous_target_pos;

    TargetingPublisher publisher;
    CloseCompare compare_distances;
    Rotation rt;
    Player c;

    delegate void DoUpdate();
    DoUpdate update;

    // Use this for initialization
    void Start()
    {
        previous_target_pos = new Vector3();

        compare_distances = new CloseCompare(this);
        sorted_by_chosen_direction = new SortedList(compare_distances);

        rt = new Rotation();
        c = GetComponent<Player>();

        publisher = event_manager.GetComponent<ComponentEventManager>().targeting_publisher;

        event_manager.GetComponent<InputManager>().publisher.InputEvent += GlobalInputEventsCallback;
        event_manager.GetComponent<ComponentEventManager>().aiming_publisher.AimingEvent += AimingEventsCallback;
        event_manager.GetComponent<ComponentEventManager>().enemy_health_publisher.EnemyHealthEvent += EnemyHealthEventsCallback;

        update = null;
    }

    private void Awake() {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!start) {
            publisher.OnTargetingEvent(null, TARGETING_EVENT.INIT);
            c.SetInLockOn(false);

            start = true;
            enabled = false; // after start, this really should only ever be enabled by AimingEvents
            return;
        }

        // update should always be set by the time we
        // get here
        if (update != null) {
            update();
        } else {
            Debug.Log("How did we get here?");
        }
    }

    void LockedOnUpdate() {
        // first checking if we are going to leave targeting state
        if (Input.GetAxis("LeftTriggerAxis") == 0) {
            DisableTargeting(); // if we can lock, aiming system will cover us
            return;
        }

        if (!IsValidTarget(target)) {
            target = GetClosestEnemy(Enemies.GetComponent<EnemyTracker>().GetAllEnemies());

            if (target == null) {
                DisableTargeting();
                return;
            }

            EngageNewTarget();
        }

        if ((Input.GetAxis("RightJoystickX") != 0 || Input.GetAxis("RightJoystickY") != 0) && !switching) {
            switching = true;
            SetNewTarget();
        }

        if ((Input.GetAxis("RightJoystickX") == 0 && Input.GetAxis("RightJoystickY") == 0) && switching) {
            switching = false;
        }

        if (IsValidTarget(target)) {
            c.SetTarget(target); // required to keep grabbing target position
            c.SetAimingDirection(target.transform.position);
        }
    }

    void CanLockUpdate() {
        if (!IsValidTarget(target)) {
            DisableTargeting();
            return;
        }

        if (Input.GetAxis("LeftTriggerAxis") > 0) {
            EngageNewTarget();
            update = LockedOnUpdate;
        }
    }

    void GlobalInputEventsCallback(object sender, INPUT_EVENT e) {
        switch (e) {
            case INPUT_EVENT.PAUSE: {
                    enabled = false; // this must happen regardless
                    paused = true;
                }
                break;
            case INPUT_EVENT.UNPAUSE: {
                    if (update == CanLockUpdate || update == LockedOnUpdate) {
                        enabled = true;
                    }
                    paused = false;
                }
                break;
            default:
                break;
        }
    }

    void AimingEventsCallback(AimingData data, AIMING_EVENT e) {
        switch (e) {
            case AIMING_EVENT.SCANNING: {
                    direction = data.direction;
                    c.SetAimingDirection(direction);

                    if (update == CanLockUpdate) {
                        update = null;
                        enabled = false;
                    }

                    publisher.OnTargetingEvent(null, TARGETING_EVENT.FREE);
                    c.SetInLockOn(false);
                }
                break;
            case AIMING_EVENT.FOUND: {
                    GameObject _target = data.target;
                    if (!IsValidTarget(_target)) {
                        goto case AIMING_EVENT.SCANNING;
                    }

                    target = _target;
                    c.SetTarget(target);
                    c.SetAimingDirection(target.transform.position);

                    publisher.OnTargetingEvent(null, TARGETING_EVENT.CAN_LOCK);
                    c.SetInLockOn(false);

                    update = CanLockUpdate;
                    if (!paused) {
                        enabled = true;
                    }
                }
                break;
            case AIMING_EVENT.AIM_OFF: {
                    // since this can happen while paused
                    // just call it anyway, regardless of enabled or not
                    DisableTargeting();
                }
                break;
            default:
                break;
        }
    }

    void EnemyHealthEventsCallback(EnemyHealthData data, ENEMY_HEALTH_EVENT e) {
        switch(e) {
            case ENEMY_HEALTH_EVENT.DESTROY: {
                    // an enemy health even destory may not always indicate that
                    // we need to reset the target, but if needed then we do it
                    if (!Enemies.GetComponent<EnemyTracker>().FindEnemy(target)) {
                        target = GetClosestEnemy(Enemies.GetComponent<EnemyTracker>().GetAllEnemies());

                        if (target == null) {
                            DisableTargeting();
                            return;
                        }

                        EngageNewTarget();
                    }
                }
                break;
            default:
                break;
        }
    }

    bool IsValidTarget(GameObject _target) {
        // sanity checks, but neither should happen, because the
        // EnemyHealthEventsCallback should capture when a destroy occurs
        // meaning if the target suddenly becomes invalid, we reset it there
        if (_target != null && Enemies.GetComponent<EnemyTracker>().FindEnemy(_target)) {
            Vector3 to_target = _target.transform.position - transform.position;
            if (!(to_target.magnitude <= current_weapon_range)) {
                return false;
            }
        } else {
            return false;
        }

        return true;
    }

    GameObject GetClosestEnemy(List<GameObject> enemies) {
        GameObject g_min = null;
        float min_dist = Mathf.Infinity;
        foreach (GameObject g in enemies) {
            if (!IsValidTarget(g)) {
                continue;
            }

            float dist = Vector3.Distance(g.transform.position, previous_target_pos);
            if (dist < min_dist) {
                g_min = g;
                min_dist = dist;
            }
        }
        return g_min;
    }

    void DisableTargeting() {
        switching = false;
        target = null;
        c.SetInLockOn(false);
        c.SetTarget(null);

        update = null; // certain things compare to this for knowning state
        enabled = false;

        publisher.OnTargetingEvent(null, TARGETING_EVENT.FREE);
    }

    void EngageNewTarget() {
        c.SetTarget(target);
        c.SetAimingDirection(target.transform.position);
        c.SetInLockOn(true);

        previous_target_pos = target.transform.position;

        publisher.OnTargetingEvent(target, TARGETING_EVENT.LOCK_ON);
    }

    void SetNewTarget()
    {
        GameObject new_target = null;

        input_x = Input.GetAxis("RightJoystickX");
        input_y = -Input.GetAxis("RightJoystickY");

		current_joystick_angle = rt.CalculateZXRotation(new Vector3(input_x, 0, input_y));

        sorted_by_chosen_direction.Clear();
        List<GameObject> enemies = Enemies.GetComponent<EnemyTracker>().GetAllEnemies();
        for (int i = 0; i < enemies.Count; ++i) {
			GameObject current_enemy = enemies[i];

			// here we will remove enemies from consideration, if, after being normalized to
			// an x and y (really z and x) pair that is in the player's axis, they are not
			// of the same sign for their corresponding set of inputs from the joystick
			Vector3 player_forward = new Vector3(transform.forward.x, 0, transform.forward.z);

			Vector3 to_current_enemy = current_enemy.transform.position - target.transform.position;
			float angle_to_new_target = rt.CalculateZXRotation(new Vector3(to_current_enemy.x, 0, to_current_enemy.z), player_forward);
				
			// Cos is calculated on y because z is effectively y
			// in our coordinate system
			float new_target_x_unit = Mathf.Sin(Mathf.Deg2Rad * angle_to_new_target);
			float new_target_y_unit = Mathf.Cos(Mathf.Deg2Rad * angle_to_new_target);

			bool bad_x = (new_target_x_unit > 0 && input_x < 0) || (new_target_x_unit < 0 && input_x > 0);
			bool bad_y = (new_target_y_unit > 0 && input_y < 0) || (new_target_y_unit < 0 && input_y > 0);
			if (bad_x || bad_y) {
				continue;
			}

            if (current_enemy.GetInstanceID() != target.GetInstanceID()) {
                sorted_by_chosen_direction.Add(current_enemy, current_enemy.GetInstanceID());
            }
        }

        try {
			new_target = (GameObject)sorted_by_chosen_direction.GetKey(0);
			target = new_target;

            if (!IsValidTarget(target)) {
                DisableTargeting();
                return;   
            }

            EngageNewTarget();
        } catch (System.ArgumentOutOfRangeException) {}
    }

    class CloseCompare : Comparer<GameObject>
    {
        ThirdPersonTargetingSystem parent;

        public CloseCompare(ThirdPersonTargetingSystem _parent)
            : base()
        {
            parent = _parent;
        }

        public override int Compare(GameObject a, GameObject b)
        {
            Vector3 to_a = a.transform.position - parent.target.transform.position;
            Vector3 to_b = b.transform.position - parent.target.transform.position;
            Vector3 player_forward = new Vector3(parent.transform.forward.x, 0, parent.transform.forward.z);

            float angle_a = parent.rt.CalculateZXRotation(new Vector3(to_a.x, 0, to_a.z), player_forward);
            float angle_b = parent.rt.CalculateZXRotation(new Vector3(to_b.x, 0, to_b.z), player_forward);
             
            int diff_angle_a = (int) Mathf.Abs(parent.current_joystick_angle - angle_a);
            int diff_angle_b = (int) Mathf.Abs(parent.current_joystick_angle - angle_b);

            if (diff_angle_a < diff_angle_b)
            {
                float distance_a = Vector3.Magnitude(to_a);
                float distance_b = Vector3.Magnitude(to_b);

                if (diff_angle_a + parent.margin_of_error < diff_angle_b)
                {
                    return -1;
                }
                else if (distance_a <= distance_b)
                {
                    return -1;
                }
            }
            if (diff_angle_a == diff_angle_b)
            {
                float distance_a = Vector3.Magnitude(to_a);
                float distance_b = Vector3.Magnitude(to_b);

                if (distance_a <= distance_b)
                {
                    return -1;
                }
            }
            if (diff_angle_a > diff_angle_b)
            {
                float distance_a = Vector3.Magnitude(to_a);
                float distance_b = Vector3.Magnitude(to_b);

                if (diff_angle_a > diff_angle_b + parent.margin_of_error)
                {
                    return 1;
                }
                else if (distance_a <= distance_b)
                {
                    return -1;
                }
            }

            return 1;
        }
    }
}