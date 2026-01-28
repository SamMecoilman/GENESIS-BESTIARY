using GenesisBestiary.Combat;
using GenesisBestiary.Player;
using UnityEngine;
using System.Collections.Generic;

namespace GenesisBestiary.Monster
{
    [RequireComponent(typeof(CharacterController))]
    public class MonsterController : MonoBehaviour, IDamageable
    {
        [SerializeField] private MonsterData data;
        [SerializeField] private MonsterAttackData[] attacks;
        [SerializeField] private CarveItem[] carveRewards;
        
        // デフォルト値（dataがnullの場合に使用）
        private const int DEFAULT_MAX_HEALTH = 1000;
        private const float DEFAULT_MOVE_SPEED = 4f;
        private const float DEFAULT_CHASE_SPEED = 6f;
        private const float DEFAULT_ROTATION_SPEED = 180f;
        private const float DEFAULT_DETECTION_RANGE = 20f;
        private const float DEFAULT_ATTACK_RANGE = 4f;
        private const float DEFAULT_FIELD_OF_VIEW = 120f;
        private const float DEFAULT_FLINCH_THRESHOLD = 100f;
        private const float DEFAULT_ROAM_DISTANCE = 10f;
        private const float DEFAULT_IDLE_TIME = 2f;
        
        private CharacterController controller;
        private MonsterStateMachine stateMachine;
        
        private int currentHealth;
        private float currentFlinchDamage;
        private Transform playerTransform;
        private bool isDead;
        
        // Events
        public System.Action<int> OnDamaged;
        public System.Action OnFlinched;
        public System.Action OnDied;
        public System.Action OnBecameTarget;
        public System.Action<CarvingPoint> OnCarvingPointSpawned;
        
        #region Properties
        public MonsterData Data => data;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => data != null ? data.maxHealth : DEFAULT_MAX_HEALTH;
        public float MoveSpeed => data != null ? data.moveSpeed : DEFAULT_MOVE_SPEED;
        public float ChaseSpeed => data != null ? data.chaseSpeed : DEFAULT_CHASE_SPEED;
        public float RotationSpeed => data != null ? data.rotationSpeed : DEFAULT_ROTATION_SPEED;
        public float DetectionRange => data != null ? data.detectionRange : DEFAULT_DETECTION_RANGE;
        public float AttackRange => data != null ? data.attackRange : DEFAULT_ATTACK_RANGE;
        public float FieldOfView => data != null ? data.fieldOfView : DEFAULT_FIELD_OF_VIEW;
        public float FlinchThreshold => data != null ? data.flinchThreshold : DEFAULT_FLINCH_THRESHOLD;
        public float RoamDistance => data != null ? data.roamDistance : DEFAULT_ROAM_DISTANCE;
        public float IdleTime => data != null ? data.idleTime : DEFAULT_IDLE_TIME;
        public string MonsterName => data != null ? data.monsterName : "Test Monster";
        public bool IsDead => isDead;
        public MonsterState CurrentState => stateMachine?.CurrentKey ?? MonsterState.Idle;
        #endregion

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            stateMachine = new MonsterStateMachine(this);
            
            currentHealth = MaxHealth;
            
            // Find player (HunterController or PlayerController)
            FindPlayer();
            
            if (data == null)
            {
                Debug.LogWarning($"MonsterController: MonsterData not assigned on {gameObject.name}. Using default values.");
            }
        }
        
        private void FindPlayer()
        {
            // まずHunterControllerを探す
            var hunter = FindObjectOfType<HunterController>();
            if (hunter != null)
            {
                playerTransform = hunter.transform;
                return;
            }
            
            // 見つからなければPlayerControllerを探す（後方互換）
            var player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                playerTransform = player.transform;
                return;
            }
            
            // どちらも見つからなければPlayerタグで探す
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        private void Update()
        {
            if (isDead) return;
            
            stateMachine.Tick();
        }

