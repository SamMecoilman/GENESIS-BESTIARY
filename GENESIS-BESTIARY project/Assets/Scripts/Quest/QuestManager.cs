using GenesisBestiary.Monster;
using GenesisBestiary.Player;
using UnityEngine;
using System.Collections.Generic;

namespace GenesisBestiary.Quest
{
    public class QuestManager : MonoBehaviour
    {
        [SerializeField] private QuestData currentQuest;
        [SerializeField] private bool autoStartOnSceneLoad = true;
        
        private float elapsedTime;
        private int deathCount;
        private int killCount;
        private QuestState state;
        private List<MonsterController> trackedMonsters = new List<MonsterController>();
        private HunterController trackedHunter;
        private PlayerController trackedPlayer;
        
        public enum QuestState
        {
            NotStarted,
            InProgress,
            Success,
            Failed
        }
        
        // Events
        public System.Action OnQuestStart;
        public System.Action OnQuestSuccess;
        public System.Action OnQuestFailed;
        public System.Action<int> OnPlayerDeath;
        public System.Action<int> OnMonsterKilled;
        
        #region Properties
        public QuestData CurrentQuest => currentQuest;
        public QuestState State => state;
        public float ElapsedTime => elapsedTime;
        public float RemainingTime => currentQuest != null ? currentQuest.timeLimit - elapsedTime : 0f;
        public int DeathCount => deathCount;
        public int RemainingLives => currentQuest != null ? currentQuest.maxDeaths - deathCount : 0;
        public int KillCount => killCount;
        public bool IsComplete => state == QuestState.Success || state == QuestState.Failed;
        #endregion

        private void Awake()
        {
            state = QuestState.NotStarted;
        }
        
        private void Start()
        {
            if (autoStartOnSceneLoad && currentQuest != null)
            {
                StartQuest(currentQuest);
            }
        }

        private void OnDestroy()
        {
            UnsubscribePlayerDeath();
        }

        private void Update()
        {
            if (state != QuestState.InProgress) return;
            
            elapsedTime += Time.deltaTime;
            
            // Check time limit
            if (elapsedTime >= currentQuest.timeLimit)
            {
                FailQuest("Time's up!");
            }
        }

        public void StartQuest(QuestData quest = null)
        {
            if (quest != null)
            {
                currentQuest = quest;
            }
            
            if (currentQuest == null)
            {
                Debug.LogError("No quest data assigned!");
                return;
            }
            
            elapsedTime = 0f;
            deathCount = 0;
            killCount = 0;
            state = QuestState.InProgress;
            
            // Find and track monsters
            TrackMonsters();
            
            // Subscribe to player death
            SubscribePlayerDeath();
            
            OnQuestStart?.Invoke();
            
            Debug.Log($"Quest started: {currentQuest.questName}");
        }

        private void TrackMonsters()
        {
            trackedMonsters.Clear();
            
            var monsters = FindObjectsOfType<MonsterController>();
            foreach (var monster in monsters)
            {
                // Check if this monster matches the quest target
                if (IsQuestTarget(monster))
                {
                    trackedMonsters.Add(monster);
                    monster.OnDied += () => OnMonsterDied(monster);
                }
            }
        }

        private bool IsQuestTarget(MonsterController monster)
        {
            if (monster == null || currentQuest == null) return false;
            
            if (string.IsNullOrWhiteSpace(currentQuest.targetMonsterName))
            {
                return true;
            }
            
            string monsterName = monster.Data != null ? monster.Data.monsterName : monster.MonsterName;
            return monsterName == currentQuest.targetMonsterName;
        }

        private void OnMonsterDied(MonsterController monster)
        {
            if (state != QuestState.InProgress) return;
            
            killCount++;
            OnMonsterKilled?.Invoke(killCount);
            
            // Check objective
            if (currentQuest.objectiveType == QuestObjectiveType.Hunt)
            {
                if (killCount >= currentQuest.targetCount)
                {
                    CompleteQuest();
                }
            }
        }

        private void SubscribePlayerDeath()
        {
            UnsubscribePlayerDeath();
            
            trackedHunter = FindObjectOfType<HunterController>();
            if (trackedHunter != null)
            {
                trackedHunter.OnDied += ReportPlayerDeath;
                return;
            }
            
            trackedPlayer = FindObjectOfType<PlayerController>();
            if (trackedPlayer != null)
            {
                trackedPlayer.OnDied += ReportPlayerDeath;
            }
        }
        
        private void UnsubscribePlayerDeath()
        {
            if (trackedHunter != null)
            {
                trackedHunter.OnDied -= ReportPlayerDeath;
            }
            
            if (trackedPlayer != null)
            {
                trackedPlayer.OnDied -= ReportPlayerDeath;
            }
            
            trackedHunter = null;
            trackedPlayer = null;
        }

        public void ReportPlayerDeath()
        {
            if (state != QuestState.InProgress) return;
            
            deathCount++;
            OnPlayerDeath?.Invoke(deathCount);
            
            if (deathCount >= currentQuest.maxDeaths)
            {
                FailQuest("Too many faints!");
            }
            else
            {
                // Respawn player (to be implemented)
                Debug.Log($"Player fainted! Lives remaining: {RemainingLives}");
            }
        }

        private void CompleteQuest()
        {
            state = QuestState.Success;
            OnQuestSuccess?.Invoke();
            
            Debug.Log($"Quest Complete! Time: {FormatTime(elapsedTime)}");
        }

        private void FailQuest(string reason)
        {
            state = QuestState.Failed;
            OnQuestFailed?.Invoke();
            
            Debug.Log($"Quest Failed: {reason}");
        }

        public string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            int milliseconds = Mathf.FloorToInt((time % 1f) * 100f);
            return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
        }

#if UNITY_EDITOR
        [ContextMenu("Debug: Start Quest")]
        private void DebugStartQuest()
        {
            StartQuest();
        }
        
        [ContextMenu("Debug: Kill Monster")]
        private void DebugKillMonster()
        {
            killCount++;
            OnMonsterKilled?.Invoke(killCount);
            if (killCount >= (currentQuest?.targetCount ?? 1))
            {
                CompleteQuest();
            }
        }
#endif
    }
}
