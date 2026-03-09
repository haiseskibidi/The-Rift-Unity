using System;
using Inventory.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using PlayerSettings;
using Inventory.Model;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private DefaultSettings defaultSettings;

    public List<string> KilledEnemies { get; private set; } = new List<string>();

    public Vector2 PlayerPosition { get; set; }

    private string targetDoorID;

    [SerializeField]
    private InventorySaveManager inventorySaveManager;

    [SerializeField]
    private InventorySO inventoryData;

    public Transform spawnPoint;

    private string lastSceneName;

    public string LastSceneName
    {
        get { return lastSceneName; }
        private set { lastSceneName = value; }
    }

    // Новый список для хранения использованных триггеров
    private HashSet<string> usedTriggers = new HashSet<string>();

    // Добавляем событие для уведомления об убийстве врага
    public event Action<string> OnEnemyKilled;

    public bool IsPlayerDead { get; private set; } = false;

    public bool HasStartedNewGame { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Не уничтожать при загрузке новой сцены
            LoadGame(); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetPlayerDead(bool dead)
    {
        IsPlayerDead = dead;
        PlayerPrefs.SetInt("PlayerIsDead", dead ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Метод для загрузки состояния смерти игрока
    public void LoadDeathState()
    {
        IsPlayerDead = PlayerPrefs.GetInt("PlayerIsDead", 0) == 1;
    }

    // Метод для пометки триггера как использованного
    public void MarkTriggerAsUsed(string triggerID)
    {
        if (!string.IsNullOrEmpty(triggerID))
        {
            usedTriggers.Add(triggerID);
            SaveGame(); 
        }
    }

    // Метод для проверки, использован ли триггер
    public bool IsTriggerUsed(string triggerID)
    {
        return usedTriggers.Contains(triggerID);
    }

    public void SetTargetDoor(string doorID)
    {
        targetDoorID = doorID;
    }

    public void RespawnPlayer()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.ResetHealth();
        }
        if (spawnPoint != null)
        {
            PlayerPosition = spawnPoint.position; 
            ChangeScene("Game", PlayerPosition);
        }
    }

    public void ChangeScene(string sceneName, Vector2 newPlayerPosition)
    {
        // Устанавливаем lastSceneName как название сцены, которую собираемся загрузить
        lastSceneName = sceneName;
        Debug.Log($"Смена сцены: {sceneName}. Сохранено lastSceneName: {lastSceneName}");

        PlayerPosition = newPlayerPosition; 
        SaveSceneName();
        SceneManager.LoadScene(sceneName);
    }

    public void StartNewGame()
    {
        ResetAllSettings();
        ResetPlayerStats();
        KilledEnemies.Clear();
        usedTriggers.Clear(); // Сброс списка использованных триггеров

        // Сохранение пустого списка убитых врагов и триггеров
        PlayerPrefs.SetString("KilledEnemies", string.Empty);
        PlayerPrefs.SetString("UsedTriggers", string.Empty);

        HasStartedNewGame = true;
        PlayerPrefs.SetInt("HasStartedNewGame", 1);
        
        PlayerPrefs.Save();

        SetPlayerDead(false);

        // Сброс списка подобранных предметов
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ResetPickedItems();
        }

        if (inventorySaveManager != null && inventoryData != null)
        {
            inventoryData.DroppedItems.Clear();
            inventorySaveManager.SaveInventory(inventoryData); // Сохраняем после очистки
        }

        if (spawnPoint != null)
        {
            PlayerPosition = spawnPoint.transform.position;
            PlayerPrefs.SetFloat("PlayerPosX", PlayerPosition.x);
            PlayerPrefs.SetFloat("PlayerPosY", PlayerPosition.y);
        }
        else
        {
            PlayerPrefs.DeleteKey("PlayerPosX");
            PlayerPrefs.DeleteKey("PlayerPosY");
        }

        // Установка начальной сцены
        lastSceneName = "Game";
        PlayerPrefs.SetString("LastSceneName", lastSceneName);
        Debug.Log($"Новая игра. lastSceneName установлен на 'Game'.");

        PlayerPrefs.Save();
        SceneManager.LoadScene("LoadLobby");
    }

    public void ContinueGame()
    {
        if (!string.IsNullOrEmpty(lastSceneName))
        {
            SceneManager.LoadScene("LoadLobby");
        }
    }

    public void SaveGame()
    {
        SaveKilledEnemies();

        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            PlayerPosition = player.transform.position;
            PlayerPrefs.SetFloat("PlayerPosX", PlayerPosition.x);
            PlayerPrefs.SetFloat("PlayerPosY", PlayerPosition.y);
        }
        PlayerPrefs.SetString("UsedTriggers", string.Join(",", usedTriggers));
        PlayerPrefs.Save();
        Debug.Log("Игра сохранена.");
    }

    public void SaveSceneName()
    {
        PlayerPrefs.SetString("LastSceneName", lastSceneName);
        PlayerPrefs.Save();
    }

    private void LoadGame()
    {
        LoadKilledEnemies();
        LoadUsedTriggers(); 
        LoadDeathState(); 
        if (PlayerPrefs.HasKey("LastSceneName"))
        {
            lastSceneName = PlayerPrefs.GetString("LastSceneName");
            float x = PlayerPrefs.GetFloat("PlayerPosX");
            float y = PlayerPrefs.GetFloat("PlayerPosY");
            PlayerPosition = new Vector2(x, y);
            Debug.Log($"Последняя сцена загружена: {lastSceneName}, позиция игрока: X={x}, Y={y}");
        }
        else
        {
            Debug.LogWarning("Ключ 'LastSceneName' не найден. Используется сцена по умолчанию.");
            lastSceneName = "Game";
            PlayerPosition = Vector2.zero;
        }
        HasStartedNewGame = PlayerPrefs.GetInt("HasStartedNewGame", 0) == 1;
    }

    public Vector2 GetPlayerPosition()
    {
        return PlayerPosition;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Menu")
        {
            return;
        }
        if (!string.IsNullOrEmpty(targetDoorID))
        {
            // Существующая логика установки позиции через дверь
            SceneEntryPoint entryPoint = FindObjectOfType<SceneEntryPoint>();
            if (entryPoint != null && entryPoint.doorID == targetDoorID)
            {
                Player player = FindObjectOfType<Player>();
                if (player != null)
                {
                    Vector3 adjustedPosition = entryPoint.transform.position + new Vector3(0, -1.1f, 0);
                    player.transform.position = adjustedPosition;
                    player.SetIdleAnimation();
                }
                else
                {
                    Debug.LogWarning("Player не найден в новой сцене.");
                }

                targetDoorID = string.Empty;
            }
            else
            {
                Debug.LogWarning($"Не найдена дверь с ID: {targetDoorID} в сцене {scene.name}");
            }
        }
        else
        {
            // Установка позиции игрока из сохранённых данных
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                player.transform.position = PlayerPosition;
                player.SetIdleAnimation();
            }
            else
            {
                Debug.LogWarning("Player не найден для установки сохранённой позиции.");
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void AddKilledEnemy(string enemyID)
    {
        if (!KilledEnemies.Contains(enemyID))
        {
            KilledEnemies.Add(enemyID);
            SaveKilledEnemies();
            Debug.Log($"Враг с ID {enemyID} добавлен в список убитых врагов.");

            // Вызов события
            OnEnemyKilled?.Invoke(enemyID);
        }
    }

    public bool IsEnemyKilled(string enemyID)
    {
        return KilledEnemies.Contains(enemyID);
    }

    private void SaveKilledEnemies()
    {
        PlayerPrefs.SetString("KilledEnemies", string.Join(",", KilledEnemies));
        PlayerPrefs.Save();
    }

    private void LoadKilledEnemies()
    {
        string killedEnemiesString = PlayerPrefs.GetString("KilledEnemies", string.Empty);
        if (!string.IsNullOrEmpty(killedEnemiesString))
        {
            KilledEnemies = new List<string>(killedEnemiesString.Split(','));
            Debug.Log($"Загружено убитых врагов: {KilledEnemies.Count}");
        }
    }

    private void ResetAllSettings()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.ResetLevelAndUpgrades();
    }

    private void LoadPlayerPosition()
    {
        if (PlayerPrefs.HasKey("PlayerPosX") && PlayerPrefs.HasKey("PlayerPosY"))
        {
            float x = PlayerPrefs.GetFloat("PlayerPosX");
            float y = PlayerPrefs.GetFloat("PlayerPosY");
            PlayerPosition = new Vector2(x, y);
        }
        else
        {
            Debug.Log("Сохранённая позиция игрока не найдена. Используется позиция по умолчанию.");
            PlayerPosition = Vector2.zero; // Или установите другую позицию по умолчанию
        }
    }

    private void ResetPlayerStats()
    {
        // Установка значений из defaultSettings
        PlayerPrefs.SetFloat("Player Speed", defaultSettings.defaultSpeed);
        PlayerPrefs.SetFloat("Player Attack", defaultSettings.defaultAttack);
        PlayerPrefs.SetFloat("Player Protection", defaultSettings.defaultProtection);
        PlayerPrefs.SetFloat("Player Agility", defaultSettings.defaultAgility);
        PlayerPrefs.SetFloat("Player Max Health", defaultSettings.defaultMaxHealth);
        PlayerPrefs.SetFloat("Player Current Health", defaultSettings.defaultCurrentHealth);
        PlayerPrefs.SetFloat("Critical Chance", defaultSettings.defaultCriticalChance);

        // Установка количества апгрейдов по умолчанию
        PlayerPrefs.SetInt("Player Speed_UpgradeCount", defaultSettings.defaultSpeedUpgradeCount);
        PlayerPrefs.SetInt("Player Attack_UpgradeCount", defaultSettings.defaultAttackUpgradeCount);
        PlayerPrefs.SetInt("Player Protection_UpgradeCount", defaultSettings.defaultProtectionUpgradeCount);
        PlayerPrefs.SetInt("Player Agility_UpgradeCount", defaultSettings.defaultAgilityUpgradeCount);
        PlayerPrefs.SetInt("Player Max Health_UpgradeCount", defaultSettings.defaultMaxHealthUpgradeCount);

        PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
        SaveSceneName();
    }

    // Методы для сохранения и загрузки использованных триггеров
    private void LoadUsedTriggers()
    {
        string usedTriggersString = PlayerPrefs.GetString("UsedTriggers", string.Empty);
        if (!string.IsNullOrEmpty(usedTriggersString))
        {
            string[] triggers = usedTriggersString.Split(',');
            usedTriggers = new HashSet<string>(triggers);
            Debug.Log($"Загружено использованных триггеров: {usedTriggers.Count}");
        }
    }
}