using UnityEngine;
using UnityEngine.InputSystem;
using GenesisBestiary.Combat;
using StarterAssets;
using System.Linq;

namespace GenesisBestiary.Player
{
    /// <summary>
    /// MH固有の処理を担当するコントローラー
    /// Starter AssetsのThirdPersonControllerと併用する
    /// </summary>
    [RequireComponent(typeof(ThirdPersonController))]
    [RequireComponent(typeof(StarterAssetsInputs))]
    public class HunterController : MonoBehaviour, IDamageable
    {
        [Header("References")]
        [SerializeField] private PlayerData data;
        [SerializeField] private PlayerCombat combat;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        // デフォルト値（dataがnullの場合に使用）
        private const int DEFAULT_MAX_HEALTH = 100;
        private const float DEFAULT_MAX_STAMINA = 100f;
        private const float DEFAULT_STAMINA_REGEN_RATE = 10f;
        private const float DEFAULT_STAMINA_REGEN_DELAY = 1f;
        private const float DEFAULT_DODGE_DISTANCE = 3f;
        private const float DEFAULT_DODGE_DURATION = 0.4f;
        private const float DEFAULT_DODGE_IFRAME_DURATION = 0.2f;
        private const float DEFAULT_DODGE_STAMINA_COST = 25f;
        private const float DEFAULT_DODGE_COOLDOWN = 0.1f;
        
        // Components
        private ThirdPersonController tpc;
        private StarterAssetsInputs starterInputs;
        private CharacterController characterController;
        private Animator animator;
        
        // State Machine
        private HunterStateMachine stateMachine;
        
        // Stats
        private int currentHealth;
        private float currentStamina;
        private float staminaRegenTimer;
        private bool isDead;
        
        // Dodge
        private float dodgeCooldownTimer;
        private bool isInvincible;
        private Vector3 dodgeDirection;
        private float dodgeTimer;
        
        // Carving
        private CarvingPoint currentCarvingPoint;

        // Events
        public System.Action OnDied;
        
        // Input (new Input System)
        private bool attackInput;
        private bool dodgeInput;
        private bool carveInputHeld;
        
        #region Properties
        public PlayerData Data => data;
        public PlayerCombat Combat => combat;
        public ThirdPersonController TPC => tpc;
        public StarterAssetsInputs StarterInputs => starterInputs;
        public CharacterController CharacterController => characterController;
        public Animator Animator => animator;
        
        public int CurrentHealth => currentHealth;
        public int MaxHealth => data != null ? data.maxHealth : DEFAULT_MAX_HEALTH;
        public float CurrentStamina => currentStamina;
        public float MaxStamina => data != null ? data.maxStamina : DEFAULT_MAX_STAMINA;
        public bool IsInvincible => isInvincible;
        public bool IsDead => isDead;
        
        public bool AttackInput => attackInput;
        public bool DodgeInput => dodgeInput;
        public bool CarveInputHeld => carveInputHeld;
        public CarvingPoint CurrentCarvingPoint => currentCarvingPoint;
        
        public HunterState CurrentState => stateMachine?.CurrentKey ?? HunterState.Locomotion;
        
        // データアクセサ（nullセーフ）
        private float DodgeStaminaCost => data != null ? data.dodgeStaminaCost : DEFAULT_DODGE_STAMINA_COST;
        private float DodgeCooldown => data != null ? data.dodgeCooldown : DEFAULT_DODGE_COOLDOWN;
        private float DodgeDistance => data != null ? data.dodgeDistance : DEFAULT_DODGE_DISTANCE;
        private float DodgeDuration => data != null ? data.dodgeDuration : DEFAULT_DODGE_DURATION;
        private float DodgeIFrameDuration => data != null ? data.dodgeIFrameDuration : DEFAULT_DODGE_IFRAME_DURATION;
        private float StaminaRegenRate => data != null ? data.staminaRegenRate : DEFAULT_STAMINA_REGEN_RATE;
        private float StaminaRegenDelay => data != null ? data.staminaRegenDelay : DEFAULT_STAMINA_REGEN_DELAY;
        
        public bool CanDodge => currentStamina >= DodgeStaminaCost && dodgeCooldownTimer <= 0f;
        
        public bool CanMove => stateMachine?.CanMove ?? true;
        
        public Vector3 DodgeDirection => dodgeDirection;
        public float DodgeTimer => dodgeTimer;
        #endregion
        
        #region Unity Lifecycle
        private void Awake()
        {
            tpc = GetComponent<ThirdPersonController>();
            starterInputs = GetComponent<StarterAssetsInputs>();
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            
            // PlayerInputをアクティブ化
            var playerInput = GetComponent<PlayerInput>();
            if (playerInput != null && !playerInput.inputIsActive)
            {
                playerInput.ActivateInput();
            }
            
            if (combat == null)
                combat = GetComponent<PlayerCombat>();
            
            // データがない場合、Resourcesから自動ロード試行
            if (data == null)
            {
                data = Resources.Load<PlayerData>("PlayerData_Default");
                if (data == null)
                {
                    Debug.LogWarning("HunterController: PlayerData not assigned. Using default values.");
                }
            }
            
            stateMachine = new HunterStateMachine(this);
            
            InitializeStats();
        }
        
        
        
        private void Update()
        {
            UpdateTimers();
            UpdateStamina();
            stateMachine.Tick();
            UpdateTPCMovement();
            
            ClearInputs();
        }
        #endregion
        
        #region Initialization
        private void InitializeStats()
        {
            currentHealth = MaxHealth;
            currentStamina = MaxStamina;
            isDead = false;
        }
        #endregion
        
