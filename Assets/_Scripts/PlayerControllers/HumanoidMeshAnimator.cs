
using UnityEngine;

public class HumanoidMeshAnimator : MonoBehaviour {

	public float forwardSpeedToAnimationScale = 8f;
	public float sidewaysSpeedToAnimationScale = 8f;
	public float turnSpeedToAnimationScale = 8f;
	
	public Grounded groundedComponent;
	
	//public float maxUpperBodyWeight = 1f;
	
	Animator animator;

	Vector3 lastPosition;
	float lastRotationY = 0;

	bool inLocomotion = false;

	bool jumpBoosting = false;
	
	//public int upperBodyLayerIndex = 2;

	//public float upperBodyWeightChangeScale = 0.1f;

	//private float targetUpperBodyWeight = 0f;
	
	private void Start() {
		animator = GetComponent<Animator>();
		lastPosition = transform.position;
		lastRotationY = transform.rotation.eulerAngles.y;
		
	}
	
	public void ManualFireTrigger(string trigger) {
		animator.SetTrigger(trigger);
	}
	
	private void FixedUpdate() {
		var velocity = transform.InverseTransformVector(GetVelocity().DropY());

		float horizontalMovement = velocity.x;
		float verticalMovement = velocity.z;
		var currentTorque = GetTurnSpeed();

		float epsilon = 0.01f;

		bool stopped = velocity.sqrMagnitude < epsilon && Mathf.Abs(currentTorque) < epsilon;

		inLocomotion = !stopped;

		animator.SetBool("Running", inLocomotion);
		
		animator.SetFloat("ForwardSpeed", verticalMovement * forwardSpeedToAnimationScale);
		animator.SetFloat("SidewaysSpeed", horizontalMovement * sidewaysSpeedToAnimationScale + currentTorque * turnSpeedToAnimationScale);

		//if ( velocity.sqrMagnitude < epsilon && Mathf.Abs(currentTorque) < epsilon) {
		//	//if ( Mathf.Approximately(0, horizontalMovement) && Mathf.Approximately(0, verticalMovement) && Mathf.Approximately(0, currentTorque)) {
		//		if ( inLocomotion ) {
		//		//animator.SetTrigger("StopLocomotion");
		//		inLocomotion = false;
		//	}
		//} else {
		//	if (!inLocomotion) {
		//		//animator.SetTrigger("StartLocomotion");
		//		inLocomotion = true;
		//	}
			
		//	//animator.SetFloat("TurnSpeed", currentTorque * turnSpeedToAnimationScale);

		//	animator.SetFloat("ForwardSpeed", verticalMovement * forwardSpeedToAnimationScale);
		//	animator.SetFloat("SidewaysSpeed", horizontalMovement * sidewaysSpeedToAnimationScale + currentTorque * turnSpeedToAnimationScale);
		//}

		//var currentTorque = GetTurnSpeed();
		//animator.SetFloat("TurnSpeed", currentTorque * turnSpeedToAnimationScale);

		lastPosition = transform.position;
		lastRotationY = transform.rotation.eulerAngles.y;

		//if (controller != null) {
		//	animator.SetBool("Grounded", controller.IsGrounded);
		//} else
		if (groundedComponent != null) {
			animator.SetBool("Grounded", groundedComponent.IsGrounded);
		}


		//float currentWeight = animator.GetLayerWeight(upperBodyLayerIndex);
		//float difference = targetUpperBodyWeight - currentWeight;

		//animator.SetLayerWeight(upperBodyLayerIndex, currentWeight+ difference* upperBodyWeightChangeScale);
	}
	
	private Vector3 GetVelocity() {
		return transform.position - lastPosition;
	}

	private float GetTurnSpeed() {
		return transform.rotation.eulerAngles.y - lastRotationY;
	}
	
}
