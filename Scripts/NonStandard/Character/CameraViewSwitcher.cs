using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Character {
	public class CameraViewSwitcher : MonoBehaviour {
		CharacterCamera cam;
		Transform t;
		public List<CameraView> knownCameraViews = new List<CameraView>();
		[System.Serializable]
		public class CameraView {
			public string name;
			[HideInInspector] public Quaternion rotation;
			[SerializeField] public Vector3 _Rotation;
			/// <summary>
			/// if target is null, use this
			/// </summary>
			public Vector3 position;
			public Transform target;
			public float distance;
			public bool useTransformPositionChanges;
			public bool ignoreLookRotationChanges;
			public bool rotationIsLocal;
			public bool positionLocalToLastTransform;
			public void ResolveLookRotationIfNeeded() {
				if (rotation.x == 0 && rotation.y == 0 && rotation.z == 0 && rotation.w == 0) {
					rotation = Quaternion.Euler(_Rotation);
				}
				//Debug.Log(Show.Stringify(this));
			}
		}
		public void ToggleView(string viewName) {
			if (currentViewname != defaultViewName) {
				LerpView(defaultViewName);
			} else {
				LerpView(viewName);
			}
		}
		private string defaultViewName = "user";
		private string currentViewname = "user";
		public string CurrentViewName { get { return currentViewname; } }
		public void SetLerpSpeed(float durationSeconds) { lerpDurationMs = (long)(durationSeconds * 1000); }
		private ulong started, end;
		public long lerpDurationMs = 250;
		private float distStart;
		private Quaternion rotStart;
		private bool lerping = false;
		private Vector3 startPosition;
		private CameraView targetView = new CameraView();
		internal Transform userTarget;
		/// <summary>
		/// user-defined zoom
		/// </summary>
		public float userDistance;
		/// <summary>
		/// user-defined rotation
		/// </summary>
		protected Quaternion userRotation;
		public Quaternion UserRotation => userRotation;
		private void Awake() {
			cam = GetComponent<CharacterCamera>();
			t = cam.transform;
			targetView.target = userTarget;
			targetView.rotation = userRotation;
			targetView.distance = userDistance;
		}
		private void Start() {
			userTarget = cam.target;
			userDistance = cam.targetDistance;
			userRotation = t.rotation;
		}
		public void LerpView(string viewName) {
			currentViewname = viewName;
			string n = viewName.ToLower();
			switch (n) {
				case "user":
					LerpRotation(userRotation);
					LerpDistance(userDistance);
					LerpTarget(userTarget);
					return;
				default:
					for (int i = 0; i < knownCameraViews.Count; ++i) {
						if (knownCameraViews[i].name.ToLower().Equals(n)) {
							//Debug.Log("doing " + n + " "+Show.Stringify(knownCameraViews[i]));
							LerpTo(knownCameraViews[i]);
							return;
						}
					}
					break;
			}
			/*
			ReflectionParseExtension.TryConvertEnumWildcard(typeof(Direction3D), viewName, out object v);
			if (v != null) {
				LerpDirection((Direction3D)v); return;
			}
			*/
			Debug.LogWarning($"unkown view name \"{viewName}\"");
		}
		public enum Direction3D { Down = 1, Left = 2, Back = 4, Up = 8, Right = 16, Forward = 32, }
		public Vector3 ConvertToVector3(Direction3D dir) {
			switch (dir) {
				case Direction3D.Down: return Vector3.down;
				case Direction3D.Left: return Vector3.left;
				case Direction3D.Back: return Vector3.back;
				case Direction3D.Up: return Vector3.up;
				case Direction3D.Right: return Vector3.right;
				case Direction3D.Forward: return Vector3.forward;
			}
			return Vector3.zero;
		}
		public void LerpDirection(Direction3D dir) { LerpDirection(ConvertToVector3(dir)); }
		public void LerpDirection(Vector3 direction) { LerpRotation(Quaternion.LookRotation(direction)); }
		public void LerpRotation(Quaternion direction) {
			targetView.rotation = direction;
			StartLerpToTarget();
		}
		public void LerpDistance(float distance) {
			targetView.distance = distance;
			StartLerpToTarget();
		}
		public void LerpTarget(Transform target) {
			targetView.target = target;
			StartLerpToTarget();
		}
		public void LerpTo(CameraView view) {
			targetView.name = view.name;
			targetView.useTransformPositionChanges = view.useTransformPositionChanges;
			targetView.ignoreLookRotationChanges = view.ignoreLookRotationChanges;
			targetView.rotationIsLocal = view.rotationIsLocal;
			targetView.positionLocalToLastTransform = view.positionLocalToLastTransform;
			if (view.useTransformPositionChanges) { targetView.target = view.target; }
			targetView.distance = view.distance;
			if (!view.ignoreLookRotationChanges) {
				view.ResolveLookRotationIfNeeded();
				targetView.rotation = view.rotation;
			}
			StartCoroutine(StartLerpToTarget());
		}
		public IEnumerator StartLerpToTarget() {
			if (lerping) yield break;
			lerping = true;
			rotStart = t.rotation;
			startPosition = t.position;
			distStart = cam.DistanceBecauseOfObstacle;
			if (targetView.positionLocalToLastTransform && cam.target != null) {
				Quaternion q = !targetView.ignoreLookRotationChanges ? targetView.rotation : t.rotation;
				targetView.position = cam.target.position - (q * Vector3.forward) * targetView.distance;
				Debug.Log("did the thing");
			}
			//if (targetView.target != null) {
			userTarget = cam.target;
			cam.target = null;
			//}
			started = Move.Now;
			end = Move.Now + (ulong)lerpDurationMs;
			yield return null;
			//Proc.Delay(0, LerpToTarget);
			while (lerping) {
				LerpToTarget();
				yield return null;
			}
		}
		private void LerpToTarget() {
			lerping = true;
			ulong now = Move.Now;
			ulong passed = now - started;
			float p = (float)passed / lerpDurationMs;
			if (now >= end) { p = 1; }
			if (!targetView.ignoreLookRotationChanges) {
				targetView.ResolveLookRotationIfNeeded();
				if (targetView.rotationIsLocal) {
					Quaternion startQ = targetView.rotationIsLocal ? targetView.target.rotation : Quaternion.identity;
					Quaternion.Lerp(rotStart, targetView.rotation * startQ, p);
				} else {
					t.rotation = Quaternion.Lerp(rotStart, targetView.rotation, p);
				}
			}
			//Show.Log("asdfdsafdsa");
			cam.targetDistance = (targetView.distance - distStart) * p + distStart;
			if (targetView.useTransformPositionChanges) {
				if (targetView.target != null) {
					Quaternion rot = targetView.rotation * (targetView.rotationIsLocal ? targetView.target.rotation : Quaternion.identity);
					Vector3 targ = targetView.target.position;
					Vector3 dir = rot * Vector3.forward;
					RaycastHit hitInfo;
					if (cam.clipAgainstWalls && Physics.Raycast(targ, -dir, out hitInfo, targetView.distance)) {
						cam.DistanceBecauseOfObstacle = hitInfo.distance;
					} else {
						cam.DistanceBecauseOfObstacle = targetView.distance;
					}
					Vector3 finalP = targ - dir * cam.DistanceBecauseOfObstacle;
					//Debug.Log(targetView.distance+"  "+distanceBecauseOfObstacle+"  "+targ+" "+targetView.target);
					t.position = Vector3.Lerp(startPosition, finalP, p);
					//Debug.Log("# "+p+" "+finalP);
				} else {
					t.position = Vector3.Lerp(startPosition, targetView.position, p);
					//Debug.Log("!" + p + " " + targetView.position);
				}
			}
			cam.RecalculateRotation();
			if (p >= 1) {
				if (targetView.useTransformPositionChanges) {
					cam.target = targetView.target;
				}
				lerping = false;
			}
		}
	}
}
