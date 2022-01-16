using System.Collections.Generic;
using UnityEngine;

public class SlotsForItems : MonoBehaviour {
	public class Slot {
		public string name;
		public int slotIndex;
		public SlottedItem[] items;
		public Slot(string name, int count) {
			this.name = name;
			this.items = new SlottedItem[count];
		}
		public bool HasEmptySlot(out int index) {
			for (int i = 0; i < items.Length; i++) {
				if (items[i] == null) {
					index = i;
					return true;
				}
			}
			index = -1;
			return false;
		}
		public bool Equip(SlottedItem item) {
			for (int i = 0; i < items.Length; i++) {
				if (items[i] == null) { return Equip(item, i); }
			}
			return false;
		}
		public bool Equip(SlottedItem item, int index) {
			if (items[index] == null) {
				items[index] = item;
				return true;
			}
			return false;
		}
	}
	public static readonly Slot[] DefaultSlots = new Slot[]{
		new Slot("head", 1),
		new Slot("armor", 1),
		new Slot("hand", 2),
	};
	public Dictionary<string, Slot> slots = new Dictionary<string, Slot>();
	public void SetSlotsToDefault() {
		slots.Clear();
		foreach (Slot s in DefaultSlots) {
			slots.Add(s.name, s);
		}
	}
	public bool CanEquip(SlottedItem item, out Slot slot, out int index) {
		if (item == null) {
			slot = null;
			index = -1;
			return false;
		}
		if (!slots.TryGetValue(item.name, out slot)) {
			index = -1;
			return false;
		}
		return slot.HasEmptySlot(out index);
	}
	public bool Equip(SlottedItem item) {
		if (!CanEquip(item, out Slot slot, out int index)) {
			return false;
		}
		return slot.Equip(item, index);
	}
}
