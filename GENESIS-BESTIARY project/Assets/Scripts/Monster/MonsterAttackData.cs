using UnityEngine;

namespace GenesisBestiary.Monster
{
    [CreateAssetMenu(menuName = "GENESIS/Monster Attack Data", fileName = "MonsterAttackData")]
    public class MonsterAttackData : ScriptableObject
    {
        [Header("Basic Info")]
        public string attackName;
        
        [Header("Damage")]
        public int damage = 20;
        
        [Header("Timing")]
        [Tooltip("Time before hitbox becomes active")]
        public float startup = 0.5f;
        
        [Tooltip("Duration of active hitbox")]
        public float active = 0.2f;
        
        [Tooltip("Recovery time after attack")]
        public float recovery = 1.0f;
        
        [Header("Hitbox")]
        public Vector3 hitboxOffset = new Vector3(0f, 1f, 2f);
        public Vector3 hitboxSize = new Vector3(2f, 2f, 3f);
        
        [Header("Selection Weight")]
        [Tooltip("Higher = more likely to be selected")]
        public float weight = 1f;
        
        [Header("Requirements")]
        [Tooltip("Minimum distance to player for this attack")]
        public float minDistance = 0f;
        
        [Tooltip("Maximum distance to player for this attack")]
        public float maxDistance = 5f;
        
        public float TotalDuration => startup + active + recovery;
    }
}
