using UnityEngine;

namespace NonStandard.GameUi {
	public class ActivateUiOnCollision : Interactable {
		// TODO triggered by collision system kind
		public LayerMask interactable;
		private void Start() {
		}
		public bool IsAllowedToInteract(GameObject otherGameObject) {
			return (otherGameObject.layer & interactable) != interactable;
		}
		private void OnCollisionEnter(Collision collision) {
			if (!enabled || IsAllowedToInteract(collision.gameObject)) return;
			UiGiverBase eg = GetComponent<UiGiverBase>();
			eg.Invoke(collision.gameObject);
		}
		private void OnTriggerEnter(Collider other) {
			if (!enabled || IsAllowedToInteract(other.gameObject)) return;
			UiGiverBase eg = GetComponent<UiGiverBase>();
			eg.Invoke(other.gameObject);
		}
	}
}