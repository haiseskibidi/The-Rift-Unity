using UnityEngine;
using System;
using System.Collections.Generic;

public class StatsUpgrade : MonoBehaviour
{
    [SerializeField]
    private StatisticsUI statisticsUI; 

    public event Action<string> OnStatUpgraded;

    private Dictionary<string, StatInfo> statInfos;

    private Player player;

    private void Awake()
    {
        player = FindObjectOfType<Player>();

        statInfos = new Dictionary<string, StatInfo>
        {
            { "Player Speed", new StatInfo("Player Speed", 2.8f, 0.04f) },
            { "Player Attack", new StatInfo("Player Attack", 3f, 0.06f) },
            { "Player Protection", new StatInfo("Player Protection", 0f, 0.03f) },
            { "Player Agility", new StatInfo("Player Agility", 1f, 0.6f) },
            { "Player Max Health", new StatInfo("Player Max Health", 10f, 0.3f) },
            { "Critical Chance", new StatInfo("Critical Chance", 5f, 2f) },
        };
    }

    public void ProductUpgrade(string statName)
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError("LevelManager не найден.");
            return;
        }

        if (LevelManager.Instance.GetAvailableUpgradePoints() <= 0)
        {
            Debug.LogWarning("Недостаточно очков для прокачки.");
            return;
        }

        if (statInfos.TryGetValue(statName, out StatInfo statInfo))
        {
            bool spent = LevelManager.Instance.SpendUpgradePoint();
            if (!spent) return;

            float currentValue = PlayerPrefs.GetFloat(statName, statInfo.DefaultValue);
            float newValue = currentValue + statInfo.Increment;

            PlayerPrefs.SetFloat(statName, newValue);

            int currentUpgradeCount = PlayerPrefs.GetInt($"{statName}_UpgradeCount", 0);
            currentUpgradeCount++;
            PlayerPrefs.SetInt($"{statName}_UpgradeCount", currentUpgradeCount);

            if (statName == "Player Agility")
            {
                float currentCrit = PlayerPrefs.GetFloat("Critical Chance", statInfos["Critical Chance"].DefaultValue);
                float newCrit = currentCrit + statInfos["Critical Chance"].Increment;
                PlayerPrefs.SetFloat("Critical Chance", newCrit);

                Debug.Log($"Critical Chance увеличен до {newCrit}%");
            }

            if (statName == "Player Max Health")
            {
                player.IncreaseHealth(statInfo.Increment);
                OnStatUpgraded?.Invoke(statName);
            }
            else
            {
                OnStatUpgraded?.Invoke(statName);
                player.StatsUpgrade();
            }

            PlayerPrefs.Save();

            Debug.Log($"{statName} улучшен до {newValue}. Апгрейдов: {currentUpgradeCount}. Осталось очков: {LevelManager.Instance.GetAvailableUpgradePoints()}");

            if(statisticsUI != null)
            {
                statisticsUI.UpdateStatisticsUI();
            }
        }
        else
        {
            Debug.LogWarning($"Неизвестный продукт: {statName}");
        }
    }

    public int GetUpgradeCount(string statName)
    {
        return PlayerPrefs.GetInt($"{statName}_UpgradeCount", 0);
    }

    public Dictionary<string, StatInfo> GetStatInfos()
    {
        return statInfos;
    }

    [System.Serializable]
    public class StatInfo
    {
        public string StatName { get; private set; }
        public float DefaultValue { get; set; }
        public float Increment { get; set; }

        public StatInfo(string statName, float defaultValue, float increment)
        {
            StatName = statName;
            DefaultValue = defaultValue;
            Increment = increment;
        }
    }
}