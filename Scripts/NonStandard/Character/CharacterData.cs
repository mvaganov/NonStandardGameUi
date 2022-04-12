using NonStandard.Data;
using NonStandard.GameUi.Inventory;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Character {
	public class CharacterData : MonoBehaviour {
		[Tooltip("stats that belong to the character")]
		public ScriptedDictionary stats;
		[Tooltip("items that this character owns")]
		public Inventory inventory;
		[Tooltip("stats that belong to the character's local organization")]
		public ScriptedDictionary sharedStats;
		[Tooltip("items that this character's local organization owns (household, close friends, team)")]
		public Inventory sharedInventory;


		public void PopulateInventory(List<object> out_listing) {
			if (out_listing == null) { return; }
			inventory?.PopulateData(out_listing);
			sharedInventory?.PopulateData(out_listing);
		}
		public void PopulatePersonalInventory(List<object> out_listing) {
			if (out_listing == null) { return; }
			inventory?.PopulateData(out_listing);
		}
		public void PopulateStats(List<object> out_listing) {
			if (out_listing == null) { return; }
			stats?.PopulateData(out_listing);
			sharedStats?.PopulateData(out_listing);
		}
	}
}