        #region Input Handling
        // SendMessages モード用（StarterAssetsInputsと同じ方式）
        public void OnAttack(InputValue value)
        {
            if (value.isPressed)
            {
                attackInput = true;
                // コンボ用に入力をバッファ
                if (combat != null)
                {
                    combat.TryBufferAttack();
                }
            }
        }
        
        public void OnDodge(InputValue value)
        {
            if (value.isPressed) dodgeInput = true;
        }
        
        public void OnCarve(InputValue value)
        {
            carveInputHeld = value.isPressed;
            if (value.isPressed) TryStartCarving();
        }
        
        // Jump を回避として使う（Starter Assets の Jump を無効化）
        public void OnJump(InputValue value)
        {
            if (value.isPressed)
            {
                dodgeInput = true;
                // Starter Assets のジャンプを無効化
                if (starterInputs != null)
                {
                    starterInputs.jump = false;
                }
            }
        }
        
        private void ClearInputs()
        {
            attackInput = false;
            dodgeInput = false;
            // carveInputHeld は held なのでクリアしない
        }
        #endregion
        
        #region Movement Control
        private void UpdateTPCMovement()
        {
            // ステートによって移動を無効化
            if (!CanMove && tpc != null)
            {
                // ThirdPersonControllerの入力を無効化
                starterInputs.move = Vector2.zero;
            }
        }
        
        public void MoveRaw(Vector3 velocity)
        {
            if (characterController != null)
            {
                characterController.Move(velocity);
            }
        }
        
        public Vector3 GetMoveDirection()
        {
            if (starterInputs == null) return Vector3.zero;
            
            Vector3 inputDirection = new Vector3(starterInputs.move.x, 0f, starterInputs.move.y).normalized;
            
            if (inputDirection.sqrMagnitude < 0.01f)
                return transform.forward;
            
            // カメラ基準に変換
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Vector3 forward = mainCamera.transform.forward;
                forward.y = 0f;
                forward.Normalize();
                
                Vector3 right = mainCamera.transform.right;
                right.y = 0f;
                right.Normalize();
                
                return (forward * inputDirection.z + right * inputDirection.x).normalized;
            }
            
            return inputDirection;
        }
        #endregion
        
        #region Stamina
        private void UpdateTimers()
        {
            if (dodgeCooldownTimer > 0f)
                dodgeCooldownTimer -= Time.deltaTime;
        }
        
        private void UpdateStamina()
        {
            if (staminaRegenTimer > 0f)
            {
                staminaRegenTimer -= Time.deltaTime;
                return;
            }
            
            if (currentStamina < MaxStamina)
            {
                currentStamina = Mathf.Min(
                    MaxStamina,
                    currentStamina + StaminaRegenRate * Time.deltaTime
                );
            }
        }
        
        public void ConsumeStamina(float amount)
        {
            currentStamina = Mathf.Max(0f, currentStamina - amount);
            staminaRegenTimer = StaminaRegenDelay;
        }
        #endregion
        
        #region Dodge
        public void StartDodge()
        {
            ConsumeStamina(DodgeStaminaCost);
            dodgeCooldownTimer = DodgeCooldown;
            SetInvincible(true);
            
            // 入力方向 or 前方
            dodgeDirection = starterInputs.move.sqrMagnitude > 0.01f
                ? GetMoveDirection()
                : transform.forward;
            
            dodgeDirection.Normalize();
            dodgeTimer = 0f;
        }
        
        public void UpdateDodge()
        {
            dodgeTimer += Time.deltaTime;
            
            // 無敵時間終了
            if (dodgeTimer >= DodgeIFrameDuration)
            {
                SetInvincible(false);
            }
            
            // 移動
            if (dodgeTimer < DodgeDuration)
            {
                float speed = DodgeDistance / DodgeDuration;
                MoveRaw(dodgeDirection * speed * Time.deltaTime);
            }
        }
        
        public bool IsDodgeComplete()
        {
            return dodgeTimer >= DodgeDuration;
        }
        
        public void SetInvincible(bool invincible)
        {
            isInvincible = invincible;
        }
        #endregion
        
        #region Carving
        private void TryStartCarving()
        {
            var carvingPoints = Physics.OverlapSphere(transform.position, 3f)
                .Select(c => c.GetComponent<CarvingPoint>())
                .Where(cp => cp != null && cp.CanCarve)
                .OrderBy(cp => Vector3.Distance(transform.position, cp.transform.position))
                .ToArray();
            
            if (carvingPoints.Length > 0)
            {
                currentCarvingPoint = carvingPoints[0];
                stateMachine.RequestStateChange(HunterState.Carve);
            }
        }
        
        public void ClearCarvingPoint()
        {
            currentCarvingPoint = null;
        }
        #endregion
        
        #region Damage
        public void TakeDamage(int damage, Vector3 sourcePosition)
        {
            if (isDead) return;
            if (isInvincible) return;
            
            currentHealth -= damage;
            
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                isDead = true;
                OnDied?.Invoke();
                stateMachine.RequestStateChange(HunterState.Dead);
            }
            else
            {
                stateMachine.RequestStateChange(HunterState.Stagger);
            }
        }
        
        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);
        }
        #endregion
        
        #region State Machine Access
        public void RequestStateChange(HunterState state)
        {
            stateMachine.RequestStateChange(state);
        }
        #endregion
        
        #region Debug
        private void OnGUI()
        {
            if (!showDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Label($"State: {CurrentState}");
            GUILayout.Label($"HP: {currentHealth}/{MaxHealth}");
            GUILayout.Label($"Stamina: {currentStamina:F0}/{MaxStamina}");
            GUILayout.Label($"CanMove: {CanMove} | CanDodge: {CanDodge}");
            GUILayout.Label($"Invincible: {isInvincible}");
            GUILayout.EndArea();
        }
        #endregion
    }
}
