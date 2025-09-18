using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


namespace FXnRXn
{
	public enum EnemyType
	{
		Idle,
		Move,
		Attack,
		Dead
	}
	
	
	[RequireComponent(typeof(NavMeshAgent))]
	public class EnemyControllerBase : NetworkBehaviour
	{
		#region Singleton
		public static EnemyControllerBase Instance { get; private set; }

		private void Awake()
		{
			if (Instance == null) Instance = this;
		}

		#endregion
		
		#region Animation Variable Hashes
		
		private readonly int isMovingHash = Animator.StringToHash("IsMoving");
		private readonly int attackHash = Animator.StringToHash("Attack");
		#endregion
    		
		#region Properties
		
		[Header("Enemy Select Plane")]
		[Space(10)]
		[field: SerializeField] private GameObject			defaultPlane;
		[field: SerializeField] private GameObject			plane2;

		[Header("Enemy Stat")] 
		[Space(10)] 
		[field: SerializeField] private float				moveSpeed = 4;
		
		[Range(100, 500)][field: SerializeField] private float	HP = 100.0f;
		[field: SerializeField] private float				DEF = 10.0f;
		[field: SerializeField] private float				attackDistance = 1.5f;
		[field: SerializeField] private float				trackingDistance = 20.0f;

		[Header("Attack")] 
		[Space(10)]
		[field: SerializeField] private float				ATK = 0.0f;
		[field: SerializeField] private GameObject			attackBoxPrefab;
		[field: SerializeField] private Transform			attackBoxPoint;
		
		
		[Header("UI")] 
		[Space(10)] 
		[field: SerializeField] private Slider				HPBar;
		
		// Private fields
		[SyncVar(hook = nameof(HPBarUpdate))] 
		public float currentHP;
		public bool CanMoveAndAttack = false;
		
		// Private fields
		private NavMeshAgent agent;
		private GameObject	targetPlayer;
		private Animator enemyAnimator;
		private EnemyType enemyState = EnemyType.Idle;

		private bool hasTarget = false;
		private bool doAttack = false;
		private float attackCooltime = 1.0f;
		private float attackDamageTime = 0.8f;
		
		#endregion
		
		#region Unity Callbacks

		public virtual void Start()
		{
			if (agent == null) agent = GetComponent<NavMeshAgent>();
			if (enemyAnimator == null)
			{
				enemyAnimator = GetComponentInChildren<Animator>();
			}

			
			currentHP = HP;
			HPBar.value = currentHP / HP;

			if (isServer)
			{
				if (CanMoveAndAttack)
				{
					EnemyTypeIdleOnceStart();	
				}
			}
		}


		public virtual void Update()
		{
			if(!isServer) return;
			
			if (CanMoveAndAttack)
			{
				EnemyStateFunction();
			}
			
		}

		#endregion
		
		#region Methods

		public virtual void Move()
		{
			if (targetPlayer != null)
			{
				if(agent != null) agent.SetDestination(targetPlayer.transform.position);
				// Check if the agent is actually moving
				//bool isMoving = agent.velocity.magnitude > 0.1f && !agent.isStopped;

				if(enemyAnimator != null) CmdSetEnemyAnimBool(isMovingHash, true);
			}
		}


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
			if (hitGO.GetComponent<HeroController>())
			{
				targetPlayer = hitGO;
				hasTarget = true;
			}
			// Debug.Log($"Enemy : {hitGO.name} Get Hit {damage} HP : {currentHP} DEF : {DEF} {gameObject.name} {gameObject.GetInstanceID()}");
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

		public void ResetDoAttack()
		{
			doAttack = false;
		}

		public virtual void CreateAttackBox()
		{
				if (attackBoxPrefab != null)
				{
					GameObject go = Instantiate(attackBoxPrefab, attackBoxPoint.position, Quaternion.identity);
					EnemyAttackBox attackBoxSC = go.GetComponent<EnemyAttackBox>();
					attackBoxSC.Enemy = gameObject;
					attackBoxSC.Damage = ATK;
				}
		}


		//--------------------------------------------------------------------------------------------------------------

		#region Enemy State
		
		public virtual void EnemyStateFunction()
		{
			switch (enemyState)
			{
				case EnemyType.Idle:
				{ EnemyTypeIdleUpdate(); }
					break;
				case EnemyType.Move:
				{ EnemyTypeMoveUpdate(); }
					break;
				case EnemyType.Attack:
				{ EnemyTypeAttackUpdate(); }
					break;
				case EnemyType.Dead:
				{ EnemyTypeDeadUpdate(); }
					break;
			}
		}

