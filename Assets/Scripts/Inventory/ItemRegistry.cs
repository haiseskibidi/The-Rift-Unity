using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    public class ItemRegistry : MonoBehaviour
    {
        public static ItemRegistry Instance { get; private set; }

        [SerializeField]
        private List<ItemSO> allItems; // Список всех доступных ItemSO

        private Dictionary<string, ItemSO> itemDictionary = new Dictionary<string, ItemSO>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeRegistry();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeRegistry()
        {
            foreach (var item in allItems)
            {
                if (!itemDictionary.ContainsKey(item.Name))
                {
                    itemDictionary.Add(item.Name, item);
                }
                else
                {
                    Debug.LogWarning($"Дублирование имен предметов в реестре: {item.Name}");
                }
            }
        }

        /// <summary>
        /// Возвращает ItemSO по имени.
        /// </summary>
        public ItemSO GetItemByName(string name)
        {
            if (itemDictionary.ContainsKey(name))
            {
                return itemDictionary[name];
            }
            else
            {
                Debug.LogError($"Предмет с именем {name} не найден в реестре.");
                return null;
            }
        }
    }
}