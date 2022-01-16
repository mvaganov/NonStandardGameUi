using NonStandard.GameUi.Inventory;
using NonStandard.Ui;
using UnityEngine;

namespace NonStandard.GameUi {
	public class ProgressGiver : UiGiverBase {
		class EventData {
			public GameObject _instigator;
			public ProgressGiver _progressGiver;
			public float distanceLimit;
			public ProgressBar ui;
			float timer;
			public bool active = true;
			public EventData(ProgressGiver progressGiver, GameObject instigator) {
				_instigator = instigator;
				_progressGiver = progressGiver;
				ProgressBar original = Global.GetComponent<ProgressBar>();
				ui = Instantiate(original.gameObject).GetComponent<ProgressBar>();
				Follow follow = null;
				if (_progressGiver.cancelOnMoveAwayDistance >= 0) {
					follow = ui.gameObject.AddComponent<Follow>();
					follow.followTarget = _progressGiver.transform;
				}
				UiText.SetText(ui.label, _progressGiver.progressText);
				EventBind.Clear(ui.onComplete);
				//ui.onComplete.AddListener(_progressGiver.OnComplete); //EventBind.On(progressBar.onComplete, _rules, nameof(_rules.OnComplete)); //
				ui.onComplete.AddListener(_progressGiver.Confirm); //EventBind.On(progressBar.onComplete, _rules, nameof(_rules.Pickup));// 
				ui.onCancel.AddListener(_progressGiver.Cancel); //EventBind.On(progressBar.onCancel, _rules, nameof(_rules.Cancel)); //
				ui.transform.SetParent(original.transform.parent, false);
				ui.gameObject.SetActive(true);
			}
			public void Start() {
				ui.Show();
				distanceLimit = Vector3.Distance(_progressGiver.transform.position, _instigator.transform.position) + _progressGiver.cancelOnMoveAwayDistance;
			}
			public void Update() {
				if (_progressGiver.cancelOnMoveAwayDistance >= 0) {
					float d = Vector3.Distance(_progressGiver.transform.position, _instigator.transform.position);
					if (d > distanceLimit) {
						_progressGiver.Cancel();
						return;
					}
				}
				timer += Time.deltaTime;
				float p = timer / _progressGiver.duration;
				ui.Progress = p;
			}
			public void Finish() {
				timer = 0;
				Destroy(ui.gameObject);
				_progressGiver.progressionData = null;
			}
			public bool IsDone => timer >= _progressGiver.duration;
		}
		public UnityEvent_GameObject _cancelEvent;
		EventData progressionData;
		[Tooltip("-1 to never cancel")]
		public float cancelOnMoveAwayDistance = 1;
		public bool resetOnCancel = true;
		public float duration = 3;
		public string progressText;

		override public void Invoke(GameObject collider) {
			if (progressionData != null) { return; }
			//Sprite icon = null;
			//InventoryItemObject item = GetComponent<InventoryItemObject>();
			//if (item != null) {
			//	icon = item.item.icon;
			//}
			progressionData = new EventData(this, collider);
			progressionData.Start();
		}
		public void FixedUpdate() {
			if (progressionData == null || !progressionData.active) { return; }
			progressionData.Update();
			if (progressionData != null && progressionData.IsDone) { Confirm(); }
		}
		public GameObject Instigator => progressionData != null ? progressionData._instigator : null;
		public void Confirm() {
			_event.Invoke(Instigator);
			Finish();
		}
		public void Cancel() {
			_cancelEvent.Invoke(Instigator);
			Finish();
		}
		public void Finish() { progressionData?.Finish(); }

#if UNITY_EDITOR
		public override void Reset() {
			base.Reset();
			// if this progress bar is for an item, use that item name as progress text
			InventoryItemObject item = GetComponent<InventoryItemObject>();
			if (item != null) {
				progressText = "pickup " + item.item.name;
				// if this is placed on an object with a button
				ButtonGiver pButt = GetComponent<ButtonGiver>();
				if (pButt != null && pButt._event.GetPersistentEventCount() == 1) {
					// and that button picks up this item
					if (pButt._event.GetPersistentMethodName(0) == nameof(item.SetPickedUp) && pButt._event.GetPersistentTarget(0) == item) {
						// assume the button should start this progress instead, the end of the progress will pickup the button.
						EventBind.Clear(pButt._event);
						EventBind.On(pButt._event, this, nameof(Invoke));
						// when the progress bar finishes, also finish the button
						EventBind.On(_event, pButt, nameof(pButt.Finish));
						cancelOnMoveAwayDistance = pButt.cancelOnMoveAwayDistance;
					}
				}
			}
		}
#endif
	}
}
