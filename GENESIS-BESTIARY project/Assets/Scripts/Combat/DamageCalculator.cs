using UnityEngine;

namespace GenesisBestiary.Combat
{
    /// <summary>
    /// Calculates damage based on MH-style formula.
    /// Phase 1: Basic calculation (Attack * MotionValue * HitZone)
    /// Phase 2: Add Sharpness, Critical, Element
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>
        /// Calculate raw damage (Phase 1 simplified version)
        /// Formula: (WeaponAttack * MotionValue / 100) * HitZone
        /// </summary>
        public static int CalculateRawDamage(int weaponAttack, int motionValue, float hitZone = 1f)
        {
            float rawDamage = weaponAttack * (motionValue / 100f);
            float finalDamage = rawDamage * hitZone;
            
            // Minimum 1 damage
            return Mathf.Max(1, Mathf.RoundToInt(finalDamage));
        }
        
        /// <summary>
        /// Calculate damage with all modifiers (Phase 2+)
        /// </summary>
        public static int CalculateDamage(
            int weaponAttack,
            int motionValue,
            float hitZone,
            float sharpnessModifier = 1f,
            bool isCritical = false,
            float criticalModifier = 1.25f)
        {
            float rawDamage = weaponAttack * (motionValue / 100f);
            float modifiedDamage = rawDamage * sharpnessModifier;
            
            if (isCritical)
            {
                modifiedDamage *= criticalModifier;
            }
            
            float finalDamage = modifiedDamage * hitZone;
            
            return Mathf.Max(1, Mathf.RoundToInt(finalDamage));
        }
        
        /// <summary>
        /// Roll for critical hit based on affinity
        /// </summary>
        public static bool RollCritical(int affinity)
        {
            if (affinity == 0) return false;
            
            int roll = Random.Range(0, 100);
            
            if (affinity > 0)
            {
                return roll < affinity;
            }
            else
            {
                // Negative affinity = chance for weak hit
                return roll < Mathf.Abs(affinity);
            }
        }
    }
}
