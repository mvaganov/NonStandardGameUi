using NonStandard.Ui;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class ConfirmDialog : PickupConfirm {
		class PickupEvent {
			public InventoryCollector _collector;
			public InventoryItemObject _item;
			public ConfirmDialog _confirmRules;
			public float distanceLimit;
			public ModalConfirmation modal;
			public PickupEvent(ConfirmDialog confirmRules, InventoryItemObject item, InventoryCollector collector) {
				_collector = collector;
				_item = item;
				_confirmRules = confirmRules;
				modal = Global.GetComponent<ModalConfirmation>();
			}
			public void DoConfirm() {
				Debug.Log(_collector + " got it");
				_item.PickupConfirmBy(_collector);
			}
			public void Start() {
				modal.CancelOk("Pickup " + _item.item.name, DoConfirm, _item.item.icon);
				distanceLimit = Vector3.Distance(_item.transform.position, _collector.transform.position) * 1.125f;
			}
			public void Update() {
				if (_confirmRules.cancelOnMoveAway) {
					float d = Vector3.Distance(_item.transform.position, _collector.transform.position);
					if (d > distanceLimit) {
						modal.Hide();
					}
				}
			}
		}
		PickupEvent pickupEvent;
		public bool cancelOnMoveAway;
		override public void StartConfirmation(InventoryCollector collector) {
			InventoryItemObject item = GetComponent<InventoryItemObject>();
			pickupEvent = new PickupEvent(this, item, collector);
			pickupEvent.Start();
		}
		public void FixedUpdate() {
			if (pickupEvent == null) { return; }
			pickupEvent.Update();
		}
	}
}
