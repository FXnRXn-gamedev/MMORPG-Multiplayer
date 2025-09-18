using System;
using Cysharp.Threading.Tasks;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

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
		
		#region Animation Variable Hashes
		
		private readonly int isRunningHash = Animator.StringToHash("IsRunning");
		private readonly int isAttackingHash = Animator.StringToHash("Attack");
		
		#endregion
    		
		#region Properties

		[Header("Settings")] 
		[Space(10)] 
		[field: SerializeField] private float				moveSpeed = 1f;
		[field: SerializeField] private float				attackRange = 5.0f;
		
		[Header("Stat")] 
		[Space(10)] 
		[field: SerializeField] private float				ATK_damage;
		[field: SerializeField] private float				HP = 100.0f;
		[field: SerializeField] private float				DEF = 0.0f;
		[Header("Refference")]
		[Space(10)]
		[field: SerializeField] private GameObject			mousePositionPrefab;
		[field: SerializeField] private GameObject			defaultPlane;
		[field: SerializeField] private GameObject			plane2;
		
		[Header("Mouse Input")]
		[Space(10)]
		[field: SerializeField] private LayerMask			groundLayerMask;
		
		[Header("Attack")]
		[Space(10)]
		[field: SerializeField] private LayerMask			enemyLayerMask;
		[field: SerializeField] private Transform			attackHitBoxTransform;
		[field: SerializeField] private GameObject			attackHitBox;
		
		
		
		
		
		// Private fields
		private Camera camera;
		private RaycastHit hit;
		private RaycastHit attackHit;
		private NavMeshAgent agent;
		private Animator animator;

		private GameObject targetNow;
		private GameObject targetAttackActor;
		
		[SyncVar(hook = "HPBarUpdate")] public float currentHP;
		private bool isAttack = false;
		[SyncVar] private bool canAttack = true;
		private Slider playerHealthXPSlider_UI;
		#endregion

		#region Network Method
		public override void OnStartLocalPlayer()
		{
			base.OnStartLocalPlayer();
			if(PlayerCameraController.Instance != null) PlayerCameraController.Instance.SetHero(gameObject); 
			
		}

		public override void OnStopLocalPlayer()
		{
			base.OnStopLocalPlayer();
			
		}

		#endregion
		
		#region Unity Callbacks

		private void Start()
		{	
			if (agent == null) agent = GetComponent<NavMeshAgent>();
			if (camera == null) camera = PlayerCameraController.Instance.GetMainCamera;
			if (animator == null) animator = GetComponentInChildren<Animator>();
			
			agent.speed = moveSpeed;
			currentHP = HP;
			if (isLocalPlayer)
			{
				SetPlane();
				if (playerHealthXPSlider_UI == null) playerHealthXPSlider_UI = UIManager.Instance?.GetPlayerXPSlider;
			}
			
			HPBarUpdate(0, 0);
			
			AnimationEventHandler.onAttackAnimationEndEvent += OnAttackAnimationEnd;
			AnimationEventHandler.onAttackHitEvent += OnAttackHit;
		}

		private void OnDisable()
		{
			AnimationEventHandler.onAttackAnimationEndEvent -= OnAttackAnimationEnd;
			AnimationEventHandler.onAttackHitEvent -= OnAttackHit;
		}

		private void Update()
		{
			if (!isLocalPlayer) return;
			if(camera == null) return;
			
			MovementHandler();
			HandleAttackInput();
			
		}

		#endregion
		
		#region Methods

		#region Player Loccomotion

		private void MovementHandler()
		{
			if(isAttack) return;
			
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
					if(animator != null) CmdSetAnimBool(isRunningHash, true);
				}
			}

			if (targetNow != null)
			{
				if (Vector3.Distance(transform.position, targetNow.transform.position) <= 1.0f)
				{
					Destroy(targetNow);
					if(animator != null) CmdSetAnimBool(isRunningHash, false);
				}
			}
			
		}
		
		private Vector3 RayPosition()
		{
			if (camera == null) return Vector3.zero;
			
			Ray ray = camera.ScreenPointToRay(InputHandler.Instance.GetMousePosition());
			if (Physics.Raycast(ray, out hit, 1000.0f, groundLayerMask))
			{
				if (hit.collider.gameObject.CompareTag("NotMoveTag"))
				{
					return Vector3.zero;
				}
				return hit.point;
			}
			return Vector3.zero;
		}

		#endregion

		#region Player Attack

		private void HandleAttackInput()
		{
			// Only process attack input if not currently attacking and can attack
			if (InputHandler.Instance.GetLeftMouseDown() && canAttack)//&& !isAttacking
			{
				GameObject enemyTarget = GetEnemyFromRaycast();
				if (enemyTarget != null && IsEnemyInRange(enemyTarget))
				{
					InitiateAttack(enemyTarget);
				}
			}
		}

		private void InitiateAttack(GameObject enemy)
		{
			if (enemy == null) return; // || isAttacking
			
			// Set color to enemy[Red : Attack]
			if (targetAttackActor != null)
			{
				targetAttackActor.GetComponent<EnemyControllerBase>()?.DoNotChoose();
			}
			
			// Set attack target and state
			targetAttackActor = enemy;
			targetAttackActor.GetComponent<EnemyControllerBase>()?.BeChoose();
			
			// Face the enemy
			Vector3 directionToEnemy = (enemy.transform.position - transform.position).normalized;
			directionToEnemy.y = 0; // Keep only horizontal rotation
			if (directionToEnemy != Vector3.zero)
			{
				transform.rotation = Quaternion.Euler(transform.position.x, 
					(Quaternion.LookRotation(targetAttackActor.transform.position - transform.position)).eulerAngles.y, 
					transform.position.z);
			}
			
			// Player can attack
			isAttack = true;
			canAttack = false;
			
			// Stop movement
			if (agent != null && agent.isActiveAndEnabled)
			{
				agent.ResetPath();
			}
			// Stop running animation
			if (animator != null)
			{
				CmdSetAnimBool(isRunningHash, false);
			}
			
			// Trigger attack animation
			CmdSetAnimTrigger(isAttackingHash);
			
			
			//Invoke("CmdCreateAttackHitBox", 0.5f);
			Invoke("ResetAttack", 2f);
		
		}
		
		private GameObject GetEnemyFromRaycast()
		{
			if (camera == null) return null;
			
			Ray ray = camera.ScreenPointToRay(InputHandler.Instance.GetMousePosition());
			if (Physics.Raycast(ray, out attackHit, 1000.0f, enemyLayerMask))
			{
				return attackHit.collider.gameObject;
			}
			
			return null;
		}

		private bool IsEnemyInRange(GameObject enemy)
		{
			if (enemy == null) return false;
			
			return Vector3.Distance(transform.position, enemy.transform.position) <= attackRange;
		}

		public void ResetAttack()
		{
			isAttack = false;
		}
		
		// Called by animation events to reset attack state
		public void OnAttackAnimationEnd(NetworkIdentity identity, string attackName)
		{
			if (identity != netIdentity) return;

			canAttack = true;
			
			switch (attackName)
			{
				case "Anim_Attack_4":
					
					break;
				default:
					break;
			}
			
		}
		
		// Called by animation events when attack should deal damage
		public void OnAttackHit(NetworkIdentity identity, string attackName)
		{
			if (identity != netIdentity) return;
			CmdCreateAttackHitBox();
			switch (attackName)
			{
				case "Anim_Attack_4":
					if (isLocalPlayer && targetAttackActor != null && IsEnemyInRange(targetAttackActor))
					{
						// Apply damage logic here
						// Debug.Log($"Attacking {targetAttackActor.name}");
						// Example: targetAttackActor.GetComponent<EnemyHealth>()?.TakeDamage(attackDamage);
					}

					break;
				default:
					break;
			}
		}

		#endregion


		private void SetPlane()
		{
			defaultPlane.SetActive(true);
			plane2.SetActive(false);
		}
		
		public virtual void GetHit(float damage, GameObject hitGO)
		{
			currentHP = Mathf.Max(0, currentHP - Mathf.Max(0, damage - DEF));
			HPBarUpdate(0,0);
			// if (currentHP <= 0)
			// {
			// 	currentHP = 0;
			// 	Destroy(gameObject);
			// }
		}

		public void HPBarUpdate(float HPold, float HPnow)
		{
			if (playerHealthXPSlider_UI != null) playerHealthXPSlider_UI.value = currentHP / HP;
		}

		#region Server/Client RPC

		[Command]
		public void CmdCreateAttackHitBox()
		{
			RpcCreateAttackHitBox();
		}
		
		[ClientRpc]
		public void RpcCreateAttackHitBox()
		{
			GameObject go = Instantiate(attackHitBox, attackHitBoxTransform.position, attackHitBoxTransform.rotation);
			
			var box = go.GetComponent<AttackHitBox>();
			if (box != null)
			{
				box.Hero = gameObject;
				box.Damage = ATK_damage;
			}
		}

		//--------------------------------------------------------------------------------------------------------------
		//-->									ANIMATION															 <--
		//--------------------------------------------------------------------------------------------------------------
		
		[Command]
		public void CmdSetAnimBool(int name, bool value)
		{
			RpcSetAnimBool(name, value);
			//animator.SetBool(name, value);
		}

		[ClientRpc]
		public void RpcSetAnimBool(int name, bool value)
		{
			if (animator != null) animator.SetBool(name, value);
		}
		
		
		[Command]
		public void CmdSetAnimTrigger(int name)
		{
			RpcSetAnimTrigger(name);
			//animator.SetTrigger(name);
		}

		[ClientRpc]
		public void RpcSetAnimTrigger(int name)
		{
			if (animator != null) animator.SetTrigger(name);
		}
		
		#endregion
		
		
		#endregion
		
		//--------------------------------------------------------------------------------------------------------------


		#region Helper
		
		
		#endregion
	}
}


