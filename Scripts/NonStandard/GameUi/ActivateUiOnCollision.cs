using UnityEngine;

namespace NonStandard.GameUi {
	public class ActivateUiOnCollision : Interactable {
		// TODO triggered by collision system kind
		public LayerMask interactable;
		private void Start() {
		}
		private void OnCollisionEnter(Collision collision) {
			if (!enabled) return;
			UiGiverBase eg = GetComponent<UiGiverBase>();
			eg.Invoke(collision.gameObject);
		}
		private void OnTriggerEnter(Collider other) {
			if (!enabled || (other.gameObject.layer & interactable) != interactable) return;
			Debug.Log(other.name+" "+ other.gameObject.layer+" "+ (int)interactable);
			UiGiverBase eg = GetComponent<UiGiverBase>();
			eg.Invoke(other.gameObject);
		}
	}
}