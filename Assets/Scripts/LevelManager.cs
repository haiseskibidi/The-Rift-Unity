using UnityEngine;
using System;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    // События
    public event Action<float> OnExperienceChanged;
    public event Action<int> OnLevelChanged;
    public event Action<int> OnUpgradePointsChanged;

    [SerializeField] private int currentLevel = 1;
    [SerializeField] private float currentExperience = 0f;
    [SerializeField] private float maxExperience = 10f; // Начальное максимальное количество опыта
    [SerializeField] private int availableUpgradePoints = 0;
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLevelData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadLevelData()
    {
        currentLevel = PlayerPrefs.GetInt("Player Current Level", 1);
        currentExperience = PlayerPrefs.GetFloat("Current Experience", 0f);
        maxExperience = PlayerPrefs.GetFloat("Max Experience", 10f);
        availableUpgradePoints = PlayerPrefs.GetInt("Player Upgrade Points", 0);
        Debug.Log($"Уровень загружен: {currentLevel}. Текущий опыт: {currentExperience}/{maxExperience}. Доступные очки апгрейда: {availableUpgradePoints}");
    }

    /// <summary>
    /// Метод для добавления опыта игроку.
    /// </summary>
    /// <param name="amount">Количество опыта для добавления.</param>
    public void AddExperience(float amount)
    {
        currentExperience += amount;
        PlayerPrefs.SetFloat("Current Experience", currentExperience);
        PlayerPrefs.Save();

        OnExperienceChanged?.Invoke(amount);
        Debug.Log($"Добавлено {amount} опыта. Текущий опыт: {currentExperience}/{maxExperience}");

        CheckLevelUp();
    }

    /// <summary>
    /// Проверяет, достиг ли игрок необходимого опыта для повышения уровня.
    /// </summary>
    private void CheckLevelUp()
    {
        while (currentExperience >= maxExperience)
        {
            currentExperience -= maxExperience;
            PlayerPrefs.SetFloat("Current Experience", currentExperience);
            LevelUp();
            maxExperience *= 1.3f; // Увеличиваем требуемый опыт на 30%
            PlayerPrefs.SetFloat("Max Experience", maxExperience);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Метод для повышения уровня игрока.
    /// </summary>
    private void LevelUp()
    {
        currentLevel++;
        availableUpgradePoints += 2;
        PlayerPrefs.SetInt("Player Current Level", currentLevel);
        PlayerPrefs.SetInt("Player Upgrade Points", availableUpgradePoints);
        PlayerPrefs.Save();

        Debug.Log($"Уровень повышен до {currentLevel}. Доступно очков апгрейда: {availableUpgradePoints}");

        OnLevelChanged?.Invoke(currentLevel);
        OnUpgradePointsChanged?.Invoke(availableUpgradePoints);
    }

    /// <summary>
    /// Метод для расходования очков апгрейда.
    /// </summary>
    public bool SpendUpgradePoint()
    {
        if (availableUpgradePoints > 0)
        {
            availableUpgradePoints--;
            PlayerPrefs.SetInt("Player Upgrade Points", availableUpgradePoints);
            PlayerPrefs.Save();
            Debug.Log($"Очко апгрейда потрачено. Осталось: {availableUpgradePoints}");
            OnUpgradePointsChanged?.Invoke(availableUpgradePoints);
            return true;
        }
        else
        {
            Debug.LogWarning("Недостаточно очков для прокачки.");
            return false;
        }
    }

    /// <summary>
    /// Метод для добавления очков апгрейда.
    /// </summary>
    /// <param name="amount">Количество очков для добавления (по умолчанию 1).</param>
    public void AddUpgradePoint(int amount = 1)
    {
        availableUpgradePoints += amount;
        PlayerPrefs.SetInt("Player Upgrade Points", availableUpgradePoints);
        PlayerPrefs.Save();

        Debug.Log($"Добавлено {amount} очков апгрейда. Доступно очков: {availableUpgradePoints}");
        OnUpgradePointsChanged?.Invoke(availableUpgradePoints);
    }

    /// <summary>
    /// Возвращает текущее количество доступных очков апгрейда.
    /// </summary>
    public int GetAvailableUpgradePoints()
    {
        return availableUpgradePoints;
    }

    /// <summary>
    /// Возвращает текущий уровень игрока.
    /// </summary>
    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    /// <summary>
    /// Возвращает текущее количество опыта игрока.
    /// </summary>
    public float GetCurrentExperience()
    {
        return currentExperience;
    }

    /// <summary>
    /// Возвращает максимальное количество опыта для следующего уровня.
    /// </summary>
    public float GetMaxExperience()
    {
        return maxExperience;
    }

    /// <summary>
    /// Сбрасывает уровень и очки апгрейда до значений по умолчанию.
    /// </summary>
    public void ResetLevelAndUpgrades()
    {
        currentLevel = 1;
        currentExperience = 0f;
        maxExperience = 10f;
        availableUpgradePoints = 0;
        PlayerPrefs.SetInt("Player Current Level", currentLevel);
        PlayerPrefs.SetFloat("Current Experience", currentExperience);
        PlayerPrefs.SetFloat("Max Experience", maxExperience);
        PlayerPrefs.SetInt("Player Upgrade Points", availableUpgradePoints);
        PlayerPrefs.Save();

        Debug.Log("Уровень и очки апгрейда сброшены до значений по умолчанию.");

        OnLevelChanged?.Invoke(currentLevel);
        OnUpgradePointsChanged?.Invoke(availableUpgradePoints);
    }
}