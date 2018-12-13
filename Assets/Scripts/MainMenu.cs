using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    [SerializeField]
    private Button m_start;

    [SerializeField]
    private Button m_exit;

    // Use this for initialization
    void Start() {
        m_start.onClick.AddListener(PlayGame);
        m_exit.onClick.AddListener(ExitGame);
    }

    void PlayGame() {
        SceneManager.LoadScene("GameScenes/DungeonSelection");
    }

    void ExitGame() {
        #if UNITY_EDITOR
                // Application.Quit() does not work in the editor so
                // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}
