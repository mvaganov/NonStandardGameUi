using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.GameUi {
	[System.Serializable] public class Interaction {
		public string text;
		public Sprite icon;
		[Tooltip("[0] is the originator of the interaction")]
		public object[] interactors;
		[Tooltip("a function that has N objects interacting. the player (interactor[0]) and object(s)")]
		public UnityEvent_objectL interaction;

		[System.Serializable] public class UnityEvent_objectL : UnityEvent<object[]> { }

		public void Activate() { interaction.Invoke(interactors); }
	}
}
