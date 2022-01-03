using UnityEngine;

namespace NonStandard.Character
{
	public class CharacterMoveAnimationCallbacks : MonoBehaviour
	{
		public Root root;

		[Tooltip("hooks that allow code execution when character state changes (useful for animation)")]
		public Callbacks callbacks;

		void Start()
		{
			if (root == null) { root = GetComponentInParent<Root>(); }
			if (root == null) { root = GetComponent<Root>(); }
			if (root == null) { Utility.Follow f = GetComponent<Utility.Follow>();
				if (f) { root = f.whoToFollow.GetComponent<Root>(); }
			}
			if (root != null) {
				if (root.callbacks == null) {
					root.callbacks = gameObject.AddComponent<Callbacks>();
					root.callbacks.Initialize();
				}
				root.callbacks.moveDirectionChanged.AddListener(callbacks.moveDirectionChanged.Invoke);
				root.callbacks.stand.AddListener(callbacks.stand.Invoke);
				root.callbacks.jumped.AddListener(callbacks.jumped.Invoke);
				root.callbacks.fall.AddListener(callbacks.fall.Invoke);
				root.callbacks.wallCollisionStart.AddListener(callbacks.wallCollisionStart.Invoke);
				root.callbacks.wallCollisionStopped.AddListener(callbacks.wallCollisionStopped.Invoke);
				root.callbacks.arrived.AddListener(callbacks.arrived.Invoke);
			}
		}

	}
}