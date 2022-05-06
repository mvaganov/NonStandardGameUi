// code by michael vaganov, released to the public domain via the unlicense (https://unlicense.org/)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.GameUi {
	public class Interactable : MonoBehaviour {
		public List<WayOfActing> interactions = new List<WayOfActing>();
		public bool stickyActionInEffortBar = false;
	}
}
