using System;
using UnityEngine;

namespace FXnRXn
{
	public class AttackHitBox : MonoBehaviour
	{
    		
		#region Properties
		[Header("Settings")] 
		[Space(10)] 
		[field: SerializeField] private float				destroyTime = 0.2f;
		
		
		
		
		public float Damage { get; set; }
		public GameObject Hero { get; set; }
		
		#endregion
		
		#region Unity Callbacks

		private void Start()
		{
			Destroy(gameObject, destroyTime);
		}

		private void OnTriggerEnter(Collider other)
		{
			EnemyControllerBase enemy = other.gameObject.GetComponent<EnemyControllerBase>();
			if (enemy != null)
			{
				enemy?.GetHit(Damage, Hero);
			}
		}

		#endregion
		
		#region Methods

		

		#endregion
		
		//--------------------------------------------------------------------------------------------------------------


		#region Helper
		
		
		#endregion
	}
}


