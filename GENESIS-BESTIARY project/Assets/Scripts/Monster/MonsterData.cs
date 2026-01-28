using UnityEngine;

namespace GenesisBestiary.Monster
{
    [CreateAssetMenu(menuName = "GENESIS/Monster Data", fileName = "MonsterData")]
    public class MonsterData : ScriptableObject
    {
        [Header("Basic Info")]
        public string monsterName;
        
        [Header("Stats")]
        public int maxHealth = 1000;
        public float moveSpeed = 3f;
        public float chaseSpeed = 5f;
        public float rotationSpeed = 120f;
        
        [Header("Detection")]
        public float detectionRange = 20f;
        public float attackRange = 4f;
        public float fieldOfView = 120f;
        
        [Header("Combat")]
        public float flinchThreshold = 100f;
        
        [Header("Behavior Timing")]
        public float idleTime = 3f;
        public float roamDistance = 10f;
        
        [Header("Rewards")]
        public int carveCount = 3;
    }
}
