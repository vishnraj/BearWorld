﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PigShooterAI : BasicEnemyAI {
    [SerializeField]
    float ray_radius;
    [SerializeField]
    float shoot_range; // range at when to begin firing

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
            if (Physics.SphereCast(ray, ray_radius, out hit, shoot_range, layer) && hit.collider.gameObject.tag == "Player") {
                Attack(to_target);
            }
            else {
                w.EndAttack();
            }
        }
        else {
            agent.speed = 0;
            w.EndAttack();
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
    }
}
