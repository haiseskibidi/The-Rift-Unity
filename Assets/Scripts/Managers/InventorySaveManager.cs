using Inventory.Model;
using System.IO;
using UnityEngine;

namespace Inventory.Save
{
    public class InventorySaveManager : MonoBehaviour
    {
        private const string SaveFileName = "inventory.json";

        /// <summary>
        /// Сохраняет данные инвентаря в JSON-файл.
        /// </summary>
        public void SaveInventory(InventorySO inventory)
        {
            InventoryData data = new InventoryData
            {
                Size = inventory.Size,
                InventoryItems = inventory.GetAllItems(),
                DroppedItems = inventory.DroppedItems, // Сохраняем дропнутые предметы
                LastEquippedItemIndex = inventory.LastEquippedItemIndex // Сохраняем индекс последнего экипированного предмета
            };

            string json = JsonUtility.ToJson(data, true);
            string path = GetSavePath();

            File.WriteAllText(path, json);
            Debug.Log($"Инвентарь сохранён по пути: {path}");
        }

        /// <summary>
        /// Загружает данные инвентаря из JSON-файла.
        /// </summary>
        public void LoadInventory(InventorySO inventory)
        {
            string path = GetSavePath();
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                InventoryData data = JsonUtility.FromJson<InventoryData>(json);

                inventory.LoadData(data);
                Debug.Log($"Инвентарь загружен из пути: {path}");
            }
            else
            {
                Debug.LogWarning("Файл сохранения инвентаря не найден. Инициализируется новый инвентарь.");
                inventory.Initialize();
            }
        }

        /// <summary>
        /// Возвращает путь к файлу сохранения.
        /// </summary>
        private string GetSavePath()
            => Path.Combine(Application.persistentDataPath, SaveFileName);

        /// <summary>
        /// Сброс инвентаря, удаляя все дропнутые предметы.
        /// </summary>
        public void ResetInventory(InventorySO inventory)
        {
            inventory.DroppedItems.Clear();
            inventory.LastEquippedItemIndex = -1; // Сбрасываем индекс экипировки
            SaveInventory(inventory);
            Debug.Log("Инвентарь и список дропнутых предметов сброшены.");
        }
    }
}