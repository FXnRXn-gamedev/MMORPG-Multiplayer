using System;
using Mirror;
using UnityEngine;

namespace FXnRXn
{
	public class AnimationEventHandler : NetworkBehaviour
	{
    		
		#region Properties

		public static event Action<NetworkIdentity, string> onAttackHitEvent;
		public static event Action<NetworkIdentity, string> onAttackAnimationEndEvent;
		
		#endregion
		
		#region Unity Callbacks
		
		private void OnDestroy()
		{
			// Clean up static events
			onAttackHitEvent = null;
			onAttackAnimationEndEvent = null;
		}
		#endregion
		
		#region Methods

		public void AttackHit(string name)
		{
			if(!isLocalPlayer) return;
			
			// Trigger the event locally first
			onAttackHitEvent?.Invoke(netIdentity, name);
			
			// Then sync to other clients
			CmdTriggerAttackHitEvent(name);
			
		}


		public void AttackAnimationEnd(string name)
		{
			if(!isLocalPlayer) return;
			
			// Trigger the event locally first
			onAttackAnimationEndEvent?.Invoke(netIdentity, name);
			
			// Then sync to other clients
			CmdTriggerAttackAnimationEndEvent(name);
		}
		

		#endregion
		
		//--------------------------------------------------------------------------------------------------------------

		#region Server/Client RPC

		//--> SERVER
		[Command]
		void CmdTriggerAttackAnimationEndEvent(string attackAnimName)
		{
			RpcTriggerAttackAnimationEndEvent(attackAnimName);
		}
		
		[Command]
		void CmdTriggerAttackHitEvent(string attackAnimName)
		{
			RpcTriggerAttackHitEvent(attackAnimName);
		}
		
		
		//--> CLIENT
		[ClientRpc]
		void RpcTriggerAttackAnimationEndEvent(string attackAnimName)
		{
			if (!isLocalPlayer)
			{
				onAttackAnimationEndEvent?.Invoke(netIdentity, attackAnimName);
			}
		}
		
		
		[ClientRpc]
		void RpcTriggerAttackHitEvent(string attackAnimName)
		{
			if (!isLocalPlayer)
			{
				onAttackHitEvent?.Invoke(netIdentity, attackAnimName);
			}
		}
		#endregion

		#region Helper
		
		
		#endregion
	}
}


