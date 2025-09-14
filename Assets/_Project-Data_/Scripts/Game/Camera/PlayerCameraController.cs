using System;
using UnityEngine;
using Mirror;

namespace FXnRXn
{
	public class PlayerCameraController : NetworkBehaviour
	{
		#region Singleton
		public static PlayerCameraController Instance { get; private set; }

		private void Awake()
		{
			if (Instance == null) Instance = this;
		}

		#endregion
    		
		#region Properties

		[Header("Settings")] 
		[Space(10)] 
		[field: SerializeField] private Vector3 cameraOffset;
		[field: SerializeField] private float smoothTime = 0.3f;
		[field: SerializeField] private float yThreshold = 3f;

		
		public GameObject Hero { get; set; }
		
		// Private fields
		private Vector3 currentVelocity;
		private float yCurrent;
		private float yTarget;
		private Vector3 targetCameraPosition
			;
		private Camera mainCamera;


		#endregion
		
		#region Network Method

		public override void OnStartLocalPlayer()
		{
			base.OnStartLocalPlayer();
			InitializeCamera();

			
		}

		#endregion

		#region Unity Callbacks
		
		private void Start()
		{
			// Cache the main camera component
			mainCamera = GetComponentInChildren<Camera>();
			transform.position = cameraOffset;
		}

		private void LateUpdate()
		{
			if (Hero == null) return;

			UpdateCameraPosition();
		}

		#endregion
		
		#region Camera Control Methods
		private void InitializeCamera()
		{
			if (HeroController.Instance != null)
			{
				Vector3 heroPosition = HeroController.Instance.transform.position;
				yTarget = heroPosition.y;
				yCurrent = yTarget;
			}
		}
		
		
		private void UpdateCameraPosition()
		{
			Vector3 heroPosition = Hero.transform.position;
			yTarget = heroPosition.y;

			// Use threshold to determine if we should snap or smooth Y movement
			if (Mathf.Abs(yTarget - yCurrent) > yThreshold)
			{
				yCurrent = yTarget; // Snap to target
			}
            
			// Calculate target camera position
			targetCameraPosition = new Vector3(heroPosition.x, yCurrent, heroPosition.z) + cameraOffset;
            
			// Smoothly move camera to target position
			transform.position = Vector3.SmoothDamp(
				transform.position, 
				targetCameraPosition, 
				ref currentVelocity, 
				smoothTime
			);
		}
		
		public void SetHero(GameObject newHero)
		{
			Hero = newHero;
			if (Hero != null)
			{
				InitializeCamera();
			}
		}


		#endregion

		#region Methods



		#endregion

		//--------------------------------------------------------------------------------------------------------------


		#region Properties and Accessors
		
		public Camera GetMainCamera => GetComponentInChildren<Camera>();
		public bool IsFollowingHero => Hero != null;

		#endregion
		
		#region Cleanup
		private void OnDestroy()
		{
			if (Instance == this)
			{
				Instance = null;
			}
		}
		#endregion

	}
}


