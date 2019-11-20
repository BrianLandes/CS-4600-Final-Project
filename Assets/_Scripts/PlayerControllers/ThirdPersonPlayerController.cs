using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
//[RequireComponent(typeof(Collider))]
public class ThirdPersonPlayerController : MonoBehaviour {

	//[Tooltip("The layer used for checking whether the character is grounded and also for positioning the camera.")]
	//public LayerMask groundLayer;
	
	public float jumpForce = 15.0f;
	public float jumpBoostTime = 0.5f;
	
	//public float moveForce = 60;
	public float moveSpeed = 20;
	//public float moveAcceleration = 1f;

	public float turnSpeed = 1f;

	//public bool controlUnitAnimator = true;
	public bool makeUnitLookForward = true;

	public float brakeForce = 1f;

	///// <summary>
	///// 
	///// </summary>
	//public float ungroundedDelay = 0.1f;

	//private bool performingUngrounding = false;
	//private float ungroundedDelayTimer = 0;

	public Grounded groundedComponent;

	////public bool brake = true;
	////public float brakeRate = 8.0f;
	////public float extraGravity = -50.0f;

	//private HashSet<Collider> currentlyCollidings = new HashSet<Collider>();

	//public ExtraEvent<ThirdPersonControllerEvent> Events {
	//	get {
	//		if (_events == null) {
	//			_events = new ExtraEvent<ThirdPersonControllerEvent>(this);
	//		}
	//		return _events;
	//	}
	//}
	//ExtraEvent<ThirdPersonControllerEvent> _events;

	#region Private Variables

	private float jumpBoostTimer = 0.0f;

	//public bool IsGrounded = false;
	private bool _jump = false;

	Rigidbody myBody;
	Vector3 movement;

	//UnitAnimator animator;

	private bool controllerEnabled = true;

	/// <summary>
	/// The state of the current input, whether we're using the mouse/keyboard or gamepad.
	/// </summary>
	private ControllerState controllerState = new ControllerState();
	
	#endregion
	
	void Start () {
		myBody = GetComponent<Rigidbody>();

		//animator = GetComponentInChildren<UnitAnimator>();
		//if (controlUnitAnimator && animator == null)
		//	controlUnitAnimator = false;
	}

	void Update() {
		MoveUpdate();
		//AnimatorUpdate();
	}

	private void MoveUpdate() {
		controllerState.Update();

		float h = Input.GetAxisRaw("Horizontal");
		float v = Input.GetAxisRaw("Vertical");
		
		Transform camTransform = Camera.main.transform;
		movement = camTransform.forward * v + camTransform.right * h;
		//movement.y = 0.0f;

		//if (myBody.velocity.magnitude < moveSpeed)
		//	movement = movement.normalized * moveForce;
		//else
		//	movement = Vector3.zero;

		// Different way of doing jump/ground check
		//IsGrounded = false;
		//RaycastHit groundRayHit;
		//Ray groundRay = new Ray(transform.position, Vector3.down);
		//if ( Physics.Raycast(groundRay, out groundRayHit, height*0.6f, groundLayer) ) {
		//	IsGrounded = true;
		//}

		//if (brake && movement.magnitude < 0.001f && IsGrounded) {
		//	movement = -myBody.velocity * brakeRate;
		//}
		//if (!IsGrounded)
		//	movement.y += extraGravity;

		if (!_jump && groundedComponent.IsGrounded && Input.GetButtonDown("Jump")) {
			_jump = true;
			//Events.Fire(ThirdPersonControllerEvent.Jump, null);
		}
	}

	//void AnimatorUpdate() {
	//	if (!controlUnitAnimator) {
	//		return;
	//	}

	//	animator.IsInAir = !IsGrounded;
	//	animator.IsRunning = movement.magnitude > 0.01f;

	//}

	private void FixedUpdate() {
		//if (IsGrounded)
		//	myBody.angularVelocity = Vector3.zero;

		//UpdateUngroundingTimer();

		if (jumpBoostTimer > 0.0f) {
			jumpBoostTimer -= Time.deltaTime;
		}
		if (_jump) {
			myBody.velocity = new Vector3(myBody.velocity.x, jumpForce, myBody.velocity.z);
			jumpBoostTimer = jumpBoostTime;
			_jump = false;
		} else
		if (jumpBoostTimer > 0.0f && Input.GetButton("Jump")) {
			float v = jumpForce * Mathf.Sin(Mathf.PI * 0.5f * (jumpBoostTimer / jumpBoostTime));
			myBody.velocity = new Vector3(myBody.velocity.x, v, myBody.velocity.z);
		} else {
			jumpBoostTimer = 0.0f;
		}

		//movement.y = 0.0f;
		//movement = movement.normalized * moveSpeed;
		//myBody.velocity = movement + myBody.velocity.JustY();

		var moveVector = movement.DropY().normalized;

		if ( moveVector.sqrMagnitude > 0.01f) {
			var idealVelocity = moveVector * moveSpeed;
			var currentVelocity = myBody.velocity;
			var idealDifference = idealVelocity - currentVelocity;
			myBody.AddForce(idealDifference);
		} else {
			var currentVelocity = myBody.velocity;
			var idealDifference = - currentVelocity;
			myBody.AddForce(idealDifference * brakeForce);
		}
		

		if (makeUnitLookForward) {
			TurnToDirection(Camera.main.transform.forward.JustXZ());
			//Vector3 forward = Camera.main.transform.forward.DropY();
			//transform.rotation = Quaternion.LookRotation(forward);
		}  else 
		if (moveVector.sqrMagnitude > 0.01f) {
			TurnToDirection(moveVector.JustXZ());

			//transform.rotation = Quaternion.LookRotation(myBody.velocity.DropY());
		}

		var currentTorque = myBody.angularVelocity;
		//Debug.Log(currentTorque);
		myBody.AddTorque(0, -currentTorque.y*0.6f,0);
	}

	private void TurnToDirection(Vector2 xzVector) {
		float targetTheta = Mathf.Atan2(xzVector.y, xzVector.x);
		var currentVector = transform.forward.JustXZ().normalized;
		float currentTheta = Mathf.Atan2(currentVector.y, currentVector.x);
		float difference = CommonUtils.AngleBetweenThetas(targetTheta, currentTheta);

		myBody.AddTorque(0, difference, 0);
	}

	///// <summary>
	///// Used in tandem with the FootContact to keep track of jump/grounded
	///// </summary>
	///// <param name="collider"></param>
	//public void OnTriggerStay(Collider collider) {
	//	if (CommonUtils.IsOnLayer(collider.gameObject, groundLayer)) {
	//		if ( !IsGrounded ) {
	//			Events.Fire(ThirdPersonControllerEvent.Land, null);
	//			IsGrounded = true;
	//			performingUngrounding = false;
	//		}
	//		currentlyCollidings.Add(collider);
	//	}

	//}

	//public void OnTriggerExit(Collider collider) {
	//	if (CommonUtils.IsOnLayer(collider.gameObject, groundLayer)) {
	//		currentlyCollidings.Remove(collider);

	//		if (IsGrounded && currentlyCollidings.Count == 0 ) {
	//			StartUngroundingTimer();
	//		}
			
	//	}
	//}

	//private void StartUngroundingTimer() {
	//	performingUngrounding = true;
	//	ungroundedDelayTimer = ungroundedDelay;
	//}

	//private void UpdateUngroundingTimer() {
	//	if (performingUngrounding) {
	//		ungroundedDelayTimer -= Time.deltaTime;
	//		if ( ungroundedDelayTimer <= 0 ) {
	//			PerformUngrounding();
	//		}
	//	}
	//}

	//private void PerformUngrounding() {
		
	//	if (IsGrounded && currentlyCollidings.Count == 0) {
	//		bool jumping = _jump || jumpBoostTimer > 0f;
	//		if (IsGrounded && !jumping) {
	//			Events.Fire(ThirdPersonControllerEvent.Fall, null);
	//		}
	//		IsGrounded = false;
	//	}
		
	//	performingUngrounding = false;
	//}
}

//public enum ThirdPersonControllerEvent {
//	Jump,
//	Fall,
//	Land,
//}