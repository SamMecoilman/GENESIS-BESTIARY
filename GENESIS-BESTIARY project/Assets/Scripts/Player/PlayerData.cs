using UnityEngine;

namespace GenesisBestiary.Player
{
    [CreateAssetMenu(menuName = "GENESIS/Player Data", fileName = "PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [Header("Movement")]
        public float moveSpeed = 5f;
        public float rotationSpeed = 720f;

        [Header("Gravity")]
        public float gravity = -9.81f;
        public float groundedGravity = -2f;

        [Header("Input")]
        public float inputDeadzone = 0.1f;
    }
}
