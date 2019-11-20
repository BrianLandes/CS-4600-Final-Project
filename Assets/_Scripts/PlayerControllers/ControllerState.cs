using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerState {

	public bool verbose = true;
	
	/// <summary>
	/// The state of the current input, whether we're using the mouse/keyboard or gamepad.
	/// </summary>
	public InputType currentInputType = InputType.MouseKeyboard;

	public float currentX;
	public float currentY;

	public ControllerState() {

	}

	public enum InputType {
		MouseKeyboard,
		Controller
	}

	/// <summary>
	/// Updates the state of the controller, whether we're using the mouse/keyboard or a game pad.
	/// Allows switching between the two during game at anytime.
	/// </summary>
	public void Update() {
		switch (currentInputType) {
			case InputType.MouseKeyboard:
				if (isControllerInput()) {
					currentInputType = InputType.Controller;
					if (verbose)
						Debug.Log("PlayerManager.UpdateControllerState - switching to controller");
				} else {
					if (Cursor.lockState == CursorLockMode.Locked) {
						currentX = Input.GetAxis("Mouse X");
						currentY = Input.GetAxis("Mouse Y");
					} else {
						currentX = 0;
						currentY = 0;
					}
				}
				break;
			case InputType.Controller:
				if (isMouseKeyboardInput()) {
					currentInputType = InputType.MouseKeyboard;
					if (verbose)
						Debug.Log("PlayerManager.UpdateControllerState - switching to mouse");
				}
				break;
		}


	}

	bool isControllerInput() {
		bool changeInputs = false;

		for (int i = 0; i < 20; i++) {
			if (Input.GetKeyDown("joystick button " + i)) {
				changeInputs = true;
			}
		}

		float x = Input.GetAxisRaw("Right Joystick X");
		float y = Input.GetAxisRaw("Right Joystick Y");
		if ( changeInputs || Mathf.Abs(x) > 0.01f || Mathf.Abs(y) > 0.01f) {
			currentX = x;
			currentY = y;
			return true;
		}
		return false;
	}

	bool isMouseKeyboardInput() {
		bool changeInputs = false;
		if (Event.current != null && (Event.current.isKey || Event.current.isMouse)) {
			changeInputs = true;
		}

		float x = Input.GetAxisRaw("Mouse X");
		float y = Input.GetAxisRaw("Mouse Y");

		if (changeInputs || x != 0.0f || y != 0.0f) {
			currentX = x;
			currentY = y;
			return true;
		}
		return false;
	}
}
