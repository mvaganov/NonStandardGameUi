using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.GameUi.Inventory {
	public class EventGiver : MonoBehaviour {
		[System.Serializable] public class UnityEvent_GameObject : UnityEvent<GameObject> { }
		public UnityEvent_GameObject _event = new UnityEvent_GameObject();
		public virtual void Invoke(GameObject source) { _event.Invoke(source); }
		public void Bind(object target, string methodName) {
			EventBind.On(_event, target, methodName);
		}
		public void Bind(object target, UnityAction<GameObject> method, GameObject source) {
			EventBind.On(_event, target, method, source);
		}
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