		public virtual void SwitchNewEnemyState(EnemyType newEnemyState)
		{
			EnemyType oldEnemyState = newEnemyState;
			switch (oldEnemyState)
			{
				case EnemyType.Idle:
					EnemyTypeIdleEnd();
					break;
				case EnemyType.Move:
					EnemyTypeMoveEnd();
					break;
				case EnemyType.Attack:
					EnemyTypeAttackEnd();
					break;
				case EnemyType.Dead:
					EnemyTypeDeadEnd();
					break;
					
			}

			enemyState = newEnemyState;
			switch (enemyState)
			{
				case EnemyType.Idle:
					EnemyTypeIdleOnceStart();
					break;
				case EnemyType.Move:
					EnemyTypeMoveOnceStart();
					break;
				case EnemyType.Attack:
					EnemyTypeAttackOnceStart();
					break;
				case EnemyType.Dead:
					EnemyTypeDeadOnceStart();
					break;
			}
		}
		
		
		
		//--> Idle
		public virtual void EnemyTypeIdleOnceStart()
		{
			if(agent != null) agent.speed = moveSpeed;
			if(enemyAnimator != null) CmdSetEnemyAnimBool(isMovingHash, false);
		}
		public virtual void EnemyTypeIdleUpdate()
		{
			if (hasTarget)
			{
				SwitchNewEnemyState(EnemyType.Move);
			}
		}
		public virtual void EnemyTypeIdleEnd()
		{
			
		}
		
		//--> Move
		public virtual void EnemyTypeMoveOnceStart()
		{
			if(enemyAnimator != null) CmdSetEnemyAnimBool(isMovingHash, true);
			
		}
		public virtual void EnemyTypeMoveUpdate()
		{
			if(agent != null) agent.SetDestination(targetPlayer.transform.position);

			if (Vector3.Distance(transform.position, targetPlayer.transform.position) <= attackDistance)
			{
				SwitchNewEnemyState(EnemyType.Attack);
			}
			else if (Vector3.Distance(transform.position, targetPlayer.transform.position) >= trackingDistance)
			{
				SwitchNewEnemyState(EnemyType.Idle);
				if(agent != null) agent.SetDestination(transform.position);
				currentHP = HP;
				targetPlayer = null;
				hasTarget = false;
			}
		}
		public virtual void EnemyTypeMoveEnd()
		{
			if(enemyAnimator != null) CmdSetEnemyAnimBool(isMovingHash, false);
		}
		
		//--> Attack
		public virtual void EnemyTypeAttackOnceStart()
		{
			
		}
		public virtual void EnemyTypeAttackUpdate()
		{
			if (!doAttack && (Vector3.Distance(transform.position, targetPlayer.transform.position) <= attackDistance))
			{
				doAttack = true;
				CmdSetEnemyAnimTrigger(attackHash);
				transform.rotation = Quaternion.Euler(transform.position.x, (Quaternion.LookRotation(targetPlayer.transform.position - transform.position)).eulerAngles.y, transform.position.z);
				Invoke("CreateAttackBox", attackDamageTime);
				Invoke("ResetDoAttack", attackCooltime);
			}
			else if (!doAttack && (Vector3.Distance(transform.position, targetPlayer.transform.position) > attackDistance))
			{
				SwitchNewEnemyState(EnemyType.Idle);
			}
		}
		public virtual void EnemyTypeAttackEnd()
		{
			Debug.Log("EnemyTypeAttackEnd");
		}
		
		//--> Dead
		public virtual void EnemyTypeDeadOnceStart()
		{
			Debug.Log("EnemyTypeDeadOnceStart");
		}
		public virtual void EnemyTypeDeadUpdate()
		{
			Debug.Log("EnemyTypeDeadUpdate");
		}
		public virtual void EnemyTypeDeadEnd()
		{
			Debug.Log("EnemyTypeDeadEnd");
			
		}
		
		
		#endregion
		
		
		
		
		#endregion
		
		
		#region Server/Client RPC
		
		//--------------------------------------------------------------------------------------------------------------
		//-->									ANIMATION															 <--
		//--------------------------------------------------------------------------------------------------------------
		
		public void CmdSetEnemyAnimBool(int name, bool value)
		{
			RpcSetEnemyAnimBool(name, value);
		}
		
		[ClientRpc]
		public void RpcSetEnemyAnimBool(int name, bool value)
		{
			if (enemyAnimator != null) enemyAnimator.SetBool(name, value);
		}
		
		
		public void CmdSetEnemyAnimTrigger(int name)
		{
			RpcSetEnemyAnimTrigger(name);
		}


		[ClientRpc]
		public void RpcSetEnemyAnimTrigger(int name)
		{
			if (enemyAnimator != null) enemyAnimator.SetTrigger(name);
		}
		#endregion
		
		
		
		//--------------------------------------------------------------------------------------------------------------

		
		#region Helper

		public EnemyType GetEnemyState() => enemyState;

		#endregion
	}
}


