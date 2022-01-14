using NonStandard.Ui;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NonStandard.GameUi.Inventory {
	public class PickupButton : PickupBase {
		class PickupEvent {
			public InventoryCollector _collector;
			public InventoryItemObject _item;
			public PickupButton _rules;
			public float distanceLimit;
			public FloatingButton ui;
			public PickupEvent(PickupButton confirmRules, InventoryItemObject item, InventoryCollector collector) {
				_collector = collector;
				_item = item;
				_rules = confirmRules;
				FloatingButton original = Global.GetComponent<FloatingButton>();
				ui = Instantiate(original.gameObject).GetComponent<FloatingButton>();
				ui.transform.SetParent(original.transform.parent, false);
				Follow f = ui.GetComponent<Follow>();
				f.followTarget = _item.transform;
				f.disableWhenTargetDisables = _item.gameObject;
				Button btn = ui.GetComponentInChildren<Button>();
				EventBind.On(btn.onClick, _rules, nameof(_rules.OnClick), _collector);
				ui.gameObject.SetActive(true);
			}
			public void PickupItem(InventoryCollector collector) {
				if (collector != _collector) {
					Debug.LogWarning("unexpected collector..."+collector+", expected "+_collector);
				}
				_item.PickupConfirmBy(collector);
			}
			public void Start() {
				UiText.SetText(ui.gameObject, _item.item.name);
				distanceLimit = Vector3.Distance(_item.transform.position, _collector.transform.position) + _rules.cancelOnMoveAwayDistance;
			}
			public void Update() {
				if (_rules.cancelOnMoveAwayDistance >= 0) {
					float d = Vector3.Distance(_item.transform.position, _collector.transform.position);
					if (d > distanceLimit) {
						Cancel();
						return;
					}
				}
			}
			void Cancel() {
				Finish();
			}
			public void Finish() {
				Destroy(ui.gameObject);
				_rules.pickupEvent = null;
			}
		}
		PickupEvent pickupEvent;
		[Tooltip("-1 to never cancel")]
		public float cancelOnMoveAwayDistance = 1;
		[System.Serializable] public class UnityEvent_Collector : UnityEvent<InventoryCollector> { }
		public UnityEvent_Collector onClick = new UnityEvent_Collector();
		private void Reset() {
			EventBind.On(onClick, this, nameof(PickupItem));
		}
		override public void StartConfirmation(InventoryCollector collector) {
			InventoryItemObject item = GetComponent<InventoryItemObject>();
			if (pickupEvent == null) {
				pickupEvent = new PickupEvent(this, item, collector);
				pickupEvent.Start();
			}
		}
		public void FixedUpdate() {
			if (pickupEvent == null) { return; }
			pickupEvent.Update();
		}
		public void PickupItem(InventoryCollector collector) {
			pickupEvent.PickupItem(collector);
			pickupEvent.Finish();
		}
		public void OnClick(InventoryCollector collector) { onClick.Invoke(collector); }
		public void Finish(InventoryCollector collector) { Finish(); }
		public void Finish() { pickupEvent?.Finish(); }
	}
}
