using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DungeonSelection : MonoBehaviour {

    [SerializeField]
    private List<Button> stages;

    // Use this for initialization
    void Start () {
		for (int i = 0; i < stages.Count; ++i) {
            int stage = i + 1;
            stages[i].onClick.AddListener(delegate { LoadAndRun(stage); });
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void LoadAndRun(int stage) {
        SceneManager.LoadScene("GameScenes/Stage" + stage.ToString());
    }
}
