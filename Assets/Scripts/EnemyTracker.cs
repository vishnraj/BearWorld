using System.Collections.Generic;
using UnityEngine;

public class EnemyTracker : MonoBehaviour {
    public List<GameObject> enemies;

	// Use this for initialization
	void Start () {
        
	}

    private void Awake() {
        enemies = new List<GameObject>();
    }

    // Update is called once per frame
    void Update () {
        enemies.RemoveAll(item => item == null);
    }
}
