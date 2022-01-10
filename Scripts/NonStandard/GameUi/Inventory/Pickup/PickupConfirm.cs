using System.Collections;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class PickupConfirm : MonoBehaviour {
		public virtual void StartConfirmation(InventoryCollector collector) {
			GetComponent<InventoryItemObject>().PickupConfirmBy(collector);
		}
	}
}
