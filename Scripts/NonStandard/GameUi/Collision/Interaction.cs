using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.GameUi {
	[System.Serializable] public class Interaction {
		public string text;
		public Sprite icon;
		[Tooltip("how to weight this interaction. lower goes first.\n0: immediately life-threatening\n100: short-term life-threatening\n200: safety threatening\n300: contractual\n400: social expectation\n500: group identity\n600: personal preference\n700: personal goals\n800: self esteem\n900: long term advancement\n1000: long-term opportunities")]
		public float priority;
		[Tooltip("[0] is the originator of the interaction")]
		public UnityEngine.Object[] interactors;
		[Tooltip("a function that has N objects interacting. the player (interactor[0]) and object(s)")]
		public UnityEvent_Interaction action;

		[System.Serializable] public class UnityEvent_Interaction : UnityEvent<Interaction> { }

		public Interaction(string text, Sprite icon, float priority,
		EventBind eventBind,//UnityEngine.Object target, string methodName, 
		UnityEngine.Object[] interactors) {
			this.text = text;
			this.icon = icon;
			this.priority = priority;
			if (action == null) { action = new UnityEvent_Interaction(); }
			//EventBind.IfNotAlready(action, target, methodName);
			eventBind.Bind(action);
			this.interactors = interactors;
		}

		public void Activate() {
			action.Invoke(this);
		}
	}
}
