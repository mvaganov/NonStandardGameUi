using System.Collections;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class PickupBase : MonoBehaviour {
		public virtual void StartConfirmation(InventoryCollector collector) {
			GetComponent<InventoryItemObject>().PickupConfirmBy(collector);
		}
	}
}
