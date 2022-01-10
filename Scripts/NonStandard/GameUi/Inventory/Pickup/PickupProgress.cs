using NonStandard.Ui;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class PickupProgress : PickupConfirm {
		class PickupEvent {
			public InventoryCollector _collector;
			public InventoryItemObject _item;
			public PickupProgress _confirmRules;
			public float distanceLimit;
			public ProgressBar progressBar;
			public PickupEvent(PickupProgress confirmRules, InventoryItemObject item, InventoryCollector collector) {
				_collector = collector;
				_item = item;
				_confirmRules = confirmRules;
				progressBar = Global.GetComponent<ProgressBar>();
			}
			public void DoConfirm() {
				Debug.Log(_collector + " got it");
				_item.PickupConfirmBy(_collector);
			}
			public void Start() {
				progressBar.Show();
				Debug.LogError("create code that makes the progress bar increment over time, with variables set in PickupProgress");
				distanceLimit = Vector3.Distance(_item.transform.position, _collector.transform.position) * 1.125f;
			}
			public void Update() {
				if (_confirmRules.cancelOnMoveAway) {
					float d = Vector3.Distance(_item.transform.position, _collector.transform.position);
					if (d > distanceLimit) {
						progressBar.Cancel();
					}
				}
				Debug.LogError("increment the progress bar over time!");
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
