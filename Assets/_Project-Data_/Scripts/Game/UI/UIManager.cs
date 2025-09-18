using UnityEngine;
using UnityEngine.UI;
using System;


namespace FXnRXn
{
    public class UIManager : MonoBehaviour
    {
        #region Singleton
		public static UIManager Instance { get; private set; }

		private void Awake()
		{
			if (Instance == null) Instance = this;
		}

		#endregion

		#region Properties

		[Header("---Player Health UI---")] 
		[Space(10)] 
		[field: SerializeField] private Slider					playerHealthXPSlider;
		
		[Header("---Other Client Health UI---")] 
		[Space(10)] 
		[field: SerializeField] private Image					client1HealthXPSlider;

		#endregion

        #region Unity Callbacks

        #endregion

        #region Cutom Method

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------

        #region Helper

        public Slider GetPlayerXPSlider => playerHealthXPSlider;

        #endregion

    }
}

