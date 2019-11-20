using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grounded : MonoBehaviour {

	public LayerMask groundLayer;

	public bool IsGrounded;

	//public ExtraEvent<GroundedEvent> Events {
	//	get {
	//		if (_events == null) {
	//			_events = new ExtraEvent<GroundedEvent>(this);
	//		}
	//		return _events;
	//	}
	//}
	//ExtraEvent<GroundedEvent> _events;

	private HashSet<Collider> currentlyCollidings = new HashSet<Collider>();

	public float ungroundedDelay = 0.1f;

	private bool performingUngrounding = false;
	private float ungroundedDelayTimer = 0;

	public void OnTriggerStay(Collider collider) {
		if (CommonUtils.IsOnLayer(collider.gameObject, groundLayer)) {
			if (!IsGrounded) {
				//Events.Fire(GroundedEvent.BecomeGrounded, null);
				IsGrounded = true;
				performingUngrounding = false;
			}
			currentlyCollidings.Add(collider);
		}
	}

	public void OnTriggerExit(Collider collider) {
		if (CommonUtils.IsOnLayer(collider.gameObject, groundLayer)) {
			currentlyCollidings.Remove(collider);

			if (IsGrounded && currentlyCollidings.Count == 0) {
				StartUngroundingTimer();
			}

		}
	}


	private void StartUngroundingTimer() {
		performingUngrounding = true;
		ungroundedDelayTimer = ungroundedDelay;
	}

	public void Update() {
		if (performingUngrounding) {
			ungroundedDelayTimer -= Time.deltaTime;
			if (ungroundedDelayTimer <= 0) {
				PerformUngrounding();
			}
		}
	}

	private void PerformUngrounding() {

		if (IsGrounded && currentlyCollidings.Count == 0) {
			//Events.Fire(GroundedEvent.BecomeUngrounded, null);
			IsGrounded = false;
		}

		performingUngrounding = false;
	}
}

//public enum GroundedEvent {
//	BecomeGrounded,
//	BecomeUngrounded,
//}