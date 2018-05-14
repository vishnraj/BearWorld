using System.Collections.Generic;
using UnityEngine;

public class EnemyTracker : MonoBehaviour {
    List<GameObject> enemies;

    // Use this for initialization
    void Start () {
        
    }

    private void Awake() {
        enemies = new List<GameObject>();
    }

    // Update is called once per frame
    void Update () {
        
    }
    
    public void RemoveEnemy(GameObject enemy) {
        enemies.Remove(enemy);
    }

    public void AddEnemy(GameObject enemy) {
        enemies.Add(enemy);
    }

    public bool FindEnemy(GameObject enemy) {
        if (enemies.Exists(x => x.GetInstanceID() == enemy.GetInstanceID())) {
            return true;
        }

        return false;
    }

    public int Count() {
        return enemies.Count;
    }

    public List<GameObject> GetAllEnemies() {
        return enemies;
    }
}
