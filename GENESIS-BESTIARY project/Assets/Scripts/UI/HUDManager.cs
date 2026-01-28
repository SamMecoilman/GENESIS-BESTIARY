using GenesisBestiary.Monster;
using GenesisBestiary.Player;
using GenesisBestiary.Quest;
using UnityEngine;
using UnityEngine.UI;

namespace GenesisBestiary.UI
{
    public class HUDManager : MonoBehaviour
    {
        [Header("Player Stats")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider staminaBar;
        [SerializeField] private Text healthText;
        [SerializeField] private Text staminaText;
        
        [Header("Quest Info")]
        [SerializeField] private Text timerText;
        [SerializeField] private Text deathCountText;
        [SerializeField] private GameObject[] deathIcons;
        
        [Header("Monster")]
        [SerializeField] private GameObject monsterHealthPanel;
        [SerializeField] private Slider monsterHealthBar;
        [SerializeField] private Text monsterNameText;
        
        [Header("Damage Numbers")]
        [SerializeField] private GameObject damageNumberPrefab;
        [SerializeField] private Canvas worldCanvas;
        
        [Header("References")]
        [SerializeField] private HunterController hunter;
        [SerializeField] private PlayerController player;
        [SerializeField] private MonsterController monster;
        
        private QuestManager questManager;
        private MonsterController observedMonster;
        private const float DAMAGE_NUMBER_HEIGHT = 2f;

        private void Awake()
        {
            questManager = FindObjectOfType<QuestManager>();
            ResolvePlayer();
            ResolveMonster();
        }

        private void OnEnable()
        {
            SubscribeMonsterEvents();
        }

        private void OnDisable()
        {
            UnsubscribeMonsterEvents();
        }

        private void Update()
        {
            UpdatePlayerStats();
            UpdateQuestInfo();
            UpdateMonsterHealthBar();
        }

        private void UpdatePlayerStats()
        {
            if (hunter != null)
            {
                UpdateHunterStats();
                return;
            }
            
            if (player != null && player.Data != null)
            {
                UpdateLegacyPlayerStats();
            }
        }
        
        private void UpdateHunterStats()
        {
            // Health
            if (healthBar != null)
            {
                healthBar.maxValue = hunter.MaxHealth;
                healthBar.value = hunter.CurrentHealth;
            }
            
            if (healthText != null)
            {
                healthText.text = $"{hunter.CurrentHealth}/{hunter.MaxHealth}";
            }
            
            // Stamina
            if (staminaBar != null)
            {
                staminaBar.maxValue = hunter.MaxStamina;
                staminaBar.value = hunter.CurrentStamina;
            }
            
            if (staminaText != null)
            {
                staminaText.text = $"{Mathf.RoundToInt(hunter.CurrentStamina)}/{Mathf.RoundToInt(hunter.MaxStamina)}";
            }
        }
        
        private void UpdateLegacyPlayerStats()
        {
            if (player == null || player.Data == null) return;
            
            // Health
            if (healthBar != null)
            {
                healthBar.maxValue = player.Data.maxHealth;
                healthBar.value = player.CurrentHealth;
            }
            
            if (healthText != null)
            {
                healthText.text = $"{player.CurrentHealth}/{player.Data.maxHealth}";
            }
            
            // Stamina
            if (staminaBar != null)
            {
                staminaBar.maxValue = player.Data.maxStamina;
                staminaBar.value = player.CurrentStamina;
            }
            
            if (staminaText != null)
            {
                staminaText.text = $"{Mathf.RoundToInt(player.CurrentStamina)}/{Mathf.RoundToInt(player.Data.maxStamina)}";
            }
        }

        private void UpdateQuestInfo()
        {
            if (questManager == null) return;
            
            // Timer
            if (timerText != null)
            {
                float remaining = questManager.RemainingTime;
                int minutes = Mathf.FloorToInt(remaining / 60f);
                int seconds = Mathf.FloorToInt(remaining % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
            
            // Death count
            if (deathCountText != null)
            {
                deathCountText.text = $"x{questManager.RemainingLives}";
            }
            
            // Death icons
            if (deathIcons != null)
            {
                for (int i = 0; i < deathIcons.Length; i++)
                {
                    if (deathIcons[i] != null)
                    {
                        deathIcons[i].SetActive(i < questManager.RemainingLives);
                    }
                }
            }
        }

        private void ResolvePlayer()
        {
            if (hunter == null)
            {
                hunter = FindObjectOfType<HunterController>();
            }
            
            if (hunter == null && player == null)
            {
                player = FindObjectOfType<PlayerController>();
            }
        }
        
        private void ResolveMonster()
        {
            if (monster == null)
            {
                monster = FindObjectOfType<MonsterController>();
            }
        }

        public void ShowMonsterHealth(string monsterName, int currentHealth, int maxHealth)
        {
            if (monsterHealthPanel != null)
            {
                monsterHealthPanel.SetActive(true);
            }
            
            if (monsterNameText != null)
            {
                monsterNameText.text = monsterName;
            }
            
            if (monsterHealthBar != null)
            {
                monsterHealthBar.maxValue = maxHealth;
                monsterHealthBar.value = currentHealth;
            }
        }

        private void UpdateMonsterHealthBar()
        {
            if (observedMonster == null) return;
            
            ShowMonsterHealth(
                observedMonster.MonsterName,
                observedMonster.CurrentHealth,
                observedMonster.MaxHealth
            );
        }
        
        public void HideMonsterHealth()
        {
            if (monsterHealthPanel != null)
            {
                monsterHealthPanel.SetActive(false);
            }
        }

        public void SpawnDamageNumber(Vector3 worldPosition, int damage, bool isCritical = false)
        {
            if (damageNumberPrefab == null || worldCanvas == null) return;
            
            GameObject dmgObj = Instantiate(damageNumberPrefab, worldCanvas.transform);
            
            // Convert world position to screen position
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            dmgObj.transform.position = screenPos;
            
            // Set damage text
            Text dmgText = dmgObj.GetComponent<Text>();
            if (dmgText != null)
            {
                dmgText.text = damage.ToString();
                dmgText.color = isCritical ? Color.yellow : Color.white;
            }
            
            // Auto-destroy
            Destroy(dmgObj, 1.5f);
        }
        
        private void SubscribeMonsterEvents()
        {
            ResolveMonster();
            if (monster == null) return;
            
            if (observedMonster == monster) return;
            
            UnsubscribeMonsterEvents();
            observedMonster = monster;
            observedMonster.OnDamaged += OnMonsterDamaged;
            observedMonster.OnDied += OnMonsterDied;
            
            ShowMonsterHealth(observedMonster.MonsterName, observedMonster.CurrentHealth, observedMonster.MaxHealth);
        }
        
        private void UnsubscribeMonsterEvents()
        {
            if (observedMonster == null) return;
            
            observedMonster.OnDamaged -= OnMonsterDamaged;
            observedMonster.OnDied -= OnMonsterDied;
            observedMonster = null;
        }
        
        private void OnMonsterDamaged(int damage)
        {
            UpdateMonsterHealthBar();
            if (observedMonster != null)
            {
                Vector3 spawnPos = observedMonster.transform.position + Vector3.up * DAMAGE_NUMBER_HEIGHT;
                SpawnDamageNumber(spawnPos, damage);
            }
        }
        
        private void OnMonsterDied()
        {
            HideMonsterHealth();
            UnsubscribeMonsterEvents();
        }
    }
}
