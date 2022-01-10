using NonStandard.GameUi;
#if NONSTANDARD_UICLICK
using NonStandard.Ui;
using NonStandard.Ui.Mouse;
#else
using UnityEngine.EventSystems;
#endif
using System;
using UnityEngine;

namespace NonStandard.Character {
	public class ClickToMove : MonoBehaviour {
		public UserController characterToMove;
		/*
		public KCode key = KCode.Mouse0;
		*/
		public Camera _camera;
		[System.Serializable]
		public class ClickSettings {
			public LayerMask raycastLayer = -1;
			public QueryTriggerInteraction raycastTriggerColliders = QueryTriggerInteraction.Ignore;
			public int moveMouseCursorSet = 1, UiMouseCursorSet = 0;
		}
		public ClickSettings clickSettings = new ClickSettings();
		public Interact3dItem prefab_waypoint;
		public Interact3dItem prefab_middleWaypoint;
		private ClickToMoveFollower follower;

#if UNITY_EDITOR
		/// called when created by Unity Editor
		void Reset() {
			if (characterToMove == null) { characterToMove = transform.GetComponentInParent<UserController>(); }
			if (characterToMove == null) { characterToMove = FindObjectOfType<UserController>(); }
			_camera = UiCamera.Find(this);
		}
#endif
		private void Awake() {
			if (prefab_middleWaypoint == null) {
				prefab_middleWaypoint = Instantiate(prefab_waypoint);
				prefab_middleWaypoint.Text = "cancel";
				prefab_middleWaypoint.name = prefab_waypoint.name + " cancel";
				if (prefab_waypoint.transform.parent != prefab_middleWaypoint.transform.parent) {
					prefab_middleWaypoint.transform.SetParent(prefab_waypoint.transform.parent);
				}
			}
		}
		public ClickToMoveFollower Follower(Move currentChar) {
			ClickToMoveFollower follower = currentChar.GetComponent<ClickToMoveFollower>();
			if (follower == null) {
				follower = currentChar.gameObject.AddComponent<ClickToMoveFollower>();
				follower.clickToMoveUi = this;
				ParticleSystem ps = currentChar.GetComponentInChildren<ParticleSystem>();
				if (ps != null) {
					ParticleSystem.MainModule m = ps.main;
					follower.color = m.startColor.color;
				}
				follower.Init(currentChar.gameObject);
			}
			return follower;
		}

		public void ClickFor(ClickToMoveFollower follower, RaycastHit rh) {
			if (!follower.enabled) return;
			follower.SetCurrentTarget(rh.point, rh.normal);
			follower.UpdateLine();
		}

		public void SetFollower(Move target) { follower = Follower(target); }
#if NONSTANDARD_UICLICK
		public bool IsMouseOverUi() { return UiClick.IsMouseOverUi(); }
		public Vector2 GetMousePosition() { return UiClick.GetMousePosition(); }
#else
		public bool IsMouseOverUi() { return EventSystem.current.IsPointerOverGameObject(); }
		public Vector2 GetMousePosition() { return UnityEngine.InputSystem.Mouse.current.position.ReadValue(); }
#endif
	public void RaycastClick(Action<RaycastHit> whatToDoOnSuccessfulClick) {
			if (!IsMouseOverUi()) {
				Ray ray = _camera.ScreenPointToRay(GetMousePosition());
				Physics.Raycast(ray, out RaycastHit rh, float.PositiveInfinity, clickSettings.raycastLayer, clickSettings.raycastTriggerColliders);
				if (rh.collider != null) {
					whatToDoOnSuccessfulClick.Invoke(rh);
				}
#if NONSTANDARD_MOUSECURSOR
				MouseCursor.Instance.currentSet = clickSettings.moveMouseCursorSet;
			} else {
				MouseCursor.Instance.currentSet = clickSettings.UiMouseCursorSet;
#endif
			}
		}

		private void Update() {
			if (follower == null && characterToMove.Target != null) {
				SetFollower(characterToMove.Target.move);
			}
			if (follower == null) return;
			/*
			if (AppInput.GetKey(key)) {
				RaycastClick(rh => ClickFor(follower, rh));
			}
			if (prefab_waypoint != null && AppInput.GetKeyUp(key)) {
				follower.ShowCurrentWaypoint();
			}
			*/
		}

		public void ClearAllWaypoints() {
			ClickToMoveFollower.allFollowers.ForEach(f => f.ClearWaypoints());
		}
	}
}