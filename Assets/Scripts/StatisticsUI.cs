using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatisticsUI : MonoBehaviour
{
    [SerializeField] private Text attackText;
    [SerializeField] private Text armorText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text critChanceText;
    [SerializeField] private Text dodgeText;
    [SerializeField] private Text speedText;

    [Header("Upgrade Points")]
    [SerializeField] private TextMeshProUGUI upgradePointsText;

    [Header("Upgrade Buttons")]
    [SerializeField] private Button attackUpgradeButton;
    [SerializeField] private Button healthUpgradeButton;
    [SerializeField] private Button agilityUpgradeButton;
    [SerializeField] private Button armorUpgradeButton;
    [SerializeField] private Button speedUpgradeButton;

    [SerializeField] private StatsUpgrade statsUpgrade;
    [SerializeField] private Player player; // Добавьте ссылку на Player

    // Базовые значения для отображения
    private const int baseAttack = 9;
    private const int baseHealth = 10;
    private const int baseCritChance = 5;
    private const int baseDodge = 3;
    private const int baseArmor = 0;
    private const int baseSpeed = 2;

    // Приросты при каждом апгрейде
    private const int attackIncrement = 3;
    private const int healthIncrement = 2;
    private const int critChanceIncrement = 2;
    private const int dodgeIncrement = 1;
    private const int armorIncrement = 3;
    private const int speedIncrement = 2;

    private void Start()
    {
        UpdateStatisticsUI();

        if(LevelManager.Instance != null)
        {
            LevelManager.Instance.OnUpgradePointsChanged += UpdateUpgradePointsUI;
            LevelManager.Instance.OnLevelChanged += OnLevelChanged;
        }

        SetupUpgradeButtons();
    }

    private void OnDestroy()
    {
        if(LevelManager.Instance != null)
        {
            LevelManager.Instance.OnUpgradePointsChanged -= UpdateUpgradePointsUI;
            LevelManager.Instance.OnLevelChanged -= OnLevelChanged;
        }

        if(statsUpgrade != null)
        {
            statsUpgrade.OnStatUpgraded -= HandleStatUpgraded;
        }
    }

    /// <summary>
    /// Метод для обновления UI статистик с учётом модификаторов.
    /// </summary>
    public void UpdateStatisticsUI()
    {
        if(player == null)
        {
            Debug.LogError("Player не назначен в StatisticsUI.");
            return;
        }

        // Получение количества апгрейдов из PlayerPrefs
        int attackUpgrades = statsUpgrade.GetUpgradeCount("Player Attack");
        int healthUpgrades = statsUpgrade.GetUpgradeCount("Player Max Health");
        int agilityUpgrades = statsUpgrade.GetUpgradeCount("Player Agility");
        int armorUpgrades = statsUpgrade.GetUpgradeCount("Player Protection");
        int speedUpgrades = statsUpgrade.GetUpgradeCount("Player Speed");

        int displayAttack = baseAttack + (int)player.AttackDamage + attackUpgrades * attackIncrement;
        int displayHealth = baseHealth + (int)player.StartingHealth + healthUpgrades * healthIncrement;
        int displayCritChance = baseCritChance + agilityUpgrades * critChanceIncrement;
        int displayDodge = baseDodge + (int)player.Agility + agilityUpgrades * dodgeIncrement;
        int displayArmor = baseArmor + (int)player.Protection + armorUpgrades * armorIncrement;
        int displaySpeed = baseSpeed + (int)player.Speed + speedUpgrades * speedIncrement;

        // Установка текстов
        attackText.text = $"{displayAttack}";
        healthText.text = $"{displayHealth}";
        critChanceText.text = $"{displayCritChance}%";
        dodgeText.text = $"{displayDodge}";
        armorText.text = $"{displayArmor}";
        speedText.text = $"{displaySpeed}";

        // Обновление доступных очков
        if(LevelManager.Instance != null)
        {
            UpdateUpgradePointsUI(LevelManager.Instance.GetAvailableUpgradePoints());
        }
    }

    /// <summary>
    /// Метод вызывается при изменении уровня.
    /// </summary>
    private void OnLevelChanged(int newLevel)
    {
        Debug.Log($"Игрок достиг уровня {newLevel}");
        // Дополнительная логика при повышении уровня при необходимости
    }

    /// <summary>
    /// Метод для обновления UI доступных очков.
    /// </summary>
    private void UpdateUpgradePointsUI(int newPoints)
    {
        upgradePointsText.text = $"{newPoints}";
        UpdateUpgradeButtonsInteractable();
    }

    /// <summary>
    /// Настройка кнопок прокачки.
    /// </summary>
    private void SetupUpgradeButtons()
    {
        if(attackUpgradeButton != null)
            attackUpgradeButton.onClick.AddListener(() => statsUpgrade.ProductUpgrade("Player Attack"));
        if(healthUpgradeButton != null)
            healthUpgradeButton.onClick.AddListener(() => statsUpgrade.ProductUpgrade("Player Max Health"));
        if(agilityUpgradeButton != null)
            agilityUpgradeButton.onClick.AddListener(() => statsUpgrade.ProductUpgrade("Player Agility"));
        if(armorUpgradeButton != null)
            armorUpgradeButton.onClick.AddListener(() => statsUpgrade.ProductUpgrade("Player Protection"));
        if(speedUpgradeButton != null)
            speedUpgradeButton.onClick.AddListener(() => statsUpgrade.ProductUpgrade("Player Speed"));

        if(statsUpgrade != null)
        {
            statsUpgrade.OnStatUpgraded += HandleStatUpgraded;
        }

        UpdateUpgradeButtonsInteractable();
    }

    /// <summary>
    /// Обработчик события апгрейда для обновления статистик.
    /// </summary>
    /// <param name="upgradedStatName">Название улучшенного статуса.</param>
    private void HandleStatUpgraded(string upgradedStatName)
    {
        UpdateStatisticsUI();
    }

    /// <summary>
    /// Обновление состояния доступности кнопок прокачки.
    /// </summary>
    private void UpdateUpgradeButtonsInteractable()
    {
        bool canUpgrade = LevelManager.Instance != null && LevelManager.Instance.GetAvailableUpgradePoints() > 0;
        if(attackUpgradeButton != null)
            attackUpgradeButton.interactable = canUpgrade;
        if(healthUpgradeButton != null)
            healthUpgradeButton.interactable = canUpgrade;
        if(agilityUpgradeButton != null)
            agilityUpgradeButton.interactable = canUpgrade;
        if(armorUpgradeButton != null)
            armorUpgradeButton.interactable = canUpgrade;
        if(speedUpgradeButton != null)
            speedUpgradeButton.interactable = canUpgrade;
    }
}