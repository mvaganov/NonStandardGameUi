using NonStandard.Ui;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class ConfirmDialogGiver : EventGiver {
		class EventData {
			public GameObject _instigator;
			public ConfirmDialogGiver _dialogGiver;
			public float distanceLimit;
			public ModalConfirmation ui;
			public EventData(ConfirmDialogGiver dialogGiver, GameObject instigator, Sprite icon = null) {
				_instigator = instigator;
				_dialogGiver = dialogGiver;
				ModalConfirmation original = Global.GetComponent<ModalConfirmation>();
				ui = Instantiate(original.gameObject).GetComponent<ModalConfirmation>();
				ui.CancelOk(_dialogGiver.confirmDialogMessage, _dialogGiver.Cancel, _dialogGiver.Confirm, icon);
				ui.transform.SetParent(original.transform.parent, false);
				ui.gameObject.SetActive(true);
			}
			public void Start() {
				distanceLimit = Vector3.Distance(_dialogGiver.transform.position, _instigator.transform.position) + _dialogGiver.cancelOnMoveAwayDistance;
			}
			public void Update() {
				if (_dialogGiver.cancelOnMoveAwayDistance >= 0) {
					float d = Vector3.Distance(_dialogGiver.transform.position, _instigator.transform.position);
					if (d > distanceLimit) {
						_dialogGiver.Cancel();
						return;
					}
				}
			}
			public void Finish() {
				Destroy(ui.gameObject);
				_dialogGiver.confirmDialogData = null;
			}
		}
		public UnityEvent_GameObject _cancelEvent;
		EventData confirmDialogData;
		[Tooltip("-1 to never cancel regardless of distance")]
		public float cancelOnMoveAwayDistance = -1;
		public string confirmDialogMessage;
		override public void Invoke(GameObject collider) {
			if (confirmDialogData != null) { return; }
			Sprite icon = null;
			InventoryItemObject item = GetComponent<InventoryItemObject>();
			if (item != null) {
				icon = item.item.icon;
			}
			confirmDialogData = new EventData(this, collider, icon);
			confirmDialogData.Start();
		}
		public void FixedUpdate() {
			if (confirmDialogData == null) { return; }
			confirmDialogData.Update();
		}
		public GameObject Instigator => confirmDialogData != null ? confirmDialogData._instigator : null;
		public void Confirm() {
			_event.Invoke(Instigator);
			Finish();
		}
		public void Cancel() {
			_cancelEvent.Invoke(Instigator);
			Finish();
		}
		public void Finish() { confirmDialogData?.Finish(); }

#if UNITY_EDITOR
		public override void Reset() {
			base.Reset();
			InventoryItemObject item = GetComponent<InventoryItemObject>();
			if (item != null) {
				confirmDialogMessage = "Pick up "+item.item.name;
			}
		}
#endif
	}
}
