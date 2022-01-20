using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class InteractionWatcher : MonoBehaviour {
		public HashSet<RangedTarget> targets = new HashSet<RangedTarget>();
		List<Wire> wires = new List<Wire>();
		public Color color = new Color(1, 1, 0, .5f);
		SphereCollider sc;
		public Collider activeCollider;
		public struct RangedTarget {
			public Transform t;
			public Interactable interactable;
			public float range;
			public override int GetHashCode() { return t.GetHashCode(); }
			public RangedTarget(Transform what, Interactable interactable, float dist) {
				this.t = what; this.interactable = interactable; this.range = dist;
			}
		}
		private void Start() {
			sc = GetComponent<SphereCollider>();
		}
		void Update() {
			List<RangedTarget> transformsOutOfRange = DrawTargetsAndRemoveOutOfRange();
			if (transformsOutOfRange != null) { transformsOutOfRange.ForEach(t => targets.Remove(t)); }
		}
		List<RangedTarget> DrawTargetsAndRemoveOutOfRange() {
			Vector3 pos = transform.position;
			int index = 0;
			List<RangedTarget> transformsOutOfRange = null;
			// make sure there are enough wires to draw all of the targets
			while (targets.Count > wires.Count) {
				wires.Add(Lines.MakeWire(wires.Count.ToString()));
			}
			foreach (RangedTarget oa in targets) {
				Wire w = wires[index++];
				bool canPointAt = oa.t != null && oa.t.gameObject.activeInHierarchy && oa.interactable.enabled;
				if (canPointAt) {
					Vector3 a, b;
					if (activeCollider != null) {
						CalculateSpaceBetween(activeCollider, oa.t.GetComponent<Collider>(), out a, out b);
					} else {
						a = pos;
						CalculateSpaceBetween(a, oa.t.GetComponent<Collider>(), out b);
					}
					float abDist = Vector3.Distance(a, b);
					if ((canPointAt = abDist < oa.range)) {
						w.Arrow(a, b, color);
						w.gameObject.SetActive(true);
					}
				} 
				if (!canPointAt) {
					if (transformsOutOfRange == null) { transformsOutOfRange = new List<RangedTarget>(); }
					transformsOutOfRange.Add(oa);
					w.gameObject.SetActive(false);
				}
			}
			for (int i = targets.Count; i < wires.Count; ++i) {
				Wire w = wires[i];
				w.gameObject.SetActive(false);
			}
			return transformsOutOfRange;
		}
		public static void CalculateSpaceBetween(Collider a, Collider b, out Vector3 edgeA, out Vector3 edgeB) {
			Vector3 apos = a.transform.position, bpos = b.transform.position;
			Vector3 delta = bpos - apos;
			float centerDist = delta.magnitude;
			Vector3 dir = delta / centerDist;
			RaycastHit rh;
			if (a.Raycast(new Ray(bpos, -dir), out rh, centerDist)) {
				edgeA = rh.point;
			} else { edgeA = apos; }
			if (b.Raycast(new Ray(apos, dir), out rh, centerDist)) {
				edgeB = rh.point;
			} else { edgeB = bpos; }
		}
		public static void CalculateSpaceBetween(Vector3 a, Collider b, out Vector3 edgeB) {
			Vector3 delta = b.transform.position - a;
			float centerDist = delta.magnitude;
			Vector3 dir = delta / centerDist;
			RaycastHit rh;
			b.Raycast(new Ray(a, dir), out rh, centerDist);
			edgeB = rh.point;
		}
		private void OnTriggerEnter(Collider other) {
			Interactable invObj = other.GetComponent<Interactable>();
			if (invObj == null || !invObj.enabled) return;
			Transform t = other.transform;
			Vector3 delta = t.position - transform.position;
			float dist = delta.magnitude;
			targets.Add(new RangedTarget(t, invObj, dist + 1f/128));
		}
	}
}