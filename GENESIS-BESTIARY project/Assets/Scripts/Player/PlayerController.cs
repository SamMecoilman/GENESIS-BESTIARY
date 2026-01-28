using GenesisBestiary.Combat;
using UnityEngine;
using System.Linq;

namespace GenesisBestiary.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [SerializeField] private PlayerData data;
        [SerializeField] private PlayerCombat combat;

        private CharacterController controller;
        private PlayerStateMachine stateMachine;
        private Vector2 moveInput;
        private Vector3 verticalVelocity;
        private Transform cameraTransform;
        
        // Stats
        private int currentHealth;
        private float currentStamina;
        private float staminaRegenTimer;
        private float dodgeCooldownTimer;
        private bool isInvincible;
        private bool isDead;
        
        // Input
        private bool attackInput;
        private bool dodgeInput;
        private bool carveInput;
        
        // Carving
        private CarvingPoint currentCarvingPoint;

        // Events
        public System.Action OnDied;

        #region Properties
        public PlayerData Data => data;
        public PlayerCombat Combat => combat;
        
        public int CurrentHealth => currentHealth;
        public float CurrentStamina => currentStamina;
        public bool IsInvincible => isInvincible;
        
        public bool AttackInput => attackInput;
        public bool DodgeInput => dodgeInput;
        public bool CarveInput => carveInput;
        public CarvingPoint CurrentCarvingPoint => currentCarvingPoint;
        
        public bool CanDodge => currentStamina >= data.dodgeStaminaCost && 
                                dodgeCooldownTimer <= 0f;

        public bool HasMoveInput
        {
            get
            {
                if (data == null) return false;
                float deadzone = data.inputDeadzone * data.inputDeadzone;
                return moveInput.sqrMagnitude >= deadzone;
            }
        }
        #endregion

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            
            if (combat == null)
            {
                combat = GetComponent<PlayerCombat>();
            }
            
            stateMachine = new PlayerStateMachine(this);
            cameraTransform = Camera.main != null ? Camera.main.transform : null;
            
            InitializeStats();
        }

        private void InitializeStats()
        {
            if (data == null) return;
            
            currentHealth = data.maxHealth;
            currentStamina = data.maxStamina;
            isDead = false;
        }

        private void Update()
        {
            ReadInput();
            
            if (data == null) return;

            UpdateTimers();
            UpdateStamina();
            stateMachine.Tick();
            ApplyGravity();
            
            ClearInputs();
        }

        private void ReadInput()
        {
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            
            if (Input.GetButtonDown("Fire1"))
            {
                attackInput = true;
            }
            
            if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
            {
                dodgeInput = true;
            }
            
            // Carve input (E key or interact button)
            carveInput = Input.GetKey(KeyCode.E);
            
            // Check for carve start
            if (Input.GetKeyDown(KeyCode.E))
            {
                TryStartCarving();
            }
        }
        
        private void ClearInputs()
        {
            attackInput = false;
            dodgeInput = false;
            // Note: carveInput is not cleared as it's held
        }
        
        private void TryStartCarving()
        {
            // Find nearby carving points
            var carvingPoints = Physics.OverlapSphere(transform.position, 3f)
                .Select(c => c.GetComponent<CarvingPoint>())
                .Where(cp => cp != null && cp.CanCarve)
                .OrderBy(cp => Vector3.Distance(transform.position, cp.transform.position))
                .ToArray();
            
            if (carvingPoints.Length > 0)
            {
                currentCarvingPoint = carvingPoints[0];
                stateMachine.RequestStateChange(PlayerState.Carve);
            }
        }
        
        public void ClearCarvingPoint()
        {
            currentCarvingPoint = null;
        }
        
        private void UpdateTimers()
        {
            if (dodgeCooldownTimer > 0f)
            {
                dodgeCooldownTimer -= Time.deltaTime;
            }
        }
        
        private void UpdateStamina()
        {
            if (staminaRegenTimer > 0f)
            {
                staminaRegenTimer -= Time.deltaTime;
                return;
            }
            
            if (currentStamina < data.maxStamina)
            {
                currentStamina = Mathf.Min(
                    data.maxStamina,
                    currentStamina + data.staminaRegenRate * Time.deltaTime
                );
            }
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
        
        public void MoveRaw(Vector3 velocity)
        {
            controller.Move(velocity);
        }

        public void RotateTowards(Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0f) return;

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
        
        public void ConsumeStamina(float amount)
        {
            currentStamina = Mathf.Max(0f, currentStamina - amount);
            staminaRegenTimer = data.staminaRegenDelay;
            dodgeCooldownTimer = data.dodgeCooldown;
        }
        
        public void SetInvincible(bool invincible)
        {
            isInvincible = invincible;
        }
        
        #region IDamageable
        public void TakeDamage(int damage, Vector3 sourcePosition)
        {
            if (isDead) return;
            if (isInvincible) return;
            
            currentHealth -= damage;
            
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
            else
            {
                // Trigger stagger state (to be implemented)
                Debug.Log($"Player took {damage} damage. HP: {currentHealth}/{data.maxHealth}");
            }
        }
        
        private void Die()
        {
            if (isDead) return;
            isDead = true;
            OnDied?.Invoke();
            Debug.Log("Player died!");
            stateMachine.RequestStateChange(PlayerState.Dead);
        }
        #endregion

#if UNITY_EDITOR
        [ContextMenu("Create Default PlayerData")]
        private void CreateDefaultPlayerData()
        {
            if (data != null) return;

            PlayerData asset = ScriptableObject.CreateInstance<PlayerData>();
            const string assetPath = "Assets/ScriptableObjects/Players/PlayerData_Default.asset";
            
            string directory = System.IO.Path.GetDirectoryName(assetPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            UnityEditor.AssetDatabase.CreateAsset(asset, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            data = asset;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
