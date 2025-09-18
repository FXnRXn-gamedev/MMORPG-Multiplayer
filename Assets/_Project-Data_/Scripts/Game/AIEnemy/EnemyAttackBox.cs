using UnityEngine;

namespace FXnRXn
{
	public class EnemyAttackBox : MonoBehaviour
	{
    		
		#region Properties
		
		[Header("Settings")] 
		[Space(10)] 
		[field: SerializeField] private float				destroyTime = 0.2f;
		
		
		
		
		public float Damage { get; set; }
		public GameObject Enemy { get; set; }

		#endregion
		
		#region Unity Callbacks
		private void Start()
		{
			Destroy(gameObject, destroyTime);
		}

		private void OnTriggerEnter(Collider other)
		{
			HeroController hero = other.gameObject.GetComponent<HeroController>();
			if (hero != null)
			{
				hero?.GetHit(Damage, Enemy);
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


