// code by michael vaganov, released to the public domain via the unlicense (https://unlicense.org/)
using NonStandard.Data;
using NonStandard.GameUi.DataSheet;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Character {
	public class CharacterSwitcher : MonoBehaviour {
		public List<Root> Characters = new List<Root>();
		public void DataPopulator(List<object> data) {
			data.AddRange(Characters);
		}
		public void NotifyReorder(List<RowData> reordered) {
			//Debug.Log("new order " + Extension.StringifyExtension.JoinToString(reordered, ", ", r => (r.obj as Root).name));
			UnityDataSheet.NotifyReorder(reordered, Characters);
		}
	}
}