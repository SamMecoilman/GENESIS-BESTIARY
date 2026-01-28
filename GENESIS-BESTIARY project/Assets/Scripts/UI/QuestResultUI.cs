using GenesisBestiary.Combat;
using GenesisBestiary.Monster;
using GenesisBestiary.Quest;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace GenesisBestiary.UI
{
    public class QuestResultUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private GameObject successPanel;
        [SerializeField] private GameObject failedPanel;
        
        [Header("Success Elements")]
        [SerializeField] private Text questCompleteText;
        [SerializeField] private Text clearTimeText;
        [SerializeField] private Text zennyRewardText;
        [SerializeField] private Transform rewardsContainer;
        [SerializeField] private GameObject rewardItemPrefab;
        
        [Header("Failed Elements")]
        [SerializeField] private Text questFailedText;
        [SerializeField] private Text failReasonText;
        
        [Header("Buttons")]
        [SerializeField] private Button returnToVillageButton;
        [SerializeField] private Button retryButton;
        
        private QuestManager questManager;
        private MonsterController monster;
        private CarvingPoint observedCarvingPoint;
        private List<RewardItemData> obtainedRewards = new List<RewardItemData>();

        private void Awake()
        {
            questManager = FindObjectOfType<QuestManager>();
            monster = FindObjectOfType<MonsterController>();
            
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }
            
            // Setup button listeners
            if (returnToVillageButton != null)
            {
                returnToVillageButton.onClick.AddListener(OnReturnToVillage);
            }
            
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnRetry);
            }
        }

        private void Start()
        {
            if (questManager != null)
            {
                questManager.OnQuestStart += ResetResults;
                questManager.OnQuestSuccess += ShowSuccessResult;
                questManager.OnQuestFailed += ShowFailedResult;
            }
            
            SubscribeMonsterEvents();
            SubscribeExistingCarvingPoint();
        }

        private void OnDestroy()
        {
            if (questManager != null)
            {
                questManager.OnQuestStart -= ResetResults;
                questManager.OnQuestSuccess -= ShowSuccessResult;
                questManager.OnQuestFailed -= ShowFailedResult;
            }
            
            UnsubscribeMonsterEvents();
            UnsubscribeCarvingPoint();
        }

        public void AddReward(string itemName, int quantity)
        {
            // Check if item already exists
            var existing = obtainedRewards.Find(r => r.itemName == itemName);
            if (existing != null)
            {
                existing.quantity += quantity;
            }
            else
            {
                obtainedRewards.Add(new RewardItemData { itemName = itemName, quantity = quantity });
            }
        }

        private void ShowSuccessResult()
        {
            if (resultPanel != null) resultPanel.SetActive(true);
            if (successPanel != null) successPanel.SetActive(true);
            if (failedPanel != null) failedPanel.SetActive(false);
            
            // Set quest complete text
            if (questCompleteText != null)
            {
                questCompleteText.text = "QUEST COMPLETE!";
            }
            
            // Set clear time
            if (clearTimeText != null && questManager != null)
            {
                clearTimeText.text = $"Clear Time: {questManager.FormatTime(questManager.ElapsedTime)}";
            }
            
            // Set zenny reward
            if (zennyRewardText != null && questManager?.CurrentQuest != null)
            {
                zennyRewardText.text = $"Zenny: {questManager.CurrentQuest.zenny}z";
            }
            
            // Populate rewards
            PopulateRewards();
            
            // Pause game
            Time.timeScale = 0f;
        }

        private void ShowFailedResult()
        {
            if (resultPanel != null) resultPanel.SetActive(true);
            if (successPanel != null) successPanel.SetActive(false);
            if (failedPanel != null) failedPanel.SetActive(true);
            
            if (questFailedText != null)
            {
                questFailedText.text = "QUEST FAILED";
            }
            
            if (failReasonText != null && questManager != null)
            {
                if (questManager.RemainingTime <= 0)
                {
                    failReasonText.text = "Time's up!";
                }
                else if (questManager.RemainingLives <= 0)
                {
                    failReasonText.text = "Too many faints!";
                }
                else
                {
                    failReasonText.text = "";
                }
            }
            
            // Pause game
            Time.timeScale = 0f;
        }

        private void ResetResults()
        {
            obtainedRewards.Clear();
            Hide();
        }

        private void PopulateRewards()
        {
            if (rewardsContainer == null || rewardItemPrefab == null) return;
            
            // Clear existing items
            foreach (Transform child in rewardsContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Add reward items
            foreach (var reward in obtainedRewards)
            {
                GameObject item = Instantiate(rewardItemPrefab, rewardsContainer);
                Text itemText = item.GetComponentInChildren<Text>();
                if (itemText != null)
                {
                    itemText.text = $"{reward.itemName} x{reward.quantity}";
                }
            }
        }

        private void OnReturnToVillage()
        {
            Time.timeScale = 1f;
            // Load village scene
            // UnityEngine.SceneManagement.SceneManager.LoadScene("Village");
            Debug.Log("Returning to village...");
        }

        private void OnRetry()
        {
            Time.timeScale = 1f;
            // Reload current scene
            // UnityEngine.SceneManagement.SceneManager.LoadScene(
            //     UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            Debug.Log("Retrying quest...");
        }

        public void Hide()
        {
            if (resultPanel != null) resultPanel.SetActive(false);
            Time.timeScale = 1f;
        }

        private void SubscribeMonsterEvents()
        {
            if (monster == null)
            {
                monster = FindObjectOfType<MonsterController>();
            }
            
            if (monster != null)
            {
                monster.OnCarvingPointSpawned += OnCarvingPointSpawned;
            }
        }
        
        private void UnsubscribeMonsterEvents()
        {
            if (monster != null)
            {
                monster.OnCarvingPointSpawned -= OnCarvingPointSpawned;
            }
        }
        
        private void SubscribeExistingCarvingPoint()
        {
            var carvingPoint = FindObjectOfType<CarvingPoint>();
            if (carvingPoint != null)
            {
                AttachCarvingPoint(carvingPoint);
            }
        }
        
        private void OnCarvingPointSpawned(CarvingPoint carvingPoint)
        {
            AttachCarvingPoint(carvingPoint);
        }
        
        private void AttachCarvingPoint(CarvingPoint carvingPoint)
        {
            if (carvingPoint == null) return;
            
            if (observedCarvingPoint == carvingPoint) return;
            
            UnsubscribeCarvingPoint();
            observedCarvingPoint = carvingPoint;
            observedCarvingPoint.OnItemObtained += AddReward;
            observedCarvingPoint.OnAllCarvesComplete += OnAllCarvesComplete;
        }
        
        private void UnsubscribeCarvingPoint()
        {
            if (observedCarvingPoint == null) return;
            
            observedCarvingPoint.OnItemObtained -= AddReward;
            observedCarvingPoint.OnAllCarvesComplete -= OnAllCarvesComplete;
            observedCarvingPoint = null;
        }
        
        private void OnAllCarvesComplete()
        {
            UnsubscribeCarvingPoint();
        }

        [System.Serializable]
        private class RewardItemData
        {
            public string itemName;
            public int quantity;
        }

#if UNITY_EDITOR
        [ContextMenu("Debug: Show Success")]
        private void DebugShowSuccess()
        {
            obtainedRewards.Clear();
            obtainedRewards.Add(new RewardItemData { itemName = "Monster Scale", quantity = 3 });
            obtainedRewards.Add(new RewardItemData { itemName = "Monster Claw", quantity = 2 });
            obtainedRewards.Add(new RewardItemData { itemName = "Monster Fang", quantity = 1 });
            ShowSuccessResult();
        }
        
        [ContextMenu("Debug: Show Failed")]
        private void DebugShowFailed()
        {
            ShowFailedResult();
        }
#endif
    }
}
