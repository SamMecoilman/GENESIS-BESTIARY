using UnityEngine;

namespace GenesisBestiary.Combat
{
    /// <summary>
    /// 剥ぎ取りポイント - モンスターの死体から素材を入手
    /// </summary>
    public class CarvingPoint : MonoBehaviour
    {
        [Header("Carving Settings")]
        [SerializeField] private int maxCarves = 3;
        [SerializeField] private float carveTime = 1.5f;
        [SerializeField] private float interactionRange = 2.5f;
        
        [Header("Loot Table")]
        [SerializeField] private CarveItem[] lootTable;
        
        private int remainingCarves;
        private bool isBeingCarved;
        private float carveProgress;
        
        // Events
        public System.Action<string, int> OnItemObtained;
        public System.Action OnCarveComplete;
        public System.Action OnAllCarvesComplete;
        
        public int RemainingCarves => remainingCarves;
        public float CarveTime => carveTime;
        public float CarveProgress => carveProgress;
        public float InteractionRange => interactionRange;
        public bool CanCarve => remainingCarves > 0 && !isBeingCarved;

        /// <summary>
        /// Initialize carving point at runtime
        /// </summary>
        public void Initialize(int maxCarveCount, float carveSeconds, CarveItem[] loot)
        {
            maxCarves = maxCarveCount;
            carveTime = carveSeconds;
            lootTable = loot;
            remainingCarves = maxCarves;
        }

        private void Awake()
        {
            remainingCarves = maxCarves;
        }

        public bool StartCarving()
        {
            if (!CanCarve) return false;
            
            isBeingCarved = true;
            carveProgress = 0f;
            return true;
        }

        public void UpdateCarving(float deltaTime)
        {
            if (!isBeingCarved) return;
            
            carveProgress += deltaTime;
            
            if (carveProgress >= carveTime)
            {
                CompleteCarve();
            }
        }

        public void CancelCarving()
        {
            isBeingCarved = false;
            carveProgress = 0f;
        }

        private void CompleteCarve()
        {
            isBeingCarved = false;
            carveProgress = 0f;
            remainingCarves--;
            
            // Roll for loot
            CarveItem item = RollLoot();
            if (item != null)
            {
                OnItemObtained?.Invoke(item.itemName, item.quantity);
                Debug.Log($"Obtained: {item.itemName} x{item.quantity}");
            }
            
            OnCarveComplete?.Invoke();
            
            if (remainingCarves <= 0)
            {
                OnAllCarvesComplete?.Invoke();
            }
        }

        private CarveItem RollLoot()
        {
            if (lootTable == null || lootTable.Length == 0) return null;
            
            float totalWeight = 0f;
            foreach (var item in lootTable)
            {
                totalWeight += item.dropRate;
            }
            
            float roll = Random.Range(0f, totalWeight);
            float current = 0f;
            
            foreach (var item in lootTable)
            {
                current += item.dropRate;
                if (roll <= current)
                {
                    return item;
                }
            }
            
            return lootTable[0];
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }

    [System.Serializable]
    public class CarveItem
    {
        public string itemName;
        public int quantity = 1;
        [Range(0f, 100f)]
        public float dropRate = 50f;
    }
}
