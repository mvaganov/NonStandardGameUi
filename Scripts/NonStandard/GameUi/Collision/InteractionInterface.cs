// code by michael vaganov, released to the public domain via the unlicense (https://unlicense.org/)
using NonStandard.GameUi.DataSheet;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
			bool removed = false;
			for(int i = 0; i < interactable.interactions.Count; ++i) {
				Interaction interaction = interactable.interactions[i];
				interaction.onAction -= IntractionUsed;
				removed |= actions.Remove(interaction);
			}
			return actionsByInteractable.Remove(interactable) || removed;
		}

		public bool Remove(Interactable interactable, Interaction interaction) {
			interaction.onAction -= IntractionUsed;
			if (actionsByInteractable.TryGetValue(interactable, out List<Interaction> actionList)) {
				int index = actionList.IndexOf(interaction);
				if (index != -1) {
					actionList.RemoveAt(index);
					return true;
				}
			}
			RefreshInteractionUsability();
			return false;
		}

		public bool Remove(Interaction interaction) {
			interaction.onAction -= IntractionUsed;
			bool removed = false;
			foreach (KeyValuePair<Interactable, List<Interaction>> kvp in actionsByInteractable) {
				removed |= kvp.Value.Remove(interaction);
			}
			return actions.Remove(interaction) || removed;
		}

		public void Add(Interactable interactable, IList<Interaction> interactions) {
			if (interactions == null || interactions.Count == 0) { return; }
			if (interactable != null && actionsByInteractable.TryGetValue(interactable, out List<Interaction> actionList)) {
				Debug.Log("already got " + interactable);
				actionList.ForEach(i => {
					i.onAction -= IntractionUsed;
					i.onAction += IntractionUsed;
				});
				actionList.AddRange(interactions);
				return;
			}
			List<Interaction> toAdd = new List<Interaction>(interactions);
			toAdd.ForEach(i => {
				i.onAction -= IntractionUsed;
				i.onAction += IntractionUsed;
			});
			actionsByInteractable[interactable] = toAdd;
			actions.AddRange(toAdd);
			//Debug.Log("ooh, +" + interactions.Count + " : " + actions.JoinToString(", ", i => i.text));
			Sort();
			dataSheet.Refresh();
			RefreshInteractionUsability();
		}

		private void IntractionUsed(Interaction interaction) {
			switch (interaction.howItIsRemoved) {
				case Interaction.HowItIsRemoved.Consumed:
					Remove(interaction);
					dataSheet.Refresh();
					break;
			}
			RefreshInteractionUsability();
		}

		public void RefreshInteractionUsability() {
			for (int i = 0; i < actions.Count; i++) {
				Interaction a = actions[i];
				DataSheetRow rowUi = dataSheet.RowUi(a);
				if (rowUi == null) {
					Debug.LogWarning("no UI for " + a.text + "?");
					continue;
				}
				System.Array.ForEach(rowUi.GetComponentsInChildren<Button>(), b=>b.interactable = a.IsActivatable());
			}
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
