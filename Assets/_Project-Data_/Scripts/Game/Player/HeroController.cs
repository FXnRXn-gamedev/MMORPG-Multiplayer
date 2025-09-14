using System;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

namespace FXnRXn
{
	[RequireComponent(typeof(CharacterController)), RequireComponent(typeof(NavMeshAgent))]
	public class HeroController : NetworkBehaviour
	{
		#region Singleton
		public static HeroController Instance { get; private set; }

		private void Awake()
		{
			if (Instance == null) Instance = this;
		}

		#endregion
		
		
    		
		#region Properties

		[Header("Settings")] 
		[Space(10)] 
		[field: SerializeField] private float moveSpeed = 1f;
		
		[Header("Refference")]
		[Space(10)]
		[field: SerializeField] private GameObject mousePositionPrefab;
		
		[Header("Mouse Input")]
		[Space(10)]
		[field: SerializeField] private LayerMask groundLayerMask;
		
		
		// Private fields
		private Camera camera;
		private RaycastHit hit;
		private NavMeshAgent agent;

		private GameObject targetNow;
		
		#endregion

		#region Network Method

		public override void OnStartLocalPlayer()
		{
			base.OnStartLocalPlayer();
			if(PlayerCameraController.Instance != null) PlayerCameraController.Instance.SetHero(gameObject);
			
		}

		#endregion
		
		#region Unity Callbacks

		private void Start()
		{
			if (agent == null) agent = GetComponent<NavMeshAgent>();
			if (camera == null) camera = PlayerCameraController.Instance.GetMainCamera;

			agent.speed = moveSpeed;
		}

		private void Update()
		{
			if (!isLocalPlayer) return;
			if(camera == null) return;
			
			MovementHandler();
		}

		#endregion
		
		#region Methods

		#region Player Loccomotion

		private void MovementHandler()
		{
			if (InputHandler.Instance.GetRightMouseDown())
			{
				Vector3 pos = RayPosition();
				if (pos != Vector3.zero)
				{
					if (targetNow != null)
					{
						Destroy(targetNow);
						
					}
					targetNow = Instantiate(mousePositionPrefab, pos, Quaternion.identity);
					
					if (agent != null) agent.SetDestination(pos);
				}
				
			}
		}
		
		private Vector3 RayPosition()
		{
			Vector3 tempHitPoint = Vector3.zero;
			if (camera != null)
			{
				Ray ray = camera.ScreenPointToRay(InputHandler.Instance.GetMousePosition());
				Physics.Raycast(ray, out hit, 1000.0f, groundLayerMask);
				if (hit.collider != null)
				{
					if (hit.collider.gameObject.CompareTag("NotMoveTag"))
					{
						tempHitPoint = Vector3.zero;
					}
					else
					{
						tempHitPoint = hit.point;
					}
				}
				else
				{
					tempHitPoint = Vector3.zero;
				}
			}
			return tempHitPoint;
		}

		#endregion

		#endregion
		
		//--------------------------------------------------------------------------------------------------------------


		#region Helper
		
		
		#endregion
	}
}


