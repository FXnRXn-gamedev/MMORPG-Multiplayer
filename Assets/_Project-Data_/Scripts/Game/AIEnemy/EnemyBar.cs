using System;
using UnityEngine;

namespace FXnRXn
{
	public class EnemyBar : MonoBehaviour
	{
    		
		#region Properties
		
		[Header("Settings")] 
		[Space(10)] 
		[field: SerializeField] private Camera				mainCamera;
		#endregion
		
		#region Unity Callbacks

		private void Start()
		{
			if(mainCamera == null) mainCamera = PlayerCameraController.Instance.GetMainCamera;
		}

		#endregion
		
		#region Methods

		private void Update()
		{
			if (mainCamera != null) transform.LookAt(mainCamera.transform);
		}

		#endregion
		
		//--------------------------------------------------------------------------------------------------------------


		#region Helper
		
		
		#endregion
	}
}


