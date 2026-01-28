using UnityEngine;

namespace GenesisBestiary.Player
{
    [CreateAssetMenu(menuName = "GENESIS/Player Data", fileName = "PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [Header("Health")]
        public int maxHealth = 100;
        
        [Header("Stamina")]
        public float maxStamina = 100f;
        public float staminaRegenRate = 10f;
        public float staminaRegenDelay = 1f;
        
        [Header("Movement")]
        public float moveSpeed = 5f;
        public float rotationSpeed = 720f;
        
        [Header("Dodge")]
        public float dodgeDistance = 3f;
        public float dodgeDuration = 0.4f;
        public float dodgeIFrameDuration = 0.2f;
        public float dodgeStaminaCost = 25f;
        public float dodgeCooldown = 0.1f;

        [Header("Gravity")]
        public float gravity = -9.81f;
        public float groundedGravity = -2f;

        [Header("Input")]
        public float inputDeadzone = 0.1f;
    }
}
