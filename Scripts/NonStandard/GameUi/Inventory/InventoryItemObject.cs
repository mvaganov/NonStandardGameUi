// code by michael vaganov, released to the public domain via the unlicense (https://unlicense.org/)
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class InventoryItemObject : Interactable {
		public InventoryItem item = new InventoryItem();
		public PickupRules rules = new PickupRules();
		[System.Serializable] public class PickupRules {
			[Tooltip("How long to wait before picking up a dropped object")]
			public float pickupDelay = 2.5f;
			[Tooltip("Set automatically when dropped, so that the player does not immediately re-acquire a dropped object")]
			public float waitTill;
			public bool automaticallySetCollider = true;
			public bool enablePickup = true;
			public bool CanBePickedUpByCollision(InventoryCollector collector) {
				return collector != null && enablePickup && Time.time >= waitTill;
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

		public void PickupItem(Interaction interaction) {
			//Debug.Log("TODO pick up item, and remove this interaction from the interaction listing.");
			InventoryItemObject invObj = interaction.interactors[1] as InventoryItemObject;
			if (invObj == null) {
				throw new Exception("can't pick up " + interaction.interactors[1]);
			}
			InventoryCollector collector = interaction.interactors[0] as InventoryCollector;
			if (collector == null) {
				throw new Exception("can't pick up with " + interaction.interactors[0]);
			}
			invObj.SetPickedUp(collector);
		}

		public List<Interaction> PickupInteractionsFor(InventoryCollector collector) {
			List<Interaction> interactions = new List<Interaction>();
			interactions.Add(new Interaction(item.name, item.icon, 700,
				//this, nameof(PickupItem),
				new EventBind(this, nameof(PickupItem)),
				new UnityEngine.Object[] { collector, this }));
			return interactions;
		}

		public void PickupRequestBy(GameObject gameObject) {
			InventoryCollector collector = gameObject.GetComponent<InventoryCollector>();
			if (collector == null || !rules.CanBePickedUpByCollision(collector)) return;
			if (interactions == null) { interactions = PickupInteractionsFor(collector); }

			UiGiverBase uiInterface = GetComponent<UiGiverBase>();
			if (uiInterface == null) {
				SetPickedUp(collector);
				return;
			}
			uiInterface.Invoke(gameObject);
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
