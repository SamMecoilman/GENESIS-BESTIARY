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
        public AttackData CurrentAttack => currentWeapon?.comboAttacks != null && 
            currentComboIndex < currentWeapon.comboAttacks.Length 
                ? currentWeapon.comboAttacks[currentComboIndex] 
                : null;
        public AttackPhase CurrentPhase => currentPhase;
        public bool IsAttacking => currentPhase != AttackPhase.None;
        public int ComboIndex => currentComboIndex;
        
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
            if (currentWeapon == null || currentWeapon.comboAttacks == null || 
                currentWeapon.comboAttacks.Length == 0)
            {
                return false;
            }
            
            // Reset combo if starting fresh
            currentComboIndex = 0;
            StartAttack();
            return true;
        }
        
        public void StartAttack()
        {
            var attack = CurrentAttack;
            if (attack == null) return;
            
            attackTimer = 0f;
            currentPhase = AttackPhase.Startup;
            attackInputBuffered = false;
            
            OnAttackStart?.Invoke();
        }
        
        public void UpdateAttack()
        {
            var attack = CurrentAttack;
            if (attack == null || currentPhase == AttackPhase.None) return;
            
            attackTimer += Time.deltaTime;
            
            // Phase transitions
            switch (currentPhase)
            {
                case AttackPhase.Startup:
                    if (attackTimer >= attack.startup)
                    {
                        currentPhase = AttackPhase.Active;
                        attackTimer = 0f;
                        PerformHitDetection();
                    }
                    break;
                    
                case AttackPhase.Active:
                    if (attackTimer >= attack.active)
                    {
                        currentPhase = AttackPhase.Recovery;
                        attackTimer = 0f;
                    }
                    break;
                    
                case AttackPhase.Recovery:
                    // Check for combo input during recovery window
                    if (attack.canCombo && attackTimer <= attack.comboWindow && attackInputBuffered)
                    {
                        if (TryAdvanceCombo())
                        {
                            return;
                        }
                    }
                    
                    if (attackTimer >= attack.recovery)
                    {
                        EndAttack();
                    }
                    break;
            }
        }
        
        private bool TryAdvanceCombo()
        {
            int nextIndex = currentComboIndex + 1;
            
            if (currentWeapon.comboAttacks.Length > nextIndex)
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
            var attack = CurrentAttack;
            if (attack == null) return;
            
            // Calculate hitbox position in world space
            Vector3 hitboxCenter = transform.position + 
                transform.forward * attack.hitboxOffset.z +
                transform.up * attack.hitboxOffset.y +
                transform.right * attack.hitboxOffset.x;
            
            // Perform overlap box
            Collider[] hits = Physics.OverlapBox(
                hitboxCenter,
                attack.hitboxSize / 2f,
                transform.rotation,
                LayerMask.GetMask("Monster")
            );
            
            foreach (var hit in hits)
            {
                // Try to get damageable component
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    int damage = DamageCalculator.CalculateRawDamage(
                        currentWeapon.attack,
                        attack.motionValue
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
            var attack = CurrentAttack;
            if (attack == null) return;
            
            // Draw hitbox
            Gizmos.color = currentPhase == AttackPhase.Active ? Color.red : Color.yellow;
            Vector3 hitboxCenter = transform.position + 
                transform.forward * attack.hitboxOffset.z +
                transform.up * attack.hitboxOffset.y +
                transform.right * attack.hitboxOffset.x;
            
            Gizmos.matrix = Matrix4x4.TRS(hitboxCenter, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, attack.hitboxSize);
        }
#endif
    }
    
    public interface IDamageable
    {
        void TakeDamage(int damage, Vector3 sourcePosition);
    }
}
