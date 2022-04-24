// code by michael vaganov, released to the public domain via the unlicense (https://unlicense.org/)
using NonStandard.Extension;
using NonStandard.GameUi.DataSheet;
using NonStandard.GameUi.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.GameUi {
	/// <summary>
	/// stores interactions that are displayed in the UI
	/// </summary>
	public class InteractionInterface : MonoBehaviour {
		public DataSheetWindow dataSheet;

		// TODO reordering the list in the DataSheet UI should reorder this list.
		public List<Interaction> actions = new List<Interaction>();

		public Dictionary<Interactable, List<Interaction>> actionsByInteractable =
			new Dictionary<Interactable,List<Interaction>>();

		public bool Remove(Interactable interactable) {
			return actionsByInteractable.Remove(interactable);
		}
		public bool Remove(Interactable interactable, Interaction interaction) {
			if (actionsByInteractable.TryGetValue(interactable, out List<Interaction> actionList)) {
				int index = actionList.IndexOf(interaction);
				if (index != -1) {
					actionList.RemoveAt(index);
					return true;
				}
			}
			return false;
		}

		public void Add(Interactable interactable, IList<Interaction> interactions) {
			if (interactions == null || interactions.Count == 0) { return; }
			if (actionsByInteractable.TryGetValue(interactable, out List<Interaction> actionList)) {
				Debug.Log("already got " + interactable);
				actionList.AddRange(interactions);
				return;
			}
			List<Interaction> toAdd = new List<Interaction>(interactions);
			actionsByInteractable[interactable] = toAdd;
			actions.AddRange(toAdd);
			//Debug.Log("ooh, +" + interactions.Count + " : " + actions.JoinToString(", ", i => i.text));
			Sort();
			dataSheet.Refresh();
		}

		private int CompareInteractions(Interaction a, Interaction b) {
			return a.priority.CompareTo(b.priority);
		}

		public void Sort() {
			actions.Sort(CompareInteractions);
		}

		public void PopulateData(List<object> out_data) {
			if (out_data == null) { return; }
			out_data.AddRange(actions);
		}
	}
}
