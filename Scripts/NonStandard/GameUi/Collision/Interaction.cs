// code by michael vaganov, released to the public domain via the unlicense (https://unlicense.org/)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.GameUi {
	public class Effort {
		/// <summary>
		/// the interaction to do, not including the actor doing the act
		/// </summary>
		public WayOfActing act;
		/// <summary>
		/// who or what is doing the interaction
		/// </summary>
		public object actor;
		/// <summary>
		/// how long this action has left to wait before it can activate
		/// </summary>
		private float delayTimer;
		public bool invalid;
		public Action<Effort> onAction;

		public Effort(object actor, WayOfActing act) {
			this.actor = actor;
			this.act = act;
			Start();
		}

		public void Start() {
			delayTimer = act.FindWarmUp();
		}

		public void Activate(object activator) {
			if (activator == null) {
				Debug.Log("it's null...");
			}
			Debug.Log("activate with [" + activator + "]");
			Activate();
		}

		public bool IsActivatable() {
			return !invalid && delayTimer <= 0;
		}
		public void Update() {
			delayTimer -= Time.deltaTime;
		}

		public void Activate() {
			if (delayTimer > 0) { return; }
			act.action.Invoke(this);
			onAction?.Invoke(this);
			if (act.howItIsRemoved == WayOfActing.HowItIsRemoved.Consumed) {
				invalid = true;
			}
			delayTimer = act.FindCooldown();
		}
	}

	[System.Serializable] public class WayOfActing {
		public string text;
		public UnityEngine.Object source;
		public Sprite icon;
		[Tooltip("how to weight this interaction. lower goes first.\n0: immediately life-threatening\n100: short-term life-threatening\n200: safety threatening\n300: contractual\n400: social expectation\n500: group identity\n600: personal preference\n700: personal goals\n800: self esteem\n900: long term advancement\n1000: long-term opportunities")]
		public float priority;
		[Tooltip("a function that has N objects interacting. the player (interactor[0]) and object(s)")]
		public UnityEvent_SubjectInteraction action;
		public HowItIsRemoved howItIsRemoved = HowItIsRemoved.Consumed;

		public DelayRule[] delayRules;
		[System.Serializable] public class DelayRule {
			public enum Kind { None, Cooldown, WarmUp }
			public Kind kind;
			public float value = 0;
		}

		public enum HowItIsRemoved { None, Consumed, LostIfTooFar }
		/// <summary>
		/// (object subjectDoingInteraction, Interaction theInteractionBeingDone)
		/// </summary>
		[System.Serializable] public class UnityEvent_SubjectInteraction : UnityEvent<Effort> { }

		public WayOfActing(UnityEngine.Object source, string text, Sprite icon, float priority, EventBind eventBind) {
			this.source = source;
			this.text = text;
			this.icon = icon;
			this.priority = priority;
			if (action == null) { action = new UnityEvent_SubjectInteraction(); }
			//EventBind.IfNotAlready(action, target, methodName);
			eventBind.Bind(action);
		}
		public Effort ActedBy(object actor) => new Effort(actor, this);
		public float FindWarmUp() => FindValue(DelayRule.Kind.WarmUp);
		public float FindCooldown() => FindValue(DelayRule.Kind.Cooldown);
		public float FindValue(DelayRule.Kind kind) {
			int index = Array.FindIndex(delayRules, r => r.kind == kind);
			if (index < 0) return 0;
			return delayRules[index].value;
		}
	}
}
