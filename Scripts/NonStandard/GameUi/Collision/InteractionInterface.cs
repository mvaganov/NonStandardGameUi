using NonStandard.GameUi.DataSheet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.GameUi {
	public class InteractionInterface : MonoBehaviour {
		public DataSheetWindow dataSheet;

		// TODO reordering the list in the DataSheet UI should reorder this list.
		public List<Interaction> actions = new List<Interaction>();

		public Dictionary<Interactable, List<Interaction>> actionsByInteractable =
			new Dictionary<Interactable,List<Interaction>>();

		public void Add(Interactable interactable) {
			if(actionsByInteractable.TryGetValue(interactable, out List<Interaction> actionList)) {
				return;
			}
			actionsByInteractable[interactable] = interactable.interactions;
			actions.AddRange(interactable.interactions);
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
