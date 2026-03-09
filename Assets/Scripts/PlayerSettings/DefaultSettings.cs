using UnityEngine;

namespace PlayerSettings
{
    [CreateAssetMenu(fileName = "DefaultSettings", menuName = "Settings/DefaultSettings")]
    public class DefaultSettings : ScriptableObject
    {
        // Основные параметры
        public float defaultSpeed = 2.8f;
        public float defaultAttack = 3f;
        public float defaultProtection = 0f;
        public float defaultAgility = 1f;
        public float defaultMaxHealth = 10f;
        public float defaultCurrentHealth = 10f;
        public float defaultCriticalChance = 5f;

        // Количество апгрейдов
        public int defaultSpeedUpgradeCount = 0;
        public int defaultAttackUpgradeCount = 0;
        public int defaultProtectionUpgradeCount = 0;
        public int defaultAgilityUpgradeCount = 0;
        public int defaultMaxHealthUpgradeCount = 0;
    }
}