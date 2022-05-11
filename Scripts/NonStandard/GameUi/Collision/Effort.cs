// code by michael vaganov, released to the public domain via the unlicense (https://unlicense.org/)
using NonStandard.Data;
using NonStandard.Extension;
using NonStandard.GameUi.DataSheet;
using NonStandard.GameUi.Inventory;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NonStandard.GameUi {
	public class Effort : IHasUiElement {
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
		private float howLongToWait;
		public GameObject uiElement;
		public bool invalid;
		public bool activatableLastFrame;
		public bool isOutOfRange;
		public bool isFinished;
		public bool isPaused;
		public Action<Effort> onAction;
		private WayOfActing.TimingRule _currentTiming;
		private Action _onTimingFinished;

		public GameObject UiElement { get { return uiElement; } set { uiElement = value; } }

		public Effort(object actor, WayOfActing act) {
			this.actor = actor;
			this.act = act;
			Start();
		}

		public void Start() {
			StartTiming(WayOfActing.TimingRule.Kind.WarmUp, ActionFinished);
		}

		public void Activate(object activator) {
			if (activator == null) {
				Debug.Log("it's null...");
			}
			//Debug.Log("activate with [" + activator + "]");
			Activate();
		}

		public bool IsActivatable() {
			return !invalid && !isOutOfRange && howLongToWait <= 0;
		}

		public void Update() {
			if (isPaused) { return; }
			if (_currentTiming != null) {
				howLongToWait -= Time.deltaTime;
				float progress = (howLongToWait > 0) ? (_currentTiming.value - howLongToWait) / _currentTiming.value : 1;
				_currentTiming.OnProgress?.Invoke(progress);
				if (progress == 1) {
					_onTimingFinished?.Invoke();
					_onTimingFinished = null;
					_currentTiming = null;
				}
			}
		}

		public void Activate() {
			if (howLongToWait > 0) { return; }
			act.action.Invoke(this);
			onAction?.Invoke(this);
			StartTiming(WayOfActing.TimingRule.Kind.Duration, ActionFinished);
		}

		public void ActionFinished() {
			if (act.howItIsRemoved == WayOfActing.HowItIsRemoved.Consumed) {
				invalid = true;
			}
			WayOfActing.TimingRule cooldown = act.FindCooldown();
			howLongToWait = cooldown != null ? cooldown.value : 0;
			cooldown?.OnProgress.Invoke(0);
			_currentTiming = cooldown;
		}

		private void StartTiming(WayOfActing.TimingRule.Kind kind, Action onFinished) {
			WayOfActing.TimingRule rule = act.FindValue(kind);
			if (rule != null) {
				rule = rule.Copy();
				int index = EventBind.GetPersistentEventMethodNameIndex(rule.OnProgress,
					nameof(InventoryItemObject.SetActivateButtonProgressUiBar));
				if (index >= 0) {
					EventBind.RemoveAt(rule.OnProgress, index);
					Image img = FindBiggestVisibleFillImage(uiElement);
					EventBind.IfNotAlready(rule.OnProgress, img, "set_" + nameof(img.fillAmount));
				}
				howLongToWait = rule.value;
				rule.OnProgress?.Invoke(0);
				_onTimingFinished = onFinished;
			} else {
				howLongToWait = 0;
				_onTimingFinished = null;
				onFinished.Invoke();
			}
			_currentTiming = rule;
		}



		public Image FindBiggestVisibleFillImage(GameObject root) {
			Image[] images = root.GetComponentsInChildren<Image>();
			if (images == null || images.Length == 0) { return null; }
			Rect r;
			float bestArea = -1, area;
			Image best = null;
			for (int i = 0; i < images.Length; ++i) {
				Image img = images[i];
				if (!img.enabled || img.color == Color.clear || img.type != Image.Type.Filled) continue;
				r = images[i].rectTransform.rect;
				area = r.width * r.height;
				if (best == null || area >= bestArea) {
					bestArea = area;
					best = images[i];
				}
			}
			return best;
		}
	}

	[System.Serializable] public class WayOfActing {
		public string text;
		public UnityEngine.Object source;
		public Sprite icon;
		[Tooltip("how to weight this activity. lower goes first.\n0: immediately life-threatening\n100: short-term life-threatening\n200: safety threatening\n300: contractual\n400: social expectation\n500: group identity\n600: personal preference\n700: personal goals\n800: self esteem\n900: long term advancement\n1000: long-term opportunities")]
		public float priority;
		public UnityEvent_SubjectInteraction action;
		public HowItIsRemoved howItIsRemoved = HowItIsRemoved.Consumed;

		public TimingRule[] delayRules;
		[System.Serializable] public class TimingRule {
			public enum Kind { None, Cooldown, WarmUp, Duration }
			public Kind kind;
			public float value = 0;
			public UnityEvent_float OnProgress;
			[Tooltip("If there is a value here, progress can be interrupted")]
			public UnityEvent OnInterrupt;
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
		public TimingRule FindWarmUp() => FindValue(TimingRule.Kind.WarmUp);
		public TimingRule FindCooldown() => FindValue(TimingRule.Kind.Cooldown);
		public TimingRule FindDuration() => FindValue(TimingRule.Kind.Duration);
		public TimingRule FindValue(TimingRule.Kind kind) {
			int index = Array.FindIndex(delayRules, r => r.kind == kind);
			if (index < 0) return null;
			return delayRules[index];
		}
	}
}
