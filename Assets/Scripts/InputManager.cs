using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputEvents {
    public enum INPUT_EVENT { START, X };

    public class InputPublisher {
        public delegate void InputEventHandler(object sender, INPUT_EVENT e);
        public event InputEventHandler InputEvent;

        public void OnInputEvent(INPUT_EVENT e) {
            InputEventHandler i = InputEvent;
            if (i != null) {
                i(this, e);
            } else {
                Debug.Log("NOOP");
            }
        }
    }
}

public class InputManager : MonoBehaviour {

    public InputEvents.InputPublisher publisher;
    
	// Use this for initialization
	void Start () {
        
	}

    private void Awake() {
        publisher = new InputEvents.InputPublisher();
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetButtonDown("Start")) {
            publisher.OnInputEvent(InputEvents.INPUT_EVENT.START);
        }
		if (Input.GetButtonDown("X")) {
            publisher.OnInputEvent(InputEvents.INPUT_EVENT.X);
        }
	}
}
