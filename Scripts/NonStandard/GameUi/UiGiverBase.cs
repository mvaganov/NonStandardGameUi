using NonStandard.GameUi.Inventory;
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.GameUi {
	public class UiGiverBase : MonoBehaviour {
		[System.Serializable] public class UnityEvent_GameObject : UnityEvent<GameObject> { }
		[UnityEngine.Tooltip("The activation purpose of this UI")]
		public UnityEvent_GameObject _event = new UnityEvent_GameObject();
		public virtual void Invoke(GameObject source) { _event.Invoke(source); }
#if UNITY_EDITOR
		public virtual void Reset() {
			InventoryItemObject item = GetComponent<InventoryItemObject>();
			if (item != null) {
				EventBind.On(_event, item, nameof(item.SetPickedUp));
			}
		}
#endif
	}
}
