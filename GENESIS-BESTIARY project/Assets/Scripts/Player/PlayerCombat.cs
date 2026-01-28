using GenesisBestiary.Combat;
using UnityEngine;

namespace GenesisBestiary.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private WeaponData currentWeapon;
        
        private int currentComboIndex = 0;
        private float attackTimer = 0f;
        private float comboTimer = 0f;
        private bool attackInputBuffered = false;
        private AttackPhase currentPhase = AttackPhase.None;
        
        // デフォルト攻撃データ（WeaponDataがない場合に使用）
        private static readonly DefaultAttackData[] DEFAULT_COMBO = new[]
        {
            new DefaultAttackData("Vertical Slash", 48, 0.4f, 0.1f, 0.5f, 1f, true, 0.5f),
            new DefaultAttackData("Horizontal Slash", 26, 0.3f, 0.1f, 0.4f, 0.5f, true, 0.4f),
            new DefaultAttackData("Rising Slash", 38, 0.4f, 0.1f, 0.6f, 0.3f, false, 0f)
        };
        private const int DEFAULT_WEAPON_ATTACK = 100;
        
        public enum AttackPhase
        {
            None,
            Startup,
            Active,
            Recovery
        }
        
        // Events for state machine integration
        public System.Action OnAttackStart;
        public System.Action OnAttackHit;
        public System.Action OnAttackEnd;
        public System.Action OnComboAdvance;
        
        public WeaponData CurrentWeapon => currentWeapon;
        public AttackData CurrentAttack => GetCurrentAttackData();
        public AttackPhase CurrentPhase => currentPhase;
        public bool IsAttacking => currentPhase != AttackPhase.None;
        public int ComboIndex => currentComboIndex;
        public float CurrentForwardMovement => GetForwardMovement();
        
        private int WeaponAttack => currentWeapon != null ? currentWeapon.attack : DEFAULT_WEAPON_ATTACK;
        private int ComboLength => currentWeapon?.comboAttacks != null ? currentWeapon.comboAttacks.Length : DEFAULT_COMBO.Length;
        
        private AttackData GetCurrentAttackData()
        {
            if (currentWeapon?.comboAttacks != null && 
                currentComboIndex < currentWeapon.comboAttacks.Length)
            {
                return currentWeapon.comboAttacks[currentComboIndex];
            }
            return null;
        }
        
        private DefaultAttackData GetDefaultAttack()
        {
            if (currentComboIndex < DEFAULT_COMBO.Length)
                return DEFAULT_COMBO[currentComboIndex];
            return DEFAULT_COMBO[0];
        }
        
        // アタックデータ取得（AttackDataかDefaultAttackDataのいずれか）
        private float GetStartup() => CurrentAttack?.startup ?? GetDefaultAttack().startup;
        private float GetActive() => CurrentAttack?.active ?? GetDefaultAttack().active;
        private float GetRecovery() => CurrentAttack?.recovery ?? GetDefaultAttack().recovery;
        private float GetComboWindow() => CurrentAttack?.comboWindow ?? GetDefaultAttack().comboWindow;
        private bool GetCanCombo() => CurrentAttack?.canCombo ?? GetDefaultAttack().canCombo;
        private int GetMotionValue() => CurrentAttack?.motionValue ?? GetDefaultAttack().motionValue;
        private float GetForwardMovement() => CurrentAttack?.forwardMovement ?? GetDefaultAttack().forwardMovement;
        private Vector3 GetHitboxOffset() => CurrentAttack?.hitboxOffset ?? new Vector3(0f, 1f, 1.5f);
        private Vector3 GetHitboxSize() => CurrentAttack?.hitboxSize ?? new Vector3(1f, 1f, 2f);
        
        private void Update()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                TryBufferAttack();
            }
        }
        
        public void TryBufferAttack()
        {
            attackInputBuffered = true;
        }
        
        public bool TryStartAttack()
        {
            // WeaponDataがなくてもデフォルトコンボで攻撃可能
            currentComboIndex = 0;
            StartAttack();
            return true;
        }
        
        public void StartAttack()
        {
            attackTimer = 0f;
            currentPhase = AttackPhase.Startup;
            attackInputBuffered = false;
            
            OnAttackStart?.Invoke();
        }
        
        public void UpdateAttack()
        {
            if (currentPhase == AttackPhase.None) return;
            
            attackTimer += Time.deltaTime;
            
            // Phase transitions
            switch (currentPhase)
            {
                case AttackPhase.Startup:
                    if (attackTimer >= GetStartup())
                    {
                        currentPhase = AttackPhase.Active;
                        attackTimer = 0f;
                        PerformHitDetection();
                    }
                    break;
                    
                case AttackPhase.Active:
                    if (attackTimer >= GetActive())
                    {
                        currentPhase = AttackPhase.Recovery;
                        attackTimer = 0f;
                    }
                    break;
                    
                case AttackPhase.Recovery:
                    // Check for combo input during recovery window
                    if (GetCanCombo() && attackTimer <= GetComboWindow() && attackInputBuffered)
                    {
                        if (TryAdvanceCombo())
                        {
                            return;
                        }
                    }
                    
                    if (attackTimer >= GetRecovery())
                    {
                        EndAttack();
                    }
                    break;
            }
        }
        
        private bool TryAdvanceCombo()
        {
            int nextIndex = currentComboIndex + 1;
            
            if (ComboLength > nextIndex)
            {
                currentComboIndex = nextIndex;
                StartAttack();
                OnComboAdvance?.Invoke();
                return true;
            }
            
            return false;
        }
        
        private void PerformHitDetection()
        {
            Vector3 hitboxOffset = GetHitboxOffset();
            Vector3 hitboxSize = GetHitboxSize();
            
            // Calculate hitbox position in world space
            Vector3 hitboxCenter = transform.position + 
                transform.forward * hitboxOffset.z +
                transform.up * hitboxOffset.y +
                transform.right * hitboxOffset.x;
            
            // Perform overlap box (全レイヤー検出、IDamageableで絞り込み)
            Collider[] hits = Physics.OverlapBox(
                hitboxCenter,
                hitboxSize / 2f,
                transform.rotation
            );
            
            foreach (var hit in hits)
            {
                // 自分自身を除外
                if (hit.transform.root == transform.root) continue;
                
                // Try to get damageable component
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    int damage = DamageCalculator.CalculateRawDamage(
                        WeaponAttack,
                        GetMotionValue()
                    );
                    
                    damageable.TakeDamage(damage, transform.position);
                    OnAttackHit?.Invoke();
                }
            }
        }
        
        public void EndAttack()
        {
            currentPhase = AttackPhase.None;
            attackTimer = 0f;
            currentComboIndex = 0;
            attackInputBuffered = false;
            
            OnAttackEnd?.Invoke();
        }
        
        public void ResetCombo()
        {
            currentComboIndex = 0;
            comboTimer = 0f;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 hitboxOffset = GetHitboxOffset();
            Vector3 hitboxSize = GetHitboxSize();
            
            // Draw hitbox
            Gizmos.color = currentPhase == AttackPhase.Active ? Color.red : Color.yellow;
            Vector3 hitboxCenter = transform.position + 
                transform.forward * hitboxOffset.z +
                transform.up * hitboxOffset.y +
                transform.right * hitboxOffset.x;
            
            Gizmos.matrix = Matrix4x4.TRS(hitboxCenter, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, hitboxSize);
        }
#endif
    }
    
    /// <summary>
    /// デフォルト攻撃データ（ScriptableObjectなしでテスト可能にする）
    /// </summary>
    public struct DefaultAttackData
    {
        public string attackName;
        public int motionValue;
        public float startup;
        public float active;
        public float recovery;
        public float forwardMovement;
        public bool canCombo;
        public float comboWindow;
        
        public DefaultAttackData(string name, int mv, float su, float ac, float rec, float fwd, bool combo, float cw)
        {
            attackName = name;
            motionValue = mv;
            startup = su;
            active = ac;
            recovery = rec;
            forwardMovement = fwd;
            canCombo = combo;
            comboWindow = cw;
        }
    }
    
    public interface IDamageable
    {
        void TakeDamage(int damage, Vector3 sourcePosition);
    }
}
