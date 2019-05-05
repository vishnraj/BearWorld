using UnityEngine;

namespace InputEvents {
    public enum INPUT_EVENT { PAUSE, UNPAUSE, L_TRIGGER_SET, L_TRIGGER_UNSET};

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
    bool is_paused = false; // global state driven by input
                            // thus stored here

    bool l_trigger_pushed = false;

	// Use this for initialization
	void Start () {
        
	}

    private void Awake() {
        publisher = new InputEvents.InputPublisher();
    }

    // Update is called once per frame
    void Update () {
        // pause the state of the game and
        // notify all subsystems that care about
        // this event - in most cases it will
        // set them to their paused state
        // however, in cases such as menu
        // systems, it will set the menu state
        if (Input.GetButtonDown("Start")) {
            if (!is_paused) {
                Time.timeScale = 0; // pause
                is_paused = true;
                publisher.OnInputEvent(InputEvents.INPUT_EVENT.PAUSE);
            } else {
                publisher.OnInputEvent(InputEvents.INPUT_EVENT.UNPAUSE); // all actions carried out first, then unpaused
                Time.timeScale = 1; // unpause
                is_paused = false;
            }
        }

        if (Input.GetAxis("LeftTriggerAxis") > 0) {
            if (!l_trigger_pushed) {
                l_trigger_pushed = true;
                publisher.OnInputEvent(InputEvents.INPUT_EVENT.L_TRIGGER_SET);
            }
        } else  {
            if (l_trigger_pushed) {
                l_trigger_pushed = false;
                publisher.OnInputEvent(InputEvents.INPUT_EVENT.L_TRIGGER_UNSET);
            }
        }
    }
}
