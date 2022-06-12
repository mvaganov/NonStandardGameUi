using NonStandard.Data;
using NonStandard.Extension;
using NonStandard.GameUi.DataSheet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO
/// sits between the base stats dictionary and game logic/UI
/// </summary>
public class StatFilter : MonoBehaviour, IDictionary<string, object> {
	public ScriptedDictionary baseDictionary;
	[SerializeField] private ScriptedDictionary.KvpChangeEvent valueChangeListener = new ScriptedDictionary.KvpChangeEvent();
	[Tooltip("Adjustment listing. Please edit this with " + nameof(AddAdjustment) + " and " + nameof(RemoveAdjustmentSourcedFrom))]
	public List<Adjustment> adjustments = new List<Adjustment>();
	public Dictionary<string, List<Adjustment>> adjustmentsDict = new Dictionary<string, List<Adjustment>>();

	public ICollection<string> Keys => throw new System.NotImplementedException();

	public ICollection<object> Values => throw new System.NotImplementedException();

	public int Count => throw new System.NotImplementedException();

	public bool IsReadOnly => throw new System.NotImplementedException();

	public object this[string key] { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

	[System.Serializable]
	public class Alteration {
		public string name;
		/// <summary>
		/// icon displayed for this adjustment
		/// </summary>
		public Sprite icon;
		public Color spriteColor;
		/// <summary>
		/// what created this adjustment?
		/// </summary>
		public object sourceAgent;
		/// <summary>
		/// what kind of bonus? enhancement? morale? circumstance?
		/// </summary>
		public string kind;
		public List<Kvp> adjustments;
		[NonSerialized] private Dictionary<string,Adjustment> _adjustments;
		public Dictionary<string, Adjustment> Adjustments {
			get {
				if (_adjustments == null && adjustments != null) {
					_adjustments = new Dictionary<string, Adjustment>();
					for (int i = 0; i < adjustments.Count; i++) {
						_adjustments[adjustments[i].stat] = new Adjustment(adjustments[i].stat, float.Parse(adjustments[i].adjustment), this);
					}
				}
				return _adjustments;
			}
		}
		[System.Serializable] public struct Kvp { public string stat; public string adjustment; }
	}
	public class Adjustment : Computable<string, object>, IComparable<Adjustment> {
		public Alteration source;

		public Adjustment(string key, object value, Alteration source) : base(key, value) {
			this.source = source;
		}
		public int KindCompareTo(Adjustment other) => KindCompareTo(this, other);
		public static int KindCompareTo(Adjustment a, Adjustment b) {
			if (a.source == null && b.source == null) return 0;
			if (a.source != null && b.source == null) return -1;
			if (a.source == null && b.source != null) return 1;
			return a.source.kind.CompareTo(b.source.kind);
		}
		public bool Equals(Adjustment other) {
			return source == other.source && KindCompareTo(other) == 0 && _key == other._key && _val == other._val;
		}

		public int CompareTo(Adjustment other) {
			int i = key.CompareTo(other.key);
			if (i != 0) return i;
			i = KindCompareTo(other);
			if (i != 0) return i;
			if (source is IComparable thisSource) {
				i = thisSource.CompareTo(other.source);
				if (i != 0) return i;
			}
			if (value is IComparable comparable) {
				i = comparable.CompareTo(other.value);
				if (i != 0) return i;
			}
			return 0;
		}

		public static int Comparer(Adjustment a, Adjustment b) => a.CompareTo(b);

		public override bool Equals(object obj) {
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public override string ToString() {
			return base.ToString();
		}
	}

	public void AddAdjustment(Alteration adjustmentGroup) {
		foreach(var kvp in adjustmentGroup.Adjustments) {
			AddAdjustment(kvp.Value);
		}
	}

