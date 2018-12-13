using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputEvents;

public class ExitLevelButton : MonoBehaviour {
    [SerializeField]
    private GameObject m_event_manager;

    private GameObject m_button = null;

    // Use this for initialization
    void Start () {
        m_event_manager.GetComponent<InputManager>().publisher.InputEvent += GlobalInputEventsCallback;
        enabled = false;
    }

    void GlobalInputEventsCallback(object sender, INPUT_EVENT e) {
        switch (e) {
            case INPUT_EVENT.PAUSE: {
                    // create button
                    m_button = Instantiate(Resources.Load("Prefabs/ToMainMenu"), transform.position, new Quaternion()) as GameObject;
                    m_button.transform.SetParent(transform);
                    m_button.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(300, -150, 0);
                    enabled = true;
                }
                break;
            case INPUT_EVENT.UNPAUSE: {
                    // destroy button
                    Destroy(m_button);
                    enabled = false;
                }
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
