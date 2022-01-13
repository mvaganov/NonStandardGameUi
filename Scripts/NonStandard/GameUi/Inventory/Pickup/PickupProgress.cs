using NonStandard.Ui;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class PickupProgress : PickupBase {
		class PickupEvent {
			public InventoryCollector _collector;
			public InventoryItemObject _item;
			public PickupProgress _rules;
			public float distanceLimit;
			public ProgressBar progressBar;
			float timer;
			public bool active = true;
			public PickupEvent(PickupProgress confirmRules, InventoryItemObject item, InventoryCollector collector) {
				_collector = collector;
				_item = item;
				_rules = confirmRules;
				ProgressBar original = Global.GetComponent<ProgressBar>();
				progressBar = Instantiate(original.gameObject).GetComponent<ProgressBar>();
				progressBar.transform.SetParent(original.transform.parent, false);
				Follow follow = null;
				if (_rules.cancelOnMoveAwayDistance >= 0) {
					follow = progressBar.gameObject.AddComponent<Follow>();
					follow.followTarget = _item.transform;
				}
				EventBind.Clear(progressBar.onComplete);
				progressBar.onComplete.AddListener(DoConfirm);
				progressBar.onCancel.AddListener(ItIsFinished);
				UiText.SetText(progressBar.label, "pickup "+_item.item.name);
				progressBar.gameObject.SetActive(true);
			}
			public void DoConfirm() {
				_item.PickupConfirmBy(_collector);
				Destroy(progressBar.gameObject);
			}
			public void Start() {
				progressBar.Show();
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
				timer += Time.deltaTime;
				float p = timer / _rules.duration;
				progressBar.Progress = p;
			}
			void ItIsFinished() {
				if (_rules.resetOnCancel) {
					timer = 0;
					_rules.pickupEvent = null;
					Destroy(progressBar.gameObject);
				}
				active = false;
			}
			void Cancel() {
				progressBar.Cancel();
				ItIsFinished();
			}
			public bool IsDone => timer > _rules.duration;
		}
		PickupEvent pickupEvent;
		[Tooltip("-1 to never cancel")]
		public float cancelOnMoveAwayDistance = -1;
		public bool resetOnCancel = true;
		public float duration = 3;
		override public void StartConfirmation(InventoryCollector collector) {
			InventoryItemObject item = GetComponent<InventoryItemObject>();
			if (pickupEvent != null) {
				pickupEvent.active = true;
			} else {
				pickupEvent = new PickupEvent(this, item, collector);
			}
			pickupEvent.Start();
		}
		public void FixedUpdate() {
			if (pickupEvent == null || !pickupEvent.active) { return; }
			pickupEvent.Update();
			if (pickupEvent != null && pickupEvent.IsDone) { pickupEvent = null; }
		}
	}
}
