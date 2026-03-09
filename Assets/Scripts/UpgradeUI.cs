using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    [SerializeField]
    private StatsUpgrade statsUpgrade; // Ссылка на StatsUpgrade

    [SerializeField]
    private string statName; // Название стат

    [SerializeField]
    private Text upgradeCountText; // Текстовый элемент для отображения количества

    private void Start()
    {
        UpdateUpgradeCount();

        if(statsUpgrade != null)
        {
            statsUpgrade.OnStatUpgraded += HandleStatUpgraded;
        }
    }

    private void OnDestroy()
    {
        if(statsUpgrade != null)
        {
            statsUpgrade.OnStatUpgraded -= HandleStatUpgraded;
        }
    }

    /// <summary>
    /// Метод, вызываемый при нажатии на кнопку апгрейда.
    /// </summary>
    public void OnUpgradeClicked()
    {
        statsUpgrade.ProductUpgrade(statName);
    }

    /// <summary>
    /// Обработчик события апгрейда.
    /// </summary>
    /// <param name="upgradedStatName">Название улучшенного статуса.</param>
    private void HandleStatUpgraded(string upgradedStatName)
    {
        if(upgradedStatName == statName)
        {
            UpdateUpgradeCount();
        }
    }

    /// <summary>
    /// Метод обновления количества апгрейдов.
    /// </summary>
    private void UpdateUpgradeCount()
    {
        if(statsUpgrade != null && upgradeCountText != null)
        {
            int count = statsUpgrade.GetUpgradeCount(statName);
            upgradeCountText.text = count.ToString();
        }
    }
}
