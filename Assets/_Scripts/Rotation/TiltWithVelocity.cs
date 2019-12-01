using UnityEngine;

public class TiltWithVelocity : MonoBehaviour {

	Vector3 lastPosition;

	public Transform testPoint;

	public float tiltAngle = 3f;

	private void Start() {
		lastPosition = transform.position;

	}

	private void LateUpdate() {
		var velocity = transform.InverseTransformVector((transform.position - lastPosition).DropY());
		//var velocity = testPoint.localPosition;

		transform.rotation = Quaternion.Euler(-velocity.z * tiltAngle, 0, -velocity.x * tiltAngle);
		

		lastPosition = transform.position;
	}
}