	public void AddAdjustment(Adjustment adjustment) {
		object oldValue = null;
		bool notifyChange = valueChangeListener.GetPersistentEventCount() > 0;
		if (notifyChange) {
			TryGetValue(adjustment.key, out oldValue);
		}
		AddAdjustmentNoNotify(adjustment);
		if (notifyChange) {
			TryGetValue(adjustment.key, out object newValue);
			valueChangeListener.Invoke(adjustment.key, oldValue, newValue);
		}
	}
	public void AddAdjustmentNoNotify(Adjustment adjustment) {
		if (!baseDictionary.ContainsKey(adjustment.key)) {
			throw new Exception("can't add modifier to '" + adjustment.key + "', does not exist in base dictionary");
		}
		if (!adjustmentsDict.TryGetValue(adjustment.key, out List<Adjustment> list)) {
			list = new List<Adjustment>();
			adjustmentsDict[adjustment.key] = list;
		}
		int index = list.BinarySearchIndexOf(adjustment);
		if (index >= 0 && list[index].Equals(adjustment)) {
			throw new Exception("duplicate adjustment? " + adjustment + " and " + list[index]);
		}
		if (index < 0) { index = ~index; }
		list.Insert(index, adjustment);
		adjustments.Add(adjustment);
	}

	public string AdjustListToString(string key) => GetAdjustmentsFor(key).ConvertAll(a => a.key + ":" + a.value).Stringify(false);

	public void DebugPrintValueChange(string key, object oldValue, object newValue) {
		Debug.Log($"\"{key}\" was ({oldValue}), now ({newValue}) : {AdjustListToString(key)}");
	}

	public List<Adjustment> GetAdjustmentsFor(string key) {
		adjustmentsDict.TryGetValue(key, out List<Adjustment> list);
		return list;
	}

	public void RemoveAdjustmentSourcedFrom(object source) {
		List<int> indexes = adjustments.FindIndexes(x => x.source == source);
		adjustments.RemoveAtIndexes(indexes);
		foreach (var mods in adjustmentsDict) {
			indexes = mods.Value.FindIndexes(x => x.source == source);
			mods.Value.RemoveAtIndexes(indexes);
		}
	}
	public void RemoveAdjustment(Alteration adjustmentGroup) {
		RemoveAdjustment(adjustmentGroup, valueChangeListener.GetPersistentEventCount() > 0);
	}
	public void RemoveAdjustment(Alteration adjustmentGroup, bool notifyChange) {
		Dictionary<string,Adjustment> adjustmentsToRemove = adjustmentGroup.Adjustments;
		List<int> indexes = null;
		foreach (KeyValuePair<string, Adjustment> adjustment in adjustmentsToRemove) {
			RemoveAlterationsForStat(adjustment.Key, adjustmentGroup);
		}
		indexes = adjustments.FindIndexes(x => x.source == adjustmentGroup);
		if (notifyChange) {
			for (int i = indexes.Count-1; i >= 0; --i) {
				Adjustment adjustment = adjustments[indexes[i]];
				TryGetValue(adjustment.key, out object oldValue);
				adjustments.RemoveAt(indexes[i]);
				TryGetValue(adjustment.key, out object newValue);
				valueChangeListener.Invoke(adjustment.key, oldValue, newValue);
			}
		} else {
			adjustments.RemoveAtIndexes(indexes);
		}
	}
	private void RemoveAlterationsForStat(string key, Alteration adjustmentGroup) {
		if (!adjustmentsDict.TryGetValue(key, out List<Adjustment> currentAdjustmentsToThisKey)) { return; }
		for (int i = currentAdjustmentsToThisKey.Count - 1; i >= 0; --i) {
			if (currentAdjustmentsToThisKey[i].source != adjustmentGroup) { continue; }
			currentAdjustmentsToThisKey.RemoveAt(i);
		}
	}
	public void AddModifier(string key, object value, Alteration source) {
		Adjustment adjustment = new Adjustment(key, value, source);
		AddAdjustment(adjustment);
	}

	public bool TryGetValue(string key, out object value) => TryGetValue(key, out value, null);

