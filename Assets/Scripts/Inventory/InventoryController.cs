using Inventory.Model;
using Inventory.Save;
using Inventory.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField]
        private UIInventoryPage inventoryUI;

        [SerializeField]
        private InventorySO inventoryData;

        [SerializeField]
        private AudioClip dropClip;

        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private GameObject itemPrefab; // Префаб предмета для инстанциации

        [SerializeField]
        private Transform playerTransform; // Трансформ игрока для определения позиции выпадения

        [SerializeField]
        private float dropYOffset = 0.24f;

        [SerializeField]
        private InventorySaveManager saveManager; 

        [SerializeField]
        private ItemParameterSO durabilityParameter;

        [SerializeField]
        private AgentWeapon agentWeapon;

        private void Start()
        {
            PrepareUI();
            LoadInventory();
            EquipLastEquippedItem(); 
        }

        private void PrepareUI()
        {
            inventoryUI.InitializeInventoryUI(inventoryData.Size);
            inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
            inventoryUI.OnSwapItems += HandleSwapItems;
            inventoryUI.OnItemActionRequested += HandleItemActionRequest;
            inventoryUI.OnStartDragging += HandleDragging;
            inventoryData.OnInventoryUpdated += UpdateInventoryUI;
            agentWeapon.OnWeaponParameterZero += HandleWeaponParameterZero;
            inventoryData.OnInventoryUpdated += (inventoryState) => saveManager.SaveInventory(inventoryData);
        }

        /// <summary>
        /// Экипирует последний экипированный предмет при старте игры.
        /// </summary>
        private void EquipLastEquippedItem()
        {
            int equippedIndex = inventoryData.LastEquippedItemIndex;

            if (equippedIndex < 0 || equippedIndex >= inventoryData.Size)
            {
                Debug.LogWarning("Нет сохранённого экипированного предмета или индекс вне диапазона.");
                return;
            }

            InventoryItem itemToEquip = inventoryData.GetItemAt(equippedIndex);
            if (itemToEquip.IsEmpty)
            {
                Debug.LogWarning($"Предмет по индексу {equippedIndex} пуст.");
                return;
            }

            EquippableItemSO equippableItem = itemToEquip.item as EquippableItemSO;
            if (equippableItem == null)
            {
                Debug.LogWarning($"Предмет по индексу {equippedIndex} не является экипируемым.");
                return;
            }

            // Экипировка предмета через AgentWeapon
            agentWeapon.SetWeapon(equippableItem, itemToEquip.itemState);
            Debug.Log($"Экипирован предмет по индексу {equippedIndex}: {equippableItem.Name}");
        }

        private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
        {
            inventoryUI.ResetAllItems();
            foreach (var item in inventoryState)
            {
                inventoryUI.UpdateData(item.Key, item.Value.item.ItemImage,
                    item.Value.quantity);
            }
        }

        private void HandleWeaponParameterZero(EquippableItemSO weapon)
        {
            if (!inventoryUI.gameObject.activeInHierarchy)
                return;

            int equippedIndex = inventoryData.LastEquippedItemIndex;
            if (equippedIndex >=0 && equippedIndex < inventoryData.Size)
            {
                InventoryItem item = inventoryData.GetItemAt(equippedIndex);
                if (item.item == weapon)
                {
                    // Обновляем UI для отображения кнопки "Destroy"
                    inventoryUI.ShowItemAction(equippedIndex);
                    inventoryUI.AddAction("Destroy", () => DestroyItem(equippedIndex));
                }
            }
        }

        // Новый метод для сброса инвентаря и добавления начальных предметов
        public void ResetInventory()
        {
            inventoryData.ResetToInitialItems(); // Используем метод из InventorySO
            inventoryData.DroppedItems.Clear();   // Очищаем список дропнутых предметов
            inventoryData.LastEquippedItemIndex = -1; // Сбрасываем индекс экипировки
            saveManager.SaveInventory(inventoryData);
            Debug.Log("Инвентарь сброшен и инициализирован начальными предметами.");
        }

        private void UnequipWeapon()
        {
            if (inventoryData.LastEquippedItemIndex == -1)
            {
                return;
            }

            InventoryItem equippedItem = inventoryData.GetItemAt(inventoryData.LastEquippedItemIndex);
            EquippableItemSO equippableItem = equippedItem.item as EquippableItemSO;

            if (equippableItem != null)
            {
                agentWeapon.RemoveWeapon(equippableItem);
                inventoryData.LastEquippedItemIndex = -1;
                saveManager.SaveInventory(inventoryData);
                Debug.Log($"Снято экипированное оружие: {equippableItem.Name}");
            }
            inventoryUI.ResetSelection();
        }

        private void HandleItemActionRequest(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
                return;
            
            bool isEquipped = (itemIndex == inventoryData.LastEquippedItemIndex);

            if (isEquipped)
            {
                inventoryUI.ShowItemAction(itemIndex);
                inventoryUI.AddAction("Unequip", () => UnequipWeapon());

                EquippableItemSO equippableItem = inventoryItem.item as EquippableItemSO;
                if (equippableItem != null)
                {
                    float parameterValue = agentWeapon.GetWeaponParameterValue(durabilityParameter);
                    if (parameterValue <= 0)
                    {
                        inventoryUI.AddAction("Destroy", () => DestroyItem(itemIndex));
                    }
                }
            }
            else
            {
                IItemAction itemAction = inventoryItem.item as IItemAction;
                if (itemAction != null)
                {
                    inventoryUI.ShowItemAction(itemIndex);
                    inventoryUI.AddAction(itemAction.ActionName, () => PerformAction(itemIndex));
                }
            }

            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                EquippableItemSO equippableItem = inventoryItem.item as EquippableItemSO;
                if (equippableItem != null)
                {
                    if (equippableItem.CanDrop)
                    {
                        inventoryUI.AddAction("Drop", () => DropItem(itemIndex, inventoryItem.quantity));
                    }
                }
                else
                {
                    inventoryUI.AddAction("Drop", () => DropItem(itemIndex, inventoryItem.quantity));
                }
            }
        }

        /// <summary>
        /// Метод для уничтожения предмета из инвентаря.
        /// </summary>
        private void DestroyItem(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
                return;

            // Удаление предмета из инвентаря
            inventoryData.RemoveItem(itemIndex, inventoryItem.quantity);
            inventoryUI.ResetSelection();
            saveManager.SaveInventory(inventoryData);

            Debug.Log($"Предмет {inventoryItem.item.Name} был уничтожен из инвентаря.");
        }

        /// <summary>
        /// Метод для дропа предмета из инвентаря.
        /// </summary>
        private void DropItem(int itemIndex, int quantity)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
                return;

            // Удаление предмета из инвентаря
            inventoryData.RemoveItem(itemIndex, quantity);
            inventoryUI.ResetSelection();
            audioSource.PlayOneShot(dropClip);

            // Инстанциация предмета в мире под игроком
            Vector3 dropPosition = playerTransform.position + new Vector3(0, dropYOffset, 0);
            GameObject droppedItemGO = Instantiate(itemPrefab, dropPosition, Quaternion.identity);
            Item droppedItemComponent = droppedItemGO.GetComponent<Item>();
            if (droppedItemComponent != null)
            {
                // Установка itemID перед активацией объекта
                string newItemID = Guid.NewGuid().ToString();
                droppedItemComponent.SetItem(inventoryItem.item, quantity, newItemID);
            }
            else
            {
                Debug.LogError("Префаб предмета не содержит компонент Item.");
            }

            // Добавление DroppedItem к InventoryData
            DroppedItem droppedData = new DroppedItem(
                droppedItemGO.GetComponent<Item>().ItemID,
                inventoryItem.item.Name,
                dropPosition,
                quantity
            );
            inventoryData.DroppedItems.Add(droppedData);
            saveManager.SaveInventory(inventoryData);

            Debug.Log($"Предмет с ID {droppedData.itemID} был дропнут и сохранён.");
        }

        public void AddItem(ItemSO item, int quantity)
        {
            inventoryData.AddItem(item, quantity);
            UpdateInventoryUI(inventoryData.GetCurrentInventoryState());
        }

        /// <summary>
        /// Выполнение действия над предметом.
        /// </summary>
        public void PerformAction(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
                return;

            IItemAction itemAction = inventoryItem.item as IItemAction;
            if (itemAction != null)
            {
                // Проверяем, является ли предмет EdibleItemSO и требует ли он ремонта
                if (inventoryItem.item is EdibleItemSO edibleItem && edibleItem.RepairItem)
                {
                    AgentWeapon agentWeapon = gameObject.GetComponent<AgentWeapon>();
                    if (agentWeapon == null || agentWeapon.CurrentWeapon == null)
                    {
                        return;
                    }
                }

                bool actionResult = itemAction.PerformAction(gameObject, inventoryItem.itemState);
                if (actionResult)
                {
                    if (inventoryItem.item is EdibleItemSO)
                    {
                        inventoryData.RemoveItem(itemIndex, 1);
                        audioSource.PlayOneShot(itemAction.actionSFX);
                    
                        // Проверка, пуст ли предмет после уменьшения количества
                        if (inventoryData.GetItemAt(itemIndex).IsEmpty)
                            inventoryUI.ResetSelection();
                    }
                    else
                    {
                        // Если предмет не съедобный, просто проигрываем звук действия
                        audioSource.PlayOneShot(itemAction.actionSFX);
                    }

                    // Если предмет экипируемый, обновляем индекс экипировки
                    EquippableItemSO equippableItem = inventoryItem.item as EquippableItemSO;
                    if (equippableItem != null)
                    {
                        EquipWeapon(itemIndex, equippableItem, inventoryItem.itemState);
                    }
                }
                inventoryUI.ResetSelection();
            }
        }

        /// <summary>
        /// Экипирует оружие и обновляет индекс.
        /// </summary>
        private void EquipWeapon(int itemIndex, EquippableItemSO equippableItem, List<ItemParameter> itemState)
        {
            // Снимаем предыдущее оружие, если есть
            if (inventoryData.LastEquippedItemIndex != -1 && inventoryData.LastEquippedItemIndex != itemIndex)
            {
                InventoryItem previousItem = inventoryData.GetItemAt(inventoryData.LastEquippedItemIndex);
                EquippableItemSO previousEquippable = previousItem.item as EquippableItemSO;
                if (previousEquippable != null)
                {
                    agentWeapon.RemoveWeapon(previousEquippable);
                }
            }

            // Экипируем новое оружие
            agentWeapon.SetWeapon(equippableItem, itemState);
            inventoryData.LastEquippedItemIndex = itemIndex;
            saveManager.SaveInventory(inventoryData);
            Debug.Log($"Экипирован предмет по индексу {itemIndex}: {equippableItem.Name}");
        }

        private void HandleDragging(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);

            if (inventoryItem.IsEmpty)
                return;
            inventoryUI.CreateDraggedItem(inventoryItem.item.ItemImage, inventoryItem.quantity);
        }

        private void HandleSwapItems(int itemIndex1, int itemIndex2)
        {
            // Выполняем обмен предметами в инвентаре
            inventoryData.SwapItems(itemIndex1, itemIndex2);

            // Проверяем, был ли один из обменянных предметов экипирован
            bool wasEquipped1 = (inventoryData.LastEquippedItemIndex == itemIndex1);
            bool wasEquipped2 = (inventoryData.LastEquippedItemIndex == itemIndex2);

            if (wasEquipped1)
            {
                // Обновляем индекс экипированного предмета на новый индекс после обмена
                inventoryData.LastEquippedItemIndex = itemIndex2;
                InventoryItem swappedItem = inventoryData.GetItemAt(itemIndex2);
                EquippableItemSO equippableItem = swappedItem.item as EquippableItemSO;
                if (equippableItem != null)
                {
                    agentWeapon.SetWeapon(equippableItem, swappedItem.itemState);
                    Debug.Log($"Экипирован предмет после обмена по новому индексу {itemIndex2}: {equippableItem.Name}");
                }
                else
                {
                    Debug.LogWarning($"Предмет по индексу {itemIndex2} не является экипируемым.");
                }
            }

            if (wasEquipped2)
            {
                // Обновляем индекс экипированного предмета на новый индекс после обмена
                inventoryData.LastEquippedItemIndex = itemIndex1;
                InventoryItem swappedItem = inventoryData.GetItemAt(itemIndex1);
                EquippableItemSO equippableItem = swappedItem.item as EquippableItemSO;
                if (equippableItem != null)
                {
                    agentWeapon.SetWeapon(equippableItem, swappedItem.itemState);
                    Debug.Log($"Экипирован предмет после обмена по новому индексу {itemIndex1}: {equippableItem.Name}");
                }
                else
                {
                    Debug.LogWarning($"Предмет по индексу {itemIndex1} не является экипируемым.");
                }
            }

            // Теперь информируем о изменениях, что вызовет сохранение инвентаря
            inventoryData.InformAboutChange();
        }

        private void HandleDescriptionRequest(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
            {
                inventoryUI.ResetSelection();
                return;
            }
            ItemSO item = inventoryItem.item;
            string description = PrepareDescription(itemIndex, inventoryItem);
            inventoryUI.UpdateDescription(itemIndex, item.ItemImage,
                item.Name, description);
        }

        private string PrepareDescription(int itemIndex, InventoryItem inventoryItem)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(inventoryItem.item.Description);
            sb.AppendLine();

            // Добавляем текущие параметры предмета
            for (int i = 0; i < inventoryItem.itemState.Count; i++)
            {
                sb.Append($"{inventoryItem.itemState[i].itemParameter.ParameterName} " +
                    $": {inventoryItem.itemState[i].value} / " +
                    $"{inventoryItem.item.DefaultParametersList[i].value}");
                sb.AppendLine();
            }

            // Проверяем, является ли предмет экипируемым
            if (inventoryItem.item is EquippableItemSO equippableItem)
            {
                // Используем LastEquippedItemIndex для проверки экипировки
                if (itemIndex == inventoryData.LastEquippedItemIndex)
                {
                    sb.AppendLine("<b>Экипировано</b>");
                }

                // Добавляем описание модификаторов статов
                foreach (var modifierData in equippableItem.ModifiersData)
                {
                    string statName = GetStatName(modifierData.statModifier);
                    if (!string.IsNullOrEmpty(statName))
                    {
                        sb.AppendLine($"{statName} +{modifierData.value}");
                    }
                }
            }

            return sb.ToString();
        }

        private string GetStatName(CharacterStatModifierSO modifier)
        {
            string className = modifier.GetType().Name;
            if (className.EndsWith("ModifierSO"))
            {
                return className.Replace("ModifierSO", "");
            }
            return string.Empty;
        }

        /// <summary>
        /// Сохранение инвентаря.
        /// </summary>
        public void SaveInventory()
        {
            saveManager.SaveInventory(inventoryData);
        }

        /// <summary>
        /// Загрузка инвентаря и восстановление дропнутых предметов.
        /// </summary>
        public void LoadInventory()
        {
            saveManager.LoadInventory(inventoryData);

            // Инстанциирование дропнутых предметов
            foreach (var droppedItem in inventoryData.DroppedItems)
            {
                // Проверяем, не был ли предмет уже подобран
                if (SaveManager.Instance.IsItemPicked(droppedItem.itemID))
                    continue;
               
                // Получаем ItemSO по itemType через ItemRegistry
                ItemSO itemSO = ItemRegistry.Instance.GetItemByName(droppedItem.itemType);
                if (itemSO == null)
                {
                    Debug.LogError($"Item with type {droppedItem.itemType} not found in registry.");
                    continue;
                }

                // Инстанциируем предмет
                GameObject droppedItemGO = Instantiate(itemPrefab, droppedItem.position, Quaternion.identity);
                Item droppedItemComponent = droppedItemGO.GetComponent<Item>();
                if (droppedItemComponent != null)
                {
                    droppedItemComponent.SetItem(itemSO, droppedItem.quantity, droppedItem.itemID);
                }
                else
                {
                    Debug.LogError("Префаб предмета не содержит компонент Item.");
                }
            }
        }

        private void OnDestroy()
        {
            // Отписываемся от событий
            if (inventoryData != null)
            {
                inventoryData.OnInventoryUpdated -= UpdateInventoryUI;
                inventoryData.OnInventoryUpdated -= (inventoryState) => saveManager.SaveInventory(inventoryData);
            }
            if (agentWeapon != null)
            {
                agentWeapon.OnWeaponParameterZero -= HandleWeaponParameterZero;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                if (!inventoryUI.isActiveAndEnabled)
                {
                    // Проверяем через UIManager
                    if (UIManager.Instance.RequestOpenUI(UIState.Inventory))
                    {
                        inventoryUI.Show();
                        foreach (var item in inventoryData.GetCurrentInventoryState())
                        {
                            inventoryUI.UpdateData(item.Key,
                                item.Value.item.ItemImage,
                                item.Value.quantity);
                        }
                    }
                }
                else
                {
                    inventoryUI.Hide();
                    UIManager.Instance.CloseCurrentUI();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && inventoryUI.isActiveAndEnabled)
            {
                inventoryUI.Hide();
                UIManager.Instance.CloseCurrentUI();
                UIManager.Instance.HandleEsc();
            }
        }
    }
}