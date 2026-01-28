using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
		
		[Header("Debug")]
		[SerializeField] private bool debugInput = true;
		private float debugTimer = 0f;
		private int onLookCallCount = 0;

#if ENABLE_INPUT_SYSTEM
		private PlayerInput playerInput;
		
		private void Awake()
		{
			playerInput = GetComponent<PlayerInput>();
		}
		
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			onLookCallCount++;
			Vector2 inputValue = value.Get<Vector2>();
			
			if (debugInput && inputValue.sqrMagnitude > 0.001f)
			{
				Debug.Log($"[StarterAssetsInputs] OnLook called #{onLookCallCount}: {inputValue}");
			}
			
			if(cursorInputForLook)
			{
				LookInput(inputValue);
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
#endif

		private void Update()
		{
			if (debugInput)
			{
				debugTimer += Time.deltaTime;
				if (debugTimer >= 2f)
				{
					debugTimer = 0f;
					
#if ENABLE_INPUT_SYSTEM
					// Check PlayerInput status
					string piStatus = "N/A";
					string currentScheme = "N/A";
					string currentMap = "N/A";
					bool inputActive = false;
					
					if (playerInput != null)
					{
						inputActive = playerInput.inputIsActive;
						currentScheme = playerInput.currentControlScheme ?? "null";
						currentMap = playerInput.currentActionMap?.name ?? "null";
						piStatus = $"Active:{inputActive}, Scheme:{currentScheme}, Map:{currentMap}";
					}
					
					// Direct mouse delta read
					var mouse = Mouse.current;
					Vector2 rawDelta = mouse != null ? mouse.delta.ReadValue() : Vector2.zero;
					bool mouseDetected = mouse != null;
					
					Debug.Log($"[StarterAssetsInputs] Status - Look:{look}, OnLookCalls:{onLookCallCount}, Mouse:{mouseDetected}, RawDelta:{rawDelta}, PI:{piStatus}");
#else
					Debug.Log($"[StarterAssetsInputs] Status - Look:{look}, OnLookCalls:{onLookCallCount}, INPUT_SYSTEM_DISABLED");
#endif
				}
			}
		}

		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
			if (debugInput)
			{
				Debug.Log($"[StarterAssetsInputs] OnApplicationFocus: {hasFocus}");
			}
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}
