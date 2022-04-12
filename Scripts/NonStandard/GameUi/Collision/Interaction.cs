using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.GameUi {
	[System.Serializable] public class Interaction {
		public string text;
		public Sprite icon;
		[Tooltip("a function that has 2 objects interacting. the player (interactor) and object")]
		public UnityEvent<object,object> interaction;
	}
}
