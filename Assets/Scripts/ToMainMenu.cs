using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ToMainMenu : MonoBehaviour {
	// Use this for initialization
	void Start () {
        gameObject.GetComponent<Button>().onClick.AddListener(LoadMenu);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void LoadMenu() {
        SceneManager.LoadScene("GameScenes/MainMenu");
        Time.timeScale = 1; // unpause - if paused
    }
}
