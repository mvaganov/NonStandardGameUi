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
	[System.Serializable] public class Alteration {
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
		[NonSerialized] private List<Adjustment> _adjustments;
		public List<Adjustment> Adjustments {
			get {
				if (_adjustments == null && adjustments != null) {
					_adjustments = new List<Adjustment>();
					for (int i = 0; i < adjustments.Count; i++) {
						_adjustments.Add(new Adjustment(adjustments[i].stat, float.Parse(adjustments[i].adjustment), this));
					}
				}
				return _adjustments;
			}
		}
		[System.Serializable] public struct Kvp { public string stat; public string adjustment; }
	}
	public class Adjustment : Computable<string,object>, IComparable<Adjustment> {
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

	[Tooltip("Adjustment listing. Please edit this with " + nameof(AddAdjustment) + " and " + nameof(RemoveAdjustmentSourcedFrom))]
	public List<Adjustment> adjustments = new List<Adjustment>();
	public Dictionary<string, List<Adjustment>> adjustmentsDict = new Dictionary<string, List<Adjustment>>();

	public ICollection<string> Keys => throw new System.NotImplementedException();

	public ICollection<object> Values => throw new System.NotImplementedException();

	public int Count => throw new System.NotImplementedException();

	public bool IsReadOnly => throw new System.NotImplementedException();

	public object this[string key] { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

	public void AddAdjustment(Alteration adjustmentGroup) {
		for(int i = 0; i < adjustmentGroup.Adjustments.Count; ++i) {
			AddAdjustment(adjustmentGroup.Adjustments[i]);
		}
	}

	public void AddAdjustment(Adjustment adjustment) {
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

	public void AddModifier(string key, object value, Alteration source) {
		Adjustment adjustment = new Adjustment(key, value, source);
		AddAdjustment(adjustment);
	}

	public bool TryGetValue(string key, out object value) {
		if (!baseDictionary.TryGetValue(key, out value)) {
			value = 0; return false;
		}
		adjustmentsDict.TryGetValue(key, out List<Adjustment> mods);
		float sum = 0;
		try {
			sum = Convert.ToSingle(value);
		} catch (Exception) {
			return true;
		}
		for (int i = 0; i < mods.Count; ++i) {
			object nextValue = mods[i].value;
			float nextValueAdded = Convert.ToSingle(nextValue);
			for (int j = i+1; j < mods.Count; ++j) {
				if (Adjustment.KindCompareTo(mods[j], mods[i]) != 0) break;
				nextValue = mods[j].value;
				float maybeHigherValue = Convert.ToSingle(nextValue);
				if (Mathf.Abs(maybeHigherValue) > Mathf.Abs(nextValueAdded)) {
					nextValueAdded = maybeHigherValue;
				}
				i = j;
			}
			sum += nextValueAdded;
		}
		value = sum;
		return true;
	}

	public void PopulateData(List<object> data) {
		baseDictionary.PopulateData(data);
		for (int i = 0; i < data.Count; ++i) {
			ComputeHashTable<string, object>.KV kv = data[i] as ComputeHashTable<string, object>.KV;
			ComputeHashTable<string, object>.KV adjustedKv = kv.CloneWithoutCallback();
			TryGetValue(kv.key, out object value);
			adjustedKv.value = value;
			data[i] = adjustedKv;
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