	public bool TryGetValue(string key, out object value, List<Adjustment> mods) {
		if (!baseDictionary.TryGetValue(key, out value)) {
			value = 0f; return false;
		}
		adjustmentsDict.TryGetValue(key, out List<Adjustment> modifiers);
		float sum;
		try {
			sum = Convert.ToSingle(value);
		} catch (Exception) {
			return true;
		}
		if (modifiers != null) {
			for (int i = 0; i < modifiers.Count; ++i) {
				object nextValue = modifiers[i].value;
				float nextValueAdded = Convert.ToSingle(nextValue);
				Adjustment best = modifiers[i];
				for (int j = i + 1; j < modifiers.Count; ++j) {
					if (Adjustment.KindCompareTo(modifiers[j], modifiers[i]) != 0) break;
					nextValue = modifiers[j].value;
					float maybeHigherValue = Convert.ToSingle(nextValue);
					if (Mathf.Abs(maybeHigherValue) > Mathf.Abs(nextValueAdded)) {
						nextValueAdded = maybeHigherValue;
						best = modifiers[j];
					}
					i = j;
				}
				if (mods != null) {
					mods.Add(best);
				}
				Debug.Log(key+" modified by "+best.source.kind);
				sum += nextValueAdded;
			}
		}
		value = sum;
		return true;
	}

	[System.Serializable] public class KvWithNotes : ComputeHashTable<string, object>.KV {
		public string notes = "NOTES";
		public KvWithNotes(int hash, string k, KeyValueChangeCallback onChange) : base(hash, k, onChange) { }
		public KvWithNotes(int hash, string k, object v, KeyValueChangeCallback onChange) : base(hash, k, v, onChange) { }
	}

	public void PopulateData(List<object> data) {
		baseDictionary.PopulateData(data);
		Debug.Log("statfilter refresh");
		List<Adjustment> mods = new List<Adjustment>();
		for (int i = 0; i < data.Count; ++i) {
			ComputeHashTable<string, object>.KV kv = data[i] as ComputeHashTable<string, object>.KV;
			KvWithNotes adjustedKv = kv.CloneWithNotesWithoutCallback();
			// TryGetValue sums the adjustments with the original data in baseDictionary
			mods.Clear();
			TryGetValue(kv.key, out object value, mods);
			adjustedKv.value = value;
			data[i] = adjustedKv;
			if (mods != null) {
				string d = mods.Count > 0 ? "+" + mods.JoinToString("+", a => a.value + " " + a.source.kind) : "";
				adjustedKv.notes = kv.value + d + " = " + value;
			} else {
				adjustedKv.notes = "nothin";
			}
		}
	}

	public void PopulateData_Adjustment(List<object> data) {
		data.AddRange(adjustments);
	}

	public void Clear() {
		baseDictionary.Clear();
		ClearAdjustments();
	}

	public void ClearAdjustments() {
		adjustments.Clear();
		adjustmentsDict.Clear();
	}

	public void Add(string key, object value) => baseDictionary.Add(key, value);
	public bool ContainsKey(string key) => baseDictionary.ContainsKey(key);
	public bool Remove(string key) => baseDictionary.Remove(key);
	public void Add(KeyValuePair<string, object> item) => baseDictionary.Add(item);
	public bool Contains(KeyValuePair<string, object> item) => baseDictionary.Contains(item);
	public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => baseDictionary.CopyTo(array, arrayIndex);
	public bool Remove(KeyValuePair<string, object> item) => baseDictionary.Remove(item);
	public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => baseDictionary.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => baseDictionary.GetEnumerator();
	public void NotifyReorder(List<RowData> reordered) { baseDictionary.NotifyReorder(reordered); }
	public void NotifyReorder_Adjustment(List<RowData> reordered) { UnityDataSheet.NotifyReorder(reordered, adjustments); }
}

public static class KvExtention {
	public static StatFilter.KvWithNotes CloneWithNotesWithoutCallback(this ComputeHashTable<string, object>.KV kv) =>
		new StatFilter.KvWithNotes(kv.hash, kv.key, kv.value, null);
}
