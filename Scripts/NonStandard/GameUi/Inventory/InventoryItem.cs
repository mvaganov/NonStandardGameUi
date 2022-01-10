using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.GameUi.Inventory {
	[System.Serializable]
	public class InventoryItem {
		[SerializeField] public string _name;
		public Sprite icon;
		[System.Serializable]
		public class SpecialBehavior {
			[System.Serializable] public class UnityEvent_object : UnityEvent<object> { }
			public UnityEvent_object onAdd = new UnityEvent_object();
			public UnityEvent_object onRemove = new UnityEvent_object();
		}
		public SpecialBehavior inventoryAddBehavior;
		/// <summary>
		/// which inventory this item believes it belongs to
		/// </summary>
		[HideInInspector] public Inventory currentInventory;
		/// <summary>
		/// if this item has a GameObject avatar, this field will be filled in
		/// </summary>
		[HideInInspector] public InventoryItemObject component;
		/// <summary>
		/// core object data. should be automatically set by the kind of object that this is, eg: <see cref="NonStandard.GameUi.Inventory.InventoryItemWeapon"/>
		/// </summary>
		[HideInInspector] public object data;
		/// <summary>
		/// mark that removes drop option, which allows player to remove the item from their inventory
		/// </summary>
		public bool nodrop = false;
		public string name {
			get => _name;
			set {
				_name = value;
				if (component != null) { component.name = _name; }
			}
		}
		public InventoryItemObject GetItemObject() {
			return component;
		}
		public Transform GetTransform() {
			return (component != null) ? component.transform : null;
		}
		public void Drop() {
			if (currentInventory == null) { return; }
			inventoryAddBehavior?.onRemove?.Invoke(this);
			currentInventory.RemoveItem(this);
			currentInventory = null;
		}
		public void AddToInventory(Inventory inventory) {
			if (this.currentInventory == inventory) {
				Debug.LogWarning(name+" being added to "+inventory.name+" again");
				return; // prevent double-add
			}
			Drop();
			this.currentInventory = inventory;
			inventory.AddItem(this);
			inventoryAddBehavior?.onAdd?.Invoke(this);
		}
		public void SetPickedUpBy(InventoryCollector inv) {
			//Debug.Log("item hits "+other);
			if (inv != null && inv.autoPickup && inv.inventory != null) {
				inv.AddItem(this);
			}
		}
	}
}
