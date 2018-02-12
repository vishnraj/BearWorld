using System.Collections.Generic;
using UnityEngine;

public class BasicBuilding : MonoBehaviour {
    public GameObject enemies;
    
    public List<GameObject> windows;

	// Use this for initialization
	void Start () {
        windows = new List<GameObject>();

        for (int i = 0; i < transform.childCount; ++i) {
            Transform child = transform.GetChild(i);
            if (child.name.Contains("Window")) {
                windows.Add(child.gameObject);
                enemies.GetComponent<EnemyTracker>().enemies.Add(child.gameObject);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        windows.RemoveAll(item => item == null);
        if (windows.Count == 0) {
            Destroy(gameObject);
        }
    }
}
