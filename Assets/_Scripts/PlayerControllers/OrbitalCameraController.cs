using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalCameraController : MonoBehaviour {

	//[Tooltip("Whether or not the camera should interact with objects in the environment, moving closer to avoid obstructions.")]
	//public bool useCameraBoomStick = true;
	[Tooltip("Affects the point in space relative to the character that the camera focuses on. " +
		"Higher values cause the camera to focus on the top of or above the character, " +
		"negative values focus the camera below.")]
	public float cameraOffsetY = 2.0f;
	[Tooltip("Affects the point in space relative to the character that the camera focuses on. " +
		"Positive values cause the camera to focus to the right the character (over the shoulder), " +
		"negative values focus the camera on the left.")]
	public float cameraOffsetX = 1.0f;
	[Tooltip("Limits how far the camera can rotate vertically down. Values below -90 will cause bad spinning.")]
	public float cameraMinAngleY = -50.0f;
	[Tooltip("Limits how far the camera can rotate vertically up. Values above 90 will cause bad spinning.")]
	public float cameraMaxAngleY = 50.0f;
	[Tooltip("Limits how far the camera can zoom in. Must be greater than 0.")]
	public float cameraMinZ = 5.0f;
	[Tooltip("Limits how far the camera can zoom out.")]
	public float cameraMaxZ = 60.0f;
	[Tooltip("The initial distance of the camera.")]
	public float cameraDistance = 22;
	[Tooltip("The sensitivity of the camera as it moves around the X-axis (left and right). Negative values with invert the controls.")]
	public float camSmoothX = 4.0f;
	[Tooltip("The sensitivity of the camera as it moves around the Y-axis (up and down). Negative values with invert the controls.")]
	public float camSmoothY = -1.2f;
	[Tooltip("The sensitivity of the camera as it zooms in and out. Negative values with invert the controls.")]
	public float camSmoothZ = -10.0f;


	[Tooltip("While true, pressing 'Exit' (Default: 'Esc' key) while playing in the editor will quit stop the play test.")]
	public bool useQuickExit = true;
	[Tooltip("While true, pressing 'ToggleLockedCursor' (Default: 'Tab' key) while playing in the editor will hide/unhide the mouse cursor.")]
	public bool useQuickCursorToggle = true;

	public bool startWithMouseCaptured = false;



	public bool cameraCollidesWithTerrain = true;
	public LayerMask collideLayer;
	public float cameraWidth = 1f;

	#region Private Variables

	Camera cam;
	Transform camTransform;
	float cameraX;
	float cameraY;
	Vector3 cameraOffset;
	private bool controllerEnabled = true;
	/// <summary>
	/// The state of the current input, whether we're using the mouse/keyboard or gamepad.
	/// </summary>
	private ControllerState controllerState = new ControllerState();

	#endregion

	void Start() {
		// Grab a handle to the camera
		cam = Camera.main;
		camTransform = cam.transform;
		// Lock and hide the cursor
		if (startWithMouseCaptured) {
			Cursor.lockState = CursorLockMode.Locked;
		}
		
		
	}
	
	void Update () {
		if ( controllerEnabled ) {
			CalculatePosition();
		}

		Hotkeys();
	}

	void Hotkeys() {
		if (useQuickExit && Application.isPlaying && Input.GetButtonDown("Exit")) {
			//Debug.Log("Used Quick Exit");
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}

		if (useQuickCursorToggle && Application.isPlaying && Input.GetButtonDown("ToggleLockedCursor")) {
			// Lock and hide the cursor
			if (Cursor.lockState == CursorLockMode.Locked) {
				Cursor.lockState = CursorLockMode.None;
			} else {
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
	}

	void FixedUpdate() {
		if (controllerEnabled) {
			Reposition();
		}
	}

	public void CalculatePosition() {
		controllerState.Update();
		cameraX += controllerState.currentX * camSmoothX;
		cameraY += controllerState.currentY * camSmoothY;

		//Debug.Log(string.Format("{0},{1}", cameraX, cameraY));

		cameraY = Mathf.Clamp(cameraY, cameraMinAngleY, cameraMaxAngleY);

		cameraDistance += Input.GetAxis("Mouse ScrollWheel") * camSmoothZ;
		cameraDistance = Mathf.Clamp(cameraDistance, cameraMinZ, cameraMaxZ);
	}

	public void Reposition() {
		if (cam==null) {
			// Grab a handle to the camera
			cam = Camera.main;
			camTransform = cam.transform;
		}

		// go out from this character a certain distance by a certain angle
		Vector3 dir = new Vector3(0, 0, -cameraDistance);
		Quaternion rotation = Quaternion.Euler(cameraY, cameraX, 0);
		cameraOffset.Set(cameraOffsetX, cameraOffsetY, 0.0f);

		// perform a ray cast out from the character
		// if we hit something we can put our camera there in order to stay in front of things that'll obstruct our view
		Ray camRay = new Ray(transform.position + cameraOffset, rotation * -Vector3.forward) ;
		RaycastHit camRayhit;
		if (cameraCollidesWithTerrain && Physics.Raycast(camRay, out camRayhit, cameraDistance, collideLayer)) {
			camTransform.position = Vector3.Lerp(camTransform.position, camRayhit.point, 0.8f);
		}
		else {
			camTransform.position = Vector3.Lerp(camTransform.position, transform.position + rotation * (dir + cameraOffset), 0.8f);
		}

		Vector3 focusPoint = transform.position + rotation * (cameraOffset);
		camTransform.LookAt(focusPoint);
		//Vector3 forward = camTransform.forward.DropY();

	}
}
