﻿using UnityEngine;

namespace NonStandard.Character
{
	public class CharacterMoveAnimationCallbacks : MonoBehaviour
	{
		public CharacterMove character;

		[Tooltip("hooks that allow code execution when character state changes (useful for animation)")]
		public CharacterMove.Callbacks callbacks = new CharacterMove.Callbacks();

		void Start()
		{
			if (character == null) { character = GetComponentInParent<CharacterMove>(); }
			if (character == null) { character = GetComponent<CharacterMove>(); }
			if (character == null) { Utility.Follow f = GetComponent<Utility.Follow>();
				if (f) { character = f.whoToFollow.GetComponent<CharacterMove>(); }
			}
			if (character != null) {
				if (character.callbacks == null) {
					character.callbacks = new CharacterMove.Callbacks();
					character.callbacks.Initialize();
				}
				character.callbacks.moveDirectionChanged.AddListener(callbacks.moveDirectionChanged.Invoke);
				character.callbacks.stand.AddListener(callbacks.stand.Invoke);
				character.callbacks.jumped.AddListener(callbacks.jumped.Invoke);
				character.callbacks.fall.AddListener(callbacks.fall.Invoke);
				character.callbacks.wallCollisionStart.AddListener(callbacks.wallCollisionStart.Invoke);
				character.callbacks.wallCollisionStopped.AddListener(callbacks.wallCollisionStopped.Invoke);
				character.callbacks.arrived.AddListener(callbacks.arrived.Invoke);
			}
		}

	}
}