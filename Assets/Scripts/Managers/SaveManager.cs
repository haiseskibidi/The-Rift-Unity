using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private HashSet<string> pickedItems = new HashSet<string>();

    private const string PickedItemsKey = "PickedItems";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPickedItems();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadPickedItems();
    }

    /// <summary>
    /// Сохраняет ID подобранного предмета.
    /// </summary>
    public void PickUpItem(string itemID)
    {
        if (pickedItems.Add(itemID)) // Добавляем только если ID еще нет
        {
            SavePickedItems();
            Debug.Log($"Предмет с ID {itemID} добавлен в список подобранных.");
        }
        else
        {
            Debug.Log($"Предмет с ID {itemID} уже был подобран ранее.");
        }
    }

    /// <summary>
    /// Проверяет, был ли предмет подобран.
    /// </summary>
    public bool IsItemPicked(string itemID)
    {
        bool isPicked = pickedItems.Contains(itemID);
        Debug.Log($"Проверка предмета с ID {itemID}: {(isPicked ? "Подобран" : "Не подобран")}");
        return isPicked;
    }

    /// <summary>
    /// Сохраняет список подобранных предметов в PlayerPrefs.
    /// </summary>
    private void SavePickedItems()
    {
        string serialized = string.Join(",", pickedItems);
        PlayerPrefs.SetString(PickedItemsKey, serialized);
        PlayerPrefs.Save();
        Debug.Log("Список подобранных предметов сохранен.");
    }

    /// <summary>
    /// Загружает список подобранных предметов из PlayerPrefs.
    /// </summary>
    private void LoadPickedItems()
    {
        if (PlayerPrefs.HasKey(PickedItemsKey))
        {
            string serialized = PlayerPrefs.GetString(PickedItemsKey);
            if (!string.IsNullOrEmpty(serialized))
            {
                string[] ids = serialized.Split(',');
                pickedItems = new HashSet<string>(ids);
                Debug.Log($"Загружено подобранных предметов: {pickedItems.Count}");
            }
        }
    }

    /// <summary>
    /// Сбрасывает список подобранных предметов.
    /// </summary>
    public void ResetPickedItems()
    {
        pickedItems.Clear();
        PlayerPrefs.DeleteKey(PickedItemsKey);
        PlayerPrefs.Save();
        Debug.Log("Список подобранных предметов сброшен.");
    }
}