using System;
using PrimeTween;
using UnityEngine;

namespace FXnRXn
{
	public class TransformTweenCases : MonoBehaviour
	{
    		
		#region Properties

		[Header("Transform")] 
		[Space(10)] 
		[field: SerializeField] private bool doTweenTransform;
		[field: SerializeField] private Vector3 targetTransform;
		[field: SerializeField] private AnimationCurve tweenTransformCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
		[field: SerializeField] private float tweenDuration = 0.6f;
		
		
		[Header("Rotation")]
		[Space(10)]
		[field: SerializeField] private Vector3 targetRotation;
		
		[Header("Scale")]
		[Space(10)]
		[field: SerializeField] private Vector3 targetScale;
		#endregion
		
		
		#region Methods

		private void Start()
		{
			if(doTweenTransform) AsyncTweenTransform();
		}


		async void AsyncTweenTransform()
		{
			await Sequence.Create(cycles: -1, CycleMode.Rewind, Ease.Linear, false, UpdateType.Default)
				.Chain(Tween.LocalPosition(transform, targetTransform, tweenDuration));

		}

		#endregion
		
		//--------------------------------------------------------------------------------------------------------------


		#region Helper
		
		
		#endregion
	}
}


