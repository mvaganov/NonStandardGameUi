// code by michael vaganov, released to the public domain via the unlicense (https://unlicense.org/)
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class InventoryItemObject : Interactable {
		[ContextMenuItem(nameof(GeneratePickup), nameof(GeneratePickup))]
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

		public void GeneratePickup() {
			interactions.AddRange(PickupInteractionsForSelf());
		}

		public void PickupItem(Effort interaction) {
			//Debug.Log("TODO pick up item, and remove this interaction from the interaction listing.");
			InventoryItemObject invObj = interaction.act.source as InventoryItemObject;
			if (invObj == null) {
				throw new Exception("can't pick up " + interaction.act.source + " as " + nameof(InventoryItemObject));
			}
			InventoryCollector collector = interaction.actor as InventoryCollector;
			if (collector == null) {
				throw new Exception("can't collect inventory with " + interaction.actor);
			}
			invObj.SetPickedUp(collector);
		}

		// TODO rename CreatePickupWaysOfActing
		public List<WayOfActing> PickupInteractionsForSelf() {
			List<WayOfActing> interactions = new List<WayOfActing>();
			interactions.Add(new WayOfActing(this, item.name, item.icon, 700,
				//this, nameof(PickupItem),
				new EventBind(this, nameof(PickupItem))));
			return interactions;
		}

		public void PickupRequestBy(GameObject gameObject) {
			InventoryCollector collector = gameObject.GetComponent<InventoryCollector>();
			if (collector == null || !rules.CanBePickedUpByCollision(collector)) return;
			//if (interactions == null) { interactions = PickupInteractionsFor(); }

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
