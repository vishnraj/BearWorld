using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PigKnightAI : BasicEnemyAI {
    [SerializeField]
    float ray_radius;
    [SerializeField]
    float swing_range;
    [SerializeField]
    int num_attack_frames_hold;

    int attack_frame_start = 0;
    bool is_attacking = false;
    NavMeshAgent agent;

    // Use this for initialization
    void Start() {
        Init();

        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update() {
        if (target != null) {
            agent.SetDestination(target.transform.position);

            // only if we can see the player - raycast
            RaycastHit hit;
            Vector3 to_target = target.transform.position - transform.position;
            Ray ray = new Ray(transform.position, to_target);

            int layer = 1 << LayerMask.NameToLayer("Current_Realm");
            if (Physics.SphereCast(ray, ray_radius, out hit, swing_range, layer) && hit.collider.gameObject.tag == "Player" && !is_attacking) {
                Attack(to_target);
            }

            if (is_attacking && (Time.frameCount - attack_frame_start) >= num_attack_frames_hold) {
                w.EndAttack();
                is_attacking = false;
            }
        }
        else {
            agent.speed = 0;
        }
    }

    protected override void Init() {
        base.Init();
    }

    void Attack(Vector3 to_target) {
        c.SetTarget(target);
        c.SetAimingDirection(target.transform.position);
        Vector3 direction = Vector3.RotateTowards(transform.forward, to_target, Mathf.PI, 0);
        transform.rotation = Quaternion.LookRotation(direction);
        w.Attack();
        is_attacking = true;
        attack_frame_start = Time.frameCount;
    }
}
