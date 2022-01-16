using NonStandard.Ui;
using UnityEngine;
using UnityEngine.UI;

namespace NonStandard.GameUi.Inventory {
	public class ButtonGiver : EventGiver {
		class EventData {
			public GameObject _instigator;
			public ButtonGiver _buttonGiver;
			public float distanceLimit;
			public FloatingButton ui;
			public EventData(ButtonGiver buttonGiver, GameObject instigator, Sprite icon = null) {
				_instigator = instigator;
				_buttonGiver = buttonGiver;
				FloatingButton original = Global.GetComponent<FloatingButton>();
				ui = Instantiate(original.gameObject).GetComponent<FloatingButton>();
				ui.transform.SetParent(original.transform.parent, false);
				Follow f = ui.GetComponent<Follow>();
				f.followTarget = _buttonGiver.transform;
				f.disableWhenTargetDisables = _buttonGiver.gameObject;
				Button btn = ui.GetComponentInChildren<Button>();
				EventBind.On(btn.onClick, _buttonGiver, nameof(_buttonGiver.Confirm));
				UiText.SetText(btn.gameObject, _buttonGiver.buttonText);
				if (icon != null) {
					Component c = UiImage.SetSprite(btn.gameObject, icon);
					if (c != null) {
						c.gameObject.SetActive(true);
					}
				}
				ui.gameObject.SetActive(true);
			}
			public void Start() {
				distanceLimit = Vector3.Distance(_buttonGiver.transform.position, _instigator.transform.position) + _buttonGiver.cancelOnMoveAwayDistance;
			}
			public void Update() {
				if (_buttonGiver.cancelOnMoveAwayDistance >= 0) {
					float d = Vector3.Distance(_buttonGiver.transform.position, _instigator.transform.position);
					if (d > distanceLimit) {
						_buttonGiver.Cancel();
						return;
					}
				}
			}
			public void Finish() {
				Destroy(ui.gameObject);
				_buttonGiver.popupButtonData = null;
			}
		}
		public UnityEvent_GameObject _cancelEvent;
		EventData popupButtonData;
		[Tooltip("-1 to never cancel")]
		public float cancelOnMoveAwayDistance = 1;
		public string buttonText;
		override public void Invoke(GameObject collider) {
			if (popupButtonData != null) { return; }
			Sprite icon = null;
			InventoryItemObject item = GetComponent<InventoryItemObject>();
			if (item != null) {
				icon = item.item.icon;
			}
			popupButtonData = new EventData(this, collider, icon);
			popupButtonData.Start();
		}
		public void FixedUpdate() {
			if (popupButtonData == null) { return; }
			popupButtonData.Update();
		}
		public GameObject Instigator => popupButtonData != null ? popupButtonData._instigator : null;
		public void Confirm() {
			_event.Invoke(Instigator);
			Finish();
		}
		public void Cancel() {
			_cancelEvent.Invoke(Instigator);
			Finish();
		}
		public void Finish() { popupButtonData?.Finish(); }
		public void Finish(GameObject instigator) { Finish(); }
#if UNITY_EDITOR
		public override void Reset() {
			base.Reset();
			InventoryItemObject item = GetComponent<InventoryItemObject>();
			if (item != null) {
				buttonText = item.item.name;
			}
		}
#endif
	}
}
