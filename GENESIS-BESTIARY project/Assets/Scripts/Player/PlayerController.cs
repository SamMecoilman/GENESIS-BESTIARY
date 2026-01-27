using UnityEngine;

namespace GenesisBestiary.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerData data;

        private CharacterController controller;
        private PlayerStateMachine stateMachine;
        private Vector2 moveInput;
        private Vector3 verticalVelocity;
        private Transform cameraTransform;

        public bool HasMoveInput
        {
            get
            {
                if (data == null)
                {
                    return false;
                }

                float deadzone = data.inputDeadzone * data.inputDeadzone;
                return moveInput.sqrMagnitude >= deadzone;
            }
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            stateMachine = new PlayerStateMachine(this);
            cameraTransform = Camera.main != null ? Camera.main.transform : null;
        }

        private void Update()
        {
            ReadInput();

            if (data == null)
            {
                return;
            }

            stateMachine.Tick();
            ApplyGravity();
        }

        private void ReadInput()
        {
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        public Vector3 GetCameraRelativeMove()
        {
            Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            if (cameraTransform == null)
            {
                return input;
            }

            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = cameraTransform.right;
            right.y = 0f;
            right.Normalize();

            return (forward * input.z + right * input.x).normalized;
        }

        public void Move(Vector3 direction)
        {
            Vector3 velocity = direction * data.moveSpeed;
            controller.Move(velocity * Time.deltaTime);
        }

        public void RotateTowards(Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float maxDegrees = data.rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxDegrees);
        }

        private void ApplyGravity()
        {
            if (controller.isGrounded && verticalVelocity.y < 0f)
            {
                verticalVelocity.y = data.groundedGravity;
            }

            verticalVelocity.y += data.gravity * Time.deltaTime;
            controller.Move(verticalVelocity * Time.deltaTime);
        }

#if UNITY_EDITOR
        [ContextMenu("Create Default PlayerData")]
        private void CreateDefaultPlayerData()
        {
            if (data != null)
            {
                return;
            }

            PlayerData asset = ScriptableObject.CreateInstance<PlayerData>();
            const string assetPath = "Assets/ScriptableObjects/Players/PlayerData_Default.asset";
            UnityEditor.AssetDatabase.CreateAsset(asset, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            data = asset;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
