// code by michael vaganov, released to the public domain via the unlicense (https://unlicense.org/)
using UnityEngine;

namespace NonStandard.Character {
	public class SuperCyanGlue : MonoBehaviour {
		public Animator animator;
		[ContextMenuItem("Bind Events", "BindEvents")]
		public Root root;
		private void Start() {
			Init();
		}
		private void Init() {
			if (animator == null) { animator = GetComponent<Animator>(); }
			if (animator == null) { animator = GetComponentInParent<Animator>(); }
			if (root == null) { root = GetComponent<Root>(); }
			if (root == null) { root = GetComponentInParent<Root>(); }
			if (root == null) {
				Utility.Follow f = GetComponent<Utility.Follow>();
				if (f) { root = f.whoToFollow.GetComponent<Root>(); }
			}
		}
#if UNITY_EDITOR
		public void BindEvents() {
			Init();
			if (root.callbacks == null) {
				root.callbacks = root.gameObject.GetComponent<Callbacks>();
				if (root.callbacks == null) {
					root.callbacks = root.gameObject.AddComponent<Callbacks>();
				}
			}
			Callbacks cb = root.callbacks;
			EventBind.IfNotAlready(cb.jumped, this, nameof(Jump));
			EventBind.IfNotAlready(cb.stand, this, nameof(Stand));
			EventBind.IfNotAlready(cb.fall, this, nameof(Fall));
			EventBind.IfNotAlready(cb.arrived, this, nameof(Wave));
		}
		private void OnValidate() {
			BindEvents();
		}
#endif
		bool shouldTriggerJumpAnimation = false;
		public void Jump(Vector3 dir) {
			//Show.Log("jump");
			//animator.SetTrigger("Land");
			animator.SetBool("Grounded", false);
			if (root.move.IsStableOnGround) {
				shouldTriggerJumpAnimation = true;
			}
		}

		public void Stand(Vector3 upDir) {
			//Show.Log("stand");
			animator.SetTrigger("Land");
			animator.SetBool("Grounded", true);
		}

		public void Fall() {
			//Show.Log("fall");
			animator.SetBool("Grounded", false);
		}

		public void Wave(Vector3 location) {
			//Show.Log("wave");
			animator.SetTrigger("Wave");
		}

		public void FixedUpdate() {
			if (animator.GetBool("Grounded")) {
				float speed = root.rb.velocity.magnitude;
				animator.SetFloat("MoveSpeed", speed);
			}
			if (shouldTriggerJumpAnimation && !animator.IsInTransition(0)) {
				animator.SetTrigger("Jump");
				animator.SetBool("Grounded", false);
				shouldTriggerJumpAnimation = false;
			}
		}
	}
}
