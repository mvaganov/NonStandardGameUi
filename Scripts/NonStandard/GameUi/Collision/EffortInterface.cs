// code by michael vaganov, released to the public domain via the unlicense (https://unlicense.org/)
using NonStandard.Data;
using NonStandard.Extension;
using NonStandard.GameUi.DataSheet;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NonStandard.GameUi {
	/// <summary>
	/// stores efforts that are displayed in the UI. Efforts can be done to a thing by the owner of this interface.
	/// A <see cref="Effort"/> object is an action on a subject done by an actor.
	/// A <see cref="WayOfActing"/> object is an action on a subject without an actor to do the action
	/// A <see cref="Interactable"/> object can carry <see cref="WayOfActing"/> objects, which it is the subject of
	/// </summary>
	public class EffortInterface : MonoBehaviour {
		public DataSheetWindow dataSheet;
		public UnityEngine.Object owner;

		public List<Effort> efforts = new List<Effort>();

		public Dictionary<Interactable, List<Effort>> effortsByThing = new Dictionary<Interactable,List<Effort>>();

		/// <summary>
		/// removes every specific effort ready to be done on the given interactable
		/// </summary>
		/// <param name="interactable"></param>
		/// <returns></returns>
		public bool Remove(Interactable interactable) {
			bool removed = false;
			for(int i = 0; i < interactable.interactions.Count; ++i) {
				WayOfActing interaction = interactable.interactions[i];
				int index = SpecificEffortIndex(interaction);
				if (index >= 0) {
					//Debug.Log(index);
					Effort effort = efforts[index];
					effort.onAction -= IntractionUsed;
					efforts.RemoveAt(index);
					removed = true;
				//} else {
				//	Debug.Log(interaction+" not in "+efforts.Stringify());
				}
			}
			//if (!removed) {
			//	Debug.Log(interactable+" not found in "+ efforts.Stringify());
			//}
			return effortsByThing.Remove(interactable) || removed;
		}

		public void RefreshUi() {
			dataSheet.Refresh();
		}

		public int SpecificEffortIndex(WayOfActing generalWayOfActing) {
			return efforts.FindIndex(eff => eff.act == generalWayOfActing);
		}

		public bool Remove(Interactable interactable, WayOfActing wayOfActing) {
			int index = SpecificEffortIndex(wayOfActing);
			if (index >= 0) {
				efforts[index].onAction -= IntractionUsed;
			}
			if (effortsByThing.TryGetValue(interactable, out List<Effort> actionList)) {
				index = actionList.FindIndex(e => e.act == wayOfActing);
				if (index != -1) {
					actionList.RemoveAt(index);
					return true;
				}
			}
			RefreshInteractionUsability();
			return false;
		}
		public bool Remove(WayOfActing wayOfActing) {
			int index = SpecificEffortIndex(wayOfActing);
			if (index < 0) return false;
			return Remove(efforts[index]);
		}
		public bool Remove(Effort effort) {
			int index = efforts.IndexOf(effort);
			bool removed = false;
			if (index >= 0) {
				efforts[index].onAction -= IntractionUsed;
				Debug.Log(" ~~ removed " + index);
				efforts.RemoveAt(index);
				removed = true;
			}
			foreach (KeyValuePair<Interactable, List<Effort>> kvp in effortsByThing) {
				index = kvp.Value.IndexOf(effort);
				if (index == -1) { continue; }
				kvp.Value[index].onAction -= IntractionUsed;
				kvp.Value.RemoveAt(index);
				removed = true;
			}
			return removed;
		}

		public List<Effort> CreateEffortBy(IList<WayOfActing> acts, object actor) {
			List<Effort> efforts = new List<Effort>();
			for (int i = 0; i < acts.Count; i++) {
				efforts.Add(acts[i].ActedBy(actor));
			}
			return efforts;
		}

		public void Add(Interactable theThing, IList<WayOfActing> interactions) {
			if (interactions == null || interactions.Count == 0) { return; }
			if (theThing != null && effortsByThing.TryGetValue(theThing, out List<Effort> effortsForTheThing)) {
				//Debug.Log("already got " + theThing);
				effortsForTheThing.ForEach(i => {
					i.onAction -= IntractionUsed;
					i.onAction += IntractionUsed;
				});
				effortsForTheThing.AddRange(CreateEffortBy(interactions, owner));
				return;
			}
			List<Effort> toAdd = new List<Effort>(CreateEffortBy(interactions, owner));
			toAdd.ForEach(i => {
				i.onAction -= IntractionUsed;
				i.onAction += IntractionUsed;
			});
			effortsByThing[theThing] = toAdd;
			efforts.AddRange(toAdd);
			//Debug.Log("ooh, +" + interactions.Count + " : " + actions.JoinToString(", ", i => i.text));
			Sort();
			dataSheet.Refresh();
			RefreshInteractionUsability();
		}

		private void IntractionUsed(Effort interaction) {
			switch (interaction.act.howItIsRemoved) {
				case WayOfActing.HowItIsRemoved.Consumed:
					if (interaction.isFinished) {
						Remove(interaction);
					} else {
						interaction.invalid = false;
					}
					dataSheet.Refresh();
					break;
			}
			RefreshInteractionUsability();
		}

		public void RefreshInteractionUsability() {
			for (int i = 0; i < efforts.Count; i++) {
				Effort effort = efforts[i];
				DataSheetRow rowUi = dataSheet.RowUi(effort);
				if (rowUi == null) {
					//Debug.LogWarning("UI not yet loaded for " + effort.act.text + "?");
					continue;
				}
				System.Array.ForEach(rowUi.GetComponentsInChildren<Button>(), b=>b.interactable = effort.IsActivatable());
			}
		}

		public void Update() {
			for (int i = 0; i < efforts.Count; i++) {
				Effort effort = efforts[i];
				effort.Update();
				DataSheetRow rowUi = dataSheet.RowUi(effort);
				if (rowUi == null) {
					//Debug.LogWarning("UI not yet loaded for " + effort.act.text + "?");
					continue;
				}
				bool activatable = effort.IsActivatable();
				if (effort.activatableLastFrame != activatable) {
					System.Array.ForEach(rowUi.GetComponentsInChildren<Button>(), b => b.interactable = activatable);
					effort.activatableLastFrame = activatable;
				}
			}
		}


		private int CompareInteractions(Effort a, Effort b) {
			return a.act.priority.CompareTo(b.act.priority);
		}

		public void Sort() {
			efforts.Sort(CompareInteractions);
		}

		public void PopulateData(List<object> out_data) {
			if (out_data == null) { return; }
			out_data.AddRange(efforts);
		}

		public void NotifyReorder(List<RowData> reorderd) {
			UnityDataSheet.NotifyReorder(reorderd, efforts);
		}
	}
}
