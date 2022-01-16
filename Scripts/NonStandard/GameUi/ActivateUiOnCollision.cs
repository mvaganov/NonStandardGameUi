using UnityEngine;

namespace NonStandard.GameUi {
	public class ActivateUiOnCollision : MonoBehaviour {
		// TODO triggered by collision system
		private void Start() {
		}
		private void OnCollisionEnter(Collision collision) {
			if (!enabled) return;
			UiGiverBase eg = GetComponent<UiGiverBase>();
			eg.Invoke(collision.gameObject);
		}
		private void OnTriggerEnter(Collider other) {
			if (!enabled) return;
			UiGiverBase eg = GetComponent<UiGiverBase>();
			eg.Invoke(other.gameObject);
		}
	}
}