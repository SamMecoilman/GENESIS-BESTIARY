using UnityEngine;

namespace GenesisBestiary.Combat
{
    [CreateAssetMenu(menuName = "GENESIS/Attack Data", fileName = "AttackData")]
    public class AttackData : ScriptableObject
    {
        [Header("Basic Info")]
        public string attackName;
        
        [Header("Damage")]
        [Tooltip("Motion value for damage calculation (higher = more damage)")]
        public int motionValue = 10;
        
        [Header("Timing")]
        [Tooltip("Time before hitbox becomes active")]
        public float startup = 0.3f;
        
        [Tooltip("Duration of active hitbox")]
        public float active = 0.1f;
        
        [Tooltip("Recovery time after attack")]
        public float recovery = 0.5f;
        
        [Header("Hitbox")]
        public Vector3 hitboxOffset = new Vector3(0f, 1f, 1.5f);
        public Vector3 hitboxSize = new Vector3(1f, 1f, 2f);
        
        [Header("Movement")]
        [Tooltip("Forward movement during attack")]
        public float forwardMovement = 0f;
        
        [Header("Combo")]
        [Tooltip("Can chain into next attack during recovery")]
        public bool canCombo = true;
        
        [Tooltip("Time window to input next combo attack")]
        public float comboWindow = 0.3f;
        
        /// <summary>
        /// Total duration of the attack
        /// </summary>
        public float TotalDuration => startup + active + recovery;
    }
}
