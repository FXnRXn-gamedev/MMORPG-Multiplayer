using UnityEngine;

namespace FXnRXn
{
	public class EnemyHermit : EnemyControllerBase
	{
    		
		#region Properties
		
		
		#endregion
		
		#region Unity Callbacks

		public override void Start()
		{
			base.Start();
		}

		public override void Update()
		{
			base.Update();
		}

		#endregion
		
		#region Methods

		public override void GetHit(float damage, GameObject hitGO)
		{
			base.GetHit(damage, hitGO);
			
		}

		#endregion
		
		//--------------------------------------------------------------------------------------------------------------


		#region Helper
		
		
		#endregion
	}
}