        #region Movement
        public void MoveTowards(Vector3 targetPosition, float speed)
        {
            Vector3 direction = (targetPosition - transform.position);
            direction.y = 0f;
            
            if (direction.sqrMagnitude > 0.01f)
            {
                direction.Normalize();
                
                // Rotate towards target
                RotateTowards(direction);
                
                // Move
                Vector3 velocity = direction * speed;
                controller.Move(velocity * Time.deltaTime);
            }
            
            // Apply gravity
            controller.Move(Vector3.down * 9.81f * Time.deltaTime);
        }

        public void RotateTowards(Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0f) return;
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float maxDegrees = RotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxDegrees);
        }

        public void FaceTarget(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position);
            direction.y = 0f;
            
            if (direction.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(direction.normalized);
            }
        }

        public void StopMoving()
        {
            // Apply gravity only
            controller.Move(Vector3.down * 9.81f * Time.deltaTime);
        }

        public Vector3 GetRandomRoamPosition()
        {
            Vector2 randomCircle = Random.insideUnitCircle * RoamDistance;
            Vector3 randomPos = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
            return randomPos;
        }
        #endregion

        #region Detection
        public bool CanSeePlayer()
        {
            if (playerTransform == null) 
            {
                FindPlayer();
                if (playerTransform == null) return false;
            }
            
            Vector3 toPlayer = playerTransform.position - transform.position;
            float distance = toPlayer.magnitude;
            
            // Check distance
            if (distance > DetectionRange) return false;
            
            // Check field of view
            toPlayer.y = 0f;
            float angle = Vector3.Angle(transform.forward, toPlayer.normalized);
            if (angle > FieldOfView / 2f) return false;
            
            return true;
        }

        public Transform GetPlayerTransform()
        {
            if (playerTransform == null)
            {
                FindPlayer();
            }
            return playerTransform;
        }
        #endregion

        // デフォルト攻撃データ（attacksが空の場合に使用）
        private static readonly DefaultMonsterAttack DEFAULT_ATTACK = new DefaultMonsterAttack
        {
            attackName = "Bite",
            damage = 15,
            startup = 0.5f,
            active = 0.2f,
            recovery = 0.8f,
            hitboxOffset = new Vector3(0f, -1.5f, 2f),  // 下方にオフセット
            hitboxSize = new Vector3(5f, 5f, 5f),       // 高さ5mに
            weight = 1f,
            minDistance = 0f,
            maxDistance = 10f
        };
        
        #region Combat
        public MonsterAttackData SelectAttack()
        {
            // attacksが空の場合はnullを返す（デフォルト攻撃はSelectDefaultAttackで処理）
            if (attacks == null || attacks.Length == 0) return null;
            
            float distance = playerTransform != null 
                ? Vector3.Distance(transform.position, playerTransform.position) 
                : 0f;
            
            // Filter valid attacks
            List<MonsterAttackData> validAttacks = new List<MonsterAttackData>();
            float totalWeight = 0f;
            
            foreach (var attack in attacks)
            {
                if (distance >= attack.minDistance && distance <= attack.maxDistance)
                {
                    validAttacks.Add(attack);
                    totalWeight += attack.weight;
                }
            }
            
            if (validAttacks.Count == 0)
            {
                // Fallback to first attack
                return attacks[0];
            }
            
            // Weighted random selection
            float roll = Random.Range(0f, totalWeight);
            float current = 0f;
            
            foreach (var attack in validAttacks)
            {
                current += attack.weight;
                if (roll <= current)
                {
                    return attack;
                }
            }
            
            return validAttacks[0];
        }

        public bool HasAttacks => attacks != null && attacks.Length > 0;
        
        public float DefaultAttackTotalDuration => DEFAULT_ATTACK.startup + DEFAULT_ATTACK.active + DEFAULT_ATTACK.recovery;
        
        public void PerformDefaultAttackHitDetection()
        {
            Vector3 hitboxCenter = transform.position +
                transform.forward * DEFAULT_ATTACK.hitboxOffset.z +
                transform.up * DEFAULT_ATTACK.hitboxOffset.y +
                transform.right * DEFAULT_ATTACK.hitboxOffset.x;
            
            Collider[] hits = Physics.OverlapBox(
                hitboxCenter,
                DEFAULT_ATTACK.hitboxSize / 2f,
                transform.rotation
            );
            
            foreach (var hit in hits)
            {
                if (hit.transform.root == transform.root) continue;
                
                // ルートオブジェクトからも検索
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable == null)
                {
                    damageable = hit.GetComponentInParent<IDamageable>();
                }
                if (damageable == null)
                {
                    damageable = hit.transform.root.GetComponent<IDamageable>();
                }
                
                if (damageable != null)
                {
                    damageable.TakeDamage(DEFAULT_ATTACK.damage, transform.position);
                }
            }
        }
        
        public float GetDefaultAttackStartup() => DEFAULT_ATTACK.startup;
        
        public void PerformAttackHitDetection(MonsterAttackData attack)
        {
            if (attack == null) return;
            
            Vector3 hitboxCenter = transform.position +
                transform.forward * attack.hitboxOffset.z +
                transform.up * attack.hitboxOffset.y +
                transform.right * attack.hitboxOffset.x;
            
            // 全レイヤー検出、IDamageableで絞り込み
            Collider[] hits = Physics.OverlapBox(
                hitboxCenter,
                attack.hitboxSize / 2f,
                transform.rotation
            );
            
            foreach (var hit in hits)
            {
                // 自分自身を除外
                if (hit.transform.root == transform.root) continue;
                
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(attack.damage, transform.position);
                }
            }
        }
        #endregion

        #region IDamageable
        public void TakeDamage(int damage, Vector3 sourcePosition)
        {
            if (isDead) return;
            
            currentHealth -= damage;
            currentFlinchDamage += damage;
            
            OnDamaged?.Invoke(damage);
            
            Debug.Log($"{MonsterName} took {damage} damage! HP: {currentHealth}/{MaxHealth}");
            
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                isDead = true;
                stateMachine.RequestStateChange(MonsterState.Dead);
            }
            else if (currentFlinchDamage >= FlinchThreshold)
            {
                stateMachine.RequestStateChange(MonsterState.Flinch);
                OnFlinched?.Invoke();
            }
        }

        public void ResetFlinch()
        {
            currentFlinchDamage = 0f;
        }

        public void OnDeath()
        {
            isDead = true;
            OnDied?.Invoke();
            Debug.Log($"{MonsterName} has been slain!");
            
            // Add carving point component
            SpawnCarvingPoint();
        }
        
        private void SpawnCarvingPoint()
        {
            // Add CarvingPoint component to self
            var carvingPoint = gameObject.AddComponent<CarvingPoint>();
            carvingPoint.Initialize(3, 1.5f, carveRewards);
            OnCarvingPointSpawned?.Invoke(carvingPoint);
            
            Debug.Log($"Carving point spawned on {MonsterName}");
        }
        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, DetectionRange);
            
            // Attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);
            
            // Attack hitbox
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // オレンジ
            Vector3 hitboxCenter = transform.position +
                transform.forward * DEFAULT_ATTACK.hitboxOffset.z +
                transform.up * DEFAULT_ATTACK.hitboxOffset.y +
                transform.right * DEFAULT_ATTACK.hitboxOffset.x;
            Gizmos.matrix = Matrix4x4.TRS(hitboxCenter, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, DEFAULT_ATTACK.hitboxSize);
            
            // Field of view
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.cyan;
            Vector3 leftBound = Quaternion.Euler(0, -FieldOfView / 2f, 0) * transform.forward;
            Vector3 rightBound = Quaternion.Euler(0, FieldOfView / 2f, 0) * transform.forward;
            Gizmos.DrawLine(transform.position, transform.position + leftBound * DetectionRange);
            Gizmos.DrawLine(transform.position, transform.position + rightBound * DetectionRange);
        }
#endif
    }
    
    /// <summary>
    /// デフォルト攻撃データ（ScriptableObjectなしでテスト可能にする）
    /// </summary>
    public struct DefaultMonsterAttack
    {
        public string attackName;
        public int damage;
        public float startup;
        public float active;
        public float recovery;
        public Vector3 hitboxOffset;
        public Vector3 hitboxSize;
        public float weight;
        public float minDistance;
        public float maxDistance;
    }
}
