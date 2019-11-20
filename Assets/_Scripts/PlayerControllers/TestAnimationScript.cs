using UnityEngine;
using UnityEngine.UI;

public class TestAnimationScript : MonoBehaviour {
	public Slider sidewaysSlider;
	public Slider forwardsSlider;

	public Toggle isRunningToggle;

	public Animator animator;

	private void LateUpdate() {
		var verticalMovement = forwardsSlider.value;
		var horizontalMovement = sidewaysSlider.value;

		animator.SetFloat("ForwardSpeed", verticalMovement);
		animator.SetFloat("SidewaysSpeed", horizontalMovement);

		animator.SetBool("Running", isRunningToggle.isOn);
	}
}
