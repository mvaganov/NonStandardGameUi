using System;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class InventoryItemObject : MonoBehaviour {
		public InventoryItem item = new InventoryItem();
		public PickupRules rules = new PickupRules();
		[System.Serializable] public class PickupRules {
			[Tooltip("How long to wait before picking up a dropped object")]
			public float pickupDelay = 2.5f;
			[Tooltip("Set automatically when dropped, so that the player does not immediately re-acquire a dropped object")]
			public float waitTill;
			public bool automaticallySetCollider = true;
			public bool enableColliderForPickup = true;
			public bool CanBePickedUpByCollision(InventoryCollector collector) {
				return collector != null && enableColliderForPickup && Time.time >= waitTill;
			}
			public void DelayPickup() {
				waitTill = Time.time + pickupDelay;
			}
		}
		private void Awake() {
			item.component = this;
		}
		private void Start() {
			Collider trigger = GetComponent<Collider>();
			if (trigger == null) {
				Renderer renderer = GetComponent<Renderer>();
				Bounds b = renderer.bounds;
				Vector3 center = b.center;
				float radius = b.extents.magnitude;
				SphereCollider sc = gameObject.AddComponent<SphereCollider>();
				sc.center = center - transform.position;
				sc.radius = radius;
				Rigidbody rb = GetComponent<Rigidbody>();
				if (rb == null || !rb.useGravity) {
					sc.isTrigger = true;
				}
			}
		}
		public void PickupRequestBy(GameObject gameObject) {
			InventoryCollector collector = gameObject.GetComponent<InventoryCollector>();
			if (!rules.CanBePickedUpByCollision(collector)) return;
			EventGiver confirm = GetComponent<EventGiver>();
			if (confirm == null) {
				SetPickedUp(collector);
				return;
			}
			confirm.Invoke(gameObject);
			// modal confirm
			// progress bar
			// modal confirm into progress bar
			// puzzle
			// request from partial owners
		}
		public void SetPickedUp(GameObject collectorObject) {
			InventoryCollector collector = collectorObject.GetComponent<InventoryCollector>();
			if (collector == null) {
				Debug.LogError(collector+" is not a "+nameof(InventoryCollector)+" object, cannot pickup "+name);
				return;
			}
			SetPickedUp(collector);
		}
		public void SetPickedUp(InventoryCollector collector) {
			item.SetPickedUpBy(collector);
		}
		public void OnEnable() {
			rules.DelayPickup();
		}
		private void OnTriggerEnter(Collider other) {
			PickupRequestBy(other.gameObject);
		}
		private void OnCollisionEnter(Collision collision) {
			PickupRequestBy(collision.gameObject); 
		}
	}
}
