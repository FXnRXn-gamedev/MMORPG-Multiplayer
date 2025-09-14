using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using TouchPhase = UnityEngine.TouchPhase;

namespace FXnRXn
{
	public class InputHandler : NetworkBehaviour
	{
		#region Singleton
		public static InputHandler Instance { get; private set; }

		private void Awake()
		{
			if (Instance == null) Instance = this;
		}

		#endregion
    		
		#region Properties
		[Header("Refference :")]
		[Space(10)]
		[field: SerializeField] private InputActionAsset inputActionAsset;
		
		[Header("Mouse Cursor Settings")]
		[Space(10)]
		[field: SerializeField] private bool cursorLocked = false;
		
		[Header("Mobile Touch Settings")]
		[Space(10)]
		[field: SerializeField] private float touchSensitivity = 2.0f;
		[field: SerializeField] private float touchDeadZone = 50f;

		
		
		//--> Private fields
		
		// Move & Look input fields
		private Vector2	move;
		private Vector2	look;
		private Vector2 mousePosition;
		private bool rightMouseClicked;
		
		// Touch input fields
		private Vector2 touchStartPos;
		private Vector2 touchCurrentPos;
		private bool isTouching;
		private int activeTouchId = -1;
		
		// Touch input actions
		private InputAction moveAction;
		private InputAction lookAction;
		private InputAction rightClickAction;
		private InputAction mousePositionAction;

		// // Touch input actions
		private InputAction touchPositionAction;
		private InputAction touchPressAction;

		#endregion
		
		#region Unity Callbacks

		private void Start()
		{
			if ( inputActionAsset != null && isLocalPlayer || isClient)
			{
				
				moveAction = InputSystem.actions.FindAction("Move");
				moveAction.performed += OnMove;
				moveAction.canceled += OnMove;
			
				lookAction = InputSystem.actions.FindAction("Look");
				lookAction.performed += OnLook;
				lookAction.canceled += OnLook;
				
				// Add mouse input actions
				rightClickAction = InputSystem.actions.FindAction("RightClick");
				rightClickAction.performed += OnRightClick;
				
				mousePositionAction = InputSystem.actions.FindAction("MousePosition");
				mousePositionAction.performed += OnMousePosition;
				
				// Add touch input actions
				// touchPositionAction = InputSystem.actions.FindAction("TouchPosition");
				// if (touchPositionAction != null)
				// {
				// 	touchPositionAction.performed += OnTouchPosition;
				// }
				//
				// touchPressAction = InputSystem.actions.FindAction("TouchPress");
				// if (touchPressAction != null)
				// {
				// 	touchPressAction.performed += OnTouchPress;
				// 	touchPressAction.canceled += OnTouchRelease;
				// }


			}
			
		}

		private void OnEnable()
		{
			inputActionAsset.FindActionMap("Player").Enable();
		}

		private void OnDisable()
		{
			inputActionAsset.FindActionMap("Player").Disable();
		}


		private void Update()
		{
			// Handle mobile touch input if no Input System touch actions are set up
			// if (touchPositionAction == null && touchPressAction == null)
			// {
			// 	HandleMobileTouchInput();
			// }

		}

		#endregion
		
		#region Methods
		
#if ENABLE_INPUT_SYSTEM

		public void OnMove(InputAction.CallbackContext ctx)
		{
			MoveInput(ctx.ReadValue<Vector2>());
		}

		public void OnLook(InputAction.CallbackContext ctx)
		{
			LookInput(ctx.ReadValue<Vector2>());

		}
		
		public void OnRightClick(InputAction.CallbackContext ctx)
		{
			rightMouseClicked = ctx.performed;
		}
		
		public void OnMousePosition(InputAction.CallbackContext ctx)
		{
			mousePosition = ctx.ReadValue<Vector2>();
		}
		
		public void OnTouchPosition(InputAction.CallbackContext ctx)
		{
			if (isTouching)
			{
				touchCurrentPos = ctx.ReadValue<Vector2>();
				Vector2 touchDelta = touchCurrentPos - touchStartPos;
				
				// Apply touch sensitivity and convert to look input
				Vector2 lookDelta = touchDelta * touchSensitivity * Time.deltaTime;
				LookInput(lookDelta);
			}
		}
		
		public void OnTouchPress(InputAction.CallbackContext ctx)
		{
			if (ctx.performed)
			{
				isTouching = true;
				touchStartPos = Touchscreen.current.primaryTouch.position.ReadValue();
				touchCurrentPos = touchStartPos;
			}
		}

		public void OnTouchRelease(InputAction.CallbackContext ctx)
		{
			isTouching = false;
			activeTouchId = -1;
		}

#endif
		
		// private void HandleMobileTouchInput()
		// {
		// 	// Handle touch input manually if Input System touch actions aren't configured
		// 	if (Input.touchCount > 0)
		// 	{
		// 		Touch touch = Input.GetTouch(0);
		// 		
		// 		switch (touch.phase)
		// 		{
		// 			case TouchPhase.Began:
		// 				isTouching = true;
		// 				activeTouchId = touch.fingerId;
		// 				touchStartPos = touch.position;
		// 				touchCurrentPos = touch.position;
		// 				break;
		// 				
		// 			case TouchPhase.Moved:
		// 				if (isTouching && activeTouchId == touch.fingerId)
		// 				{
		// 					touchCurrentPos = touch.position;
		// 					Vector2 touchDelta = touchCurrentPos - touchStartPos;
		// 					
		// 					// Only process if touch moved beyond dead zone
		// 					if (touchDelta.magnitude > touchDeadZone)
		// 					{
		// 						Vector2 lookDelta = touchDelta * touchSensitivity * Time.deltaTime;
		// 						LookInput(lookDelta);
		// 						touchStartPos = touchCurrentPos; // Update start position for smooth movement
		// 					}
		// 				}
		// 				break;
		// 				
		// 			case TouchPhase.Ended:
		// 			case TouchPhase.Canceled:
		// 				if (activeTouchId == touch.fingerId)
		// 				{
		// 					isTouching = false;
		// 					activeTouchId = -1;
		// 					LookInput(Vector2.zero); // Reset look input
		// 				}
		// 				break;
		// 		}
		// 		
		// 		// Handle tap as right click equivalent for mobile
		// 		if (touch.phase == TouchPhase.Ended && touch.tapCount == 1)
		// 		{
		// 			Vector2 tapDelta = touch.position - touchStartPos;
		// 			if (tapDelta.magnitude < touchDeadZone) // Only register as tap if within dead zone
		// 			{
		// 				rightMouseClicked = true;
		// 				mousePosition = touch.position;
		// 			}
		// 		}
		// 	}
		// }

		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		#endregion
		
		//--------------------------------------------------------------------------------------------------------------


		#region Helper
		public Vector2 GetMoveAxis() => move;
		public Vector2 GetLookAxis() => look;
		public Vector2 GetMousePosition() => mousePosition;
		public bool GetRightMouseDown() 
		{
			bool clicked = rightMouseClicked;
			rightMouseClicked = false; // Reset after reading
			return clicked;
		}
		
		// public bool IsTouching() => isTouching;
		// public Vector2 GetTouchPosition() => touchCurrentPos;

		
		#endregion
	}
}


