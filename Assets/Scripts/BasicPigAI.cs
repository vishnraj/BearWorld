using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicPigAI : BasicEnemyAI {
    public float speed;

    // Use this for initialization
    void Start () {
        Init();
    }
	
	// Update is called once per frame
	void Update () {
        if (target != null) {
            c.SetTarget(target);
            c.SetAimingDirection(target.transform.position);
            Vector3 to_target = target.transform.position - transform.position;
            Vector3 direction = Vector3.RotateTowards(transform.forward, to_target, Mathf.PI, 0);
            transform.rotation = Quaternion.LookRotation(direction);

           
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, step);
        } else {
            w.EndAttack();
        }
    }

    protected override void Init() {
        base.Init();
        if (target != null) {
            w.Attack();
        }
    }
}
