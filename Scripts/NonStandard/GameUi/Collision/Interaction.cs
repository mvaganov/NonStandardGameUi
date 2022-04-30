using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.GameUi {
	[System.Serializable] public class Interaction {
		public string text;
		public UnityEngine.Object source;
		public Sprite icon;
		[Tooltip("how to weight this interaction. lower goes first.\n0: immediately life-threatening\n100: short-term life-threatening\n200: safety threatening\n300: contractual\n400: social expectation\n500: group identity\n600: personal preference\n700: personal goals\n800: self esteem\n900: long term advancement\n1000: long-term opportunities")]
		public float priority;
		[Tooltip("a function that has N objects interacting. the player (interactor[0]) and object(s)")]
		public UnityEvent_SubjectInteraction action;
		public HowItIsRemoved howItIsRemoved = HowItIsRemoved.Consumed;
		/// <summary>
		/// how long this action has left to wait before it can activate
		/// </summary>
		private float delayTimer;
		private DelayRule _cooldown;
		private DelayRule _warmup;
		public DelayRule[] delayRules;
		public Action<Interaction> onAction;
		public bool invalid;

		[System.Serializable] public class DelayRule {
			public enum Kind { None, Cooldown, WarmUp }
			public Kind kind;
			public float time = 0;
		}

		public enum HowItIsRemoved { None, Consumed, LostIfTooFar }
		/// <summary>
		/// (object subjectDoingInteraction, Interaction theInteractionBeingDone)
		/// </summary>
		[System.Serializable] public class UnityEvent_SubjectInteraction : UnityEvent<object, Interaction> { }

		public static void HowRemoved_Consumed(Interaction interaction, InteractionInterface interactionInterface) {
			interactionInterface.actions.Remove(interaction);
		}

		public Interaction(UnityEngine.Object source, string text, Sprite icon, float priority, EventBind eventBind) {
			this.source = source;
			this.text = text;
			this.icon = icon;
			this.priority = priority;
			if (action == null) { action = new UnityEvent_SubjectInteraction(); }
			//EventBind.IfNotAlready(action, target, methodName);
			eventBind.Bind(action);
			Start();
		}

		public void Start() {
			RefreshDelayRules();
			if (_warmup != null) {
				delayTimer = _warmup.time;
			}
		}

		public void RefreshDelayRules() {
			if (delayRules == null) return;
			for (int i = 0; i < delayRules.Length; i++) {
				switch (delayRules[i].kind) {
					case DelayRule.Kind.Cooldown: _cooldown = delayRules[i]; break;
					case DelayRule.Kind.WarmUp: _warmup = delayRules[i]; break;
				}
			}
		}

		public void Update() {
			delayTimer -= Time.deltaTime;
		}

		//public void Activate() { Debug.Log("activate with no args"); }

		public void Activate(object activator) {
			Debug.Log("activate with " + activator);
			if (delayTimer > 0) { return; }
			action.Invoke(activator, this);
			onAction?.Invoke(this);
			if (howItIsRemoved == HowItIsRemoved.Consumed) {
				invalid = true;
			}
			if (_cooldown != null) {
				delayTimer = _cooldown.time;
			}
		}

		public bool IsActivatable() {
			return !invalid && delayTimer <= 0;
		}
	}
}
