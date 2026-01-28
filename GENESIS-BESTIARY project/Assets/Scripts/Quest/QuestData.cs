using UnityEngine;

namespace GenesisBestiary.Quest
{
    [CreateAssetMenu(menuName = "GENESIS/Quest Data", fileName = "QuestData")]
    public class QuestData : ScriptableObject
    {
        [Header("Basic Info")]
        public string questName;
        [TextArea(2, 4)]
        public string description;
        
        [Header("Objective")]
        public QuestObjectiveType objectiveType;
        public string targetMonsterName;
        public int targetCount = 1;
        
        [Header("Limits")]
        public float timeLimit = 3000f; // 50 minutes
        public int maxDeaths = 3;
        
        [Header("Rewards")]
        public int zenny = 1000;
        // Future: public ItemReward[] itemRewards;
    }
    
    public enum QuestObjectiveType
    {
        Hunt,       // Kill monster
        Capture,    // Capture monster (Phase 3)
        Gather,     // Gather items (Phase 3)
        Slay        // Kill multiple small monsters
    }
}
