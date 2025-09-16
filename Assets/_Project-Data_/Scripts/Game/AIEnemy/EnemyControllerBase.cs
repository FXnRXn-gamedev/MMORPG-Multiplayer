using System;
using Mirror;
using UnityEngine;
using UnityEngine.UI;


namespace FXnRXn
{
	public class EnemyControllerBase : NetworkBehaviour
	{
		#region Singleton
		public static EnemyControllerBase Instance { get; private set; }

		private void Awake()
		{
			if (Instance == null) Instance = this;
		}

		#endregion
    		
		#region Properties
		
		[Header("Enemy Select Plane")]
		[Space(10)]
		[field: SerializeField] private GameObject			defaultPlane;
		[field: SerializeField] private GameObject			plane2;
		
		[Header("Enemy Stat")]
		[Space(10)]
		[field: SerializeField] private float				ATK = 0.0f;
		[field: SerializeField] private float				HP = 100.0f;
		[field: SerializeField] private float				DEF = 10.0f;

		[Header("UI")] 
		[Space(10)] 
		[field: SerializeField] private Slider				HPBar;
		
		// Private fields
		[SyncVar(hook = nameof(HPBarUpdate))] 
		public float currentHP;
		
		
		
		
		
		
		
		#endregion
		
		#region Unity Callbacks

		public virtual void Start()
		{
			currentHP = HP;
			HPBar.value = currentHP / HP;
		}


		public virtual void Update()
		{
			
		}

		#endregion
		
		#region Methods


		public void BeChoose()
		{
			defaultPlane.SetActive(false);
			plane2.SetActive(true);
		}
		
		public void DoNotChoose()
		{
			defaultPlane.SetActive(true);
			plane2.SetActive(false);
		}

		public virtual void GetHit(float damage, GameObject hitGO)
		{
			currentHP = Mathf.Max(0, currentHP - Mathf.Max(0, damage - DEF));
			HPBarUpdate(0f, 0f);
			Debug.Log($"Enemy : {hitGO.name} Get Hit {damage} HP : {currentHP} DEF : {DEF} {gameObject.name} {gameObject.GetInstanceID()}");
			
			// if (currentHP <= 0)
			// {
			// 	currentHP = 0;
			// 	Destroy(gameObject);
			// }
		}

		public virtual void HPBarUpdate(float HPold, float HPcurrent)
		{
			HPBar.value = currentHP / HP;
		}
		#endregion
		
		//--------------------------------------------------------------------------------------------------------------


		#region Helper
		
		
		#endregion
	}
}


