using UnityEngine;

namespace GenesisBestiary.Combat
{
    public enum WeaponType
    {
        GreatSword
        // Future: Hammer, LongSword, etc.
    }
    
    [CreateAssetMenu(menuName = "GENESIS/Weapon Data", fileName = "WeaponData")]
    public class WeaponData : ScriptableObject
    {
        [Header("Basic Info")]
        public string weaponName;
        public WeaponType weaponType;
        
        [Header("Stats")]
        [Tooltip("Base attack power")]
        public int attack = 100;
        
        [Tooltip("Critical hit chance (-100 to 100)")]
        [Range(-100, 100)]
        public int affinity = 0;
        
        [Header("Combo Attacks")]
        [Tooltip("Attack sequence for basic combo")]
        public AttackData[] comboAttacks;
        
        [Header("Movement Modifier")]
        [Tooltip("Movement speed multiplier when weapon is drawn")]
        public float drawnMoveSpeedMultiplier = 0.7f;
    }
}
