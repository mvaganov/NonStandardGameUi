using NonStandard.Ui;
using UnityEngine;
using UnityEngine.Events;

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
				progressBar.onComplete.AddListener(_rules.OnComplete); //EventBind.On(progressBar.onComplete, _rules, nameof(_rules.OnComplete)); //
				progressBar.onComplete.AddListener(_rules.Pickup); //EventBind.On(progressBar.onComplete, _rules, nameof(_rules.Pickup));// 
				progressBar.onCancel.AddListener(ItIsFinished); //EventBind.On(progressBar.onCancel, _rules, nameof(_rules.Cancel)); //
				UiText.SetText(progressBar.label, "pickup "+_item.item.name);
				progressBar.gameObject.SetActive(true);
			}
			public void Pickup() {
				_item.PickupConfirmBy(_collector);
				ItIsFinished();
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
			public void Cancel() {
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

		public PickupButton.UnityEvent_Collector onProgressComplete = new PickupButton.UnityEvent_Collector();
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
		public void Reset() {
			// if this is placed on an object with a pickup button, assume the progress should start on the button.
			PickupButton pButt = GetComponent<PickupButton>();
			if (pButt != null && pButt.onClick.GetPersistentEventCount() == 1 && 
				pButt.onClick.GetPersistentMethodName(0) == nameof(pButt.PickupItem) && pButt.onClick.GetPersistentTarget(0) == pButt) {
				EventBind.Clear(pButt.onClick);
				EventBind.On(pButt.onClick, this, nameof(StartConfirmation));
				EventBind.On(pButt.onClick, pButt, nameof(pButt.Finish));
				EventBind.On(onProgressComplete, pButt, nameof(pButt.Finish));
				cancelOnMoveAwayDistance = pButt.cancelOnMoveAwayDistance;
			}
		}
		public void OnComplete(InventoryCollector collector) { onProgressComplete.Invoke(collector); }
		public void OnComplete() { onProgressComplete.Invoke(pickupEvent._collector); }
		public void Pickup() { pickupEvent.Pickup(); }
		public void Cancel() { pickupEvent.Cancel(); }
	}
}
