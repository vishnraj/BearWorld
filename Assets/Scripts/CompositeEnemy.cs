using System.Collections.Generic;
using UnityEngine;

public class CompositeEnemy : MonoBehaviour {
    List<GameObject> contained_enemies;

	// Use this for initialization
	void Start () {
        contained_enemies = new List<GameObject>();

        for (int i = 0; i < transform.childCount; ++i) {
            Transform child = transform.GetChild(i);
            if (child.GetComponent<EnemyHealth>() != null) {
                contained_enemies.Add(child.gameObject);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        contained_enemies.RemoveAll(item => item == null);
        if (contained_enemies.Count == 0) {
            Destroy(gameObject);
        }
    }
}
