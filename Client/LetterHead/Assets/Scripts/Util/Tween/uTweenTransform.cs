using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace uTools {
	[AddComponentMenu("uTools/Tween/Tween Position(Transform)")]
	public class uTweenTransform : uTweener {
		
		public Vector3 from;
		public Vector3 to;
		
		Transform mRectTransform;

        public Transform cachedRectTransform { get { if (mRectTransform == null) mRectTransform = GetComponent<Transform>(); return mRectTransform; } }
		public Vector3 value {
			get { return cachedRectTransform.localPosition;}
			set { cachedRectTransform.localPosition = value;}
		}
		
		protected override void OnUpdate (float factor, bool isFinished)
		{
			value = from + factor * (to - from);
		}

        public static uTweenTransform Begin(GameObject go, Vector3 from, Vector3 to, float duration = 1f, float delay = 0f)
        {
            uTweenTransform comp = uTweener.Begin<uTweenTransform>(go, duration);
			comp.from = from;
			comp.to = to;
			comp.duration = duration;
			comp.delay = delay;
			if (duration <= 0) {
				comp.Sample(1, true);
				comp.enabled = false;
			}
			return comp;
		}

		[ContextMenu("Set 'From' to current value")]
		public override void SetStartToCurrentValue () { from = value; }
		
		[ContextMenu("Set 'To' to current value")]
		public override void SetEndToCurrentValue () { to = value; }
		
		[ContextMenu("Assume value of 'From'")]
		public override void SetCurrentValueToStart () { value = from; }
		
		[ContextMenu("Assume value of 'To'")]
		public override void SetCurrentValueToEnd () { value = to; }

	}
}
