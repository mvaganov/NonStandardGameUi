// code by michael vaganov, released to the public domain via the unlicense (https://unlicense.org/)
using System;
using UnityEngine;

namespace NonStandard.GameUi {
	public class GiveUiOnCollision : Interactable {
		public LayerMask interactableLayer = -1;
		public CollisionTag[] interactableTags;
		private void Start() {
		}
		public bool IsAllowedToInteract(GameObject otherGameObject) {
			int otherGameObjectLayerMask = 1 << otherGameObject.layer;
			bool allowed = (interactableLayer & otherGameObjectLayerMask) != otherGameObjectLayerMask;
			if (interactableTags != null && interactableTags.Length > 0) {
				CollisionTagged tagged = otherGameObject.GetComponent<CollisionTagged>();
				allowed = false;
				if (tagged != null) {
					int index = Array.IndexOf(interactableTags, tagged.kind);
					allowed = index >= 0;
				}
			}
			//Debug.Log(allowed + " " + otherGameObject.name+" "+Convert.ToString(flag, 2) + " v " + (int)interactable+" "+Convert.ToString(interactable, 2));
			return allowed;
		}
		private void OnCollisionEnter(Collision collision) {
			if (!enabled || IsAllowedToInteract(collision.gameObject)) return;
			UiGiverBase eg = GetComponent<UiGiverBase>();
			eg.Invoke(collision.gameObject);
		}
		private void OnTriggerEnter(Collider other) {
			if (!enabled || IsAllowedToInteract(other.gameObject)) return;
			UiGiverBase eg = GetComponent<UiGiverBase>();
			if (eg == null) { return; }
			eg.Invoke(other.gameObject);
		}
	}
}