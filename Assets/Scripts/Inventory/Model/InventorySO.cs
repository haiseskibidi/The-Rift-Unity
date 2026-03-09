using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(menuName = "Inventory/InventorySO")]
    public class InventorySO : ScriptableObject
    {
        [SerializeField]
        private List<InventoryItem> inventoryItems;

        [SerializeField]
        private List<InventoryItem> initialItems; // Список начальных предметов

        [SerializeField]
        private List<DroppedItem> droppedItems; // Список дропнутых предметов

        [field: SerializeField]
        public int Size { get; private set; } = 10;

        public List<DroppedItem> DroppedItems => droppedItems;

        public int LastEquippedItemIndex { get; set; } = -1; // Индекс последнего экипированного предмета

        public event Action<Dictionary<int, InventoryItem>> OnInventoryUpdated;

        private void OnEnable()
        {
            // Инициализация, если необходимо
            if (droppedItems == null)
            {
                droppedItems = new List<DroppedItem>();
            }

            if (inventoryItems == null)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            inventoryItems = new List<InventoryItem>();
            for (int i = 0; i < Size; i++)
            {
                inventoryItems.Add(InventoryItem.GetEmptyItem());
            }
            droppedItems = new List<DroppedItem>(); // Инициализация дропнутых предметов
            LastEquippedItemIndex = -1; // Инициализация индекса экипировки
            InformAboutChange();
        }

        public void ResetToInitialItems()
        {
            Initialize(); // Очистка инвентаря и дропнутых предметов
            foreach (var item in initialItems)
            {
                if (!item.IsEmpty)
                {
                    AddItem(item);
                }
            }
            InformAboutChange();
        }

        public int AddItem(ItemSO item, int quantity, List<ItemParameter> itemState = null, List<ModifierData> modifiersData = null)
        {
            if (!item.IsStackable)
            {
                for (int i = 0; i < inventoryItems.Count; i++)
                {
                    while (quantity > 0 && !IsInventoryFull())
                    {
                        quantity -= AddItemToFirstFreeSlot(item, 1, itemState, modifiersData);
                    }
                    InformAboutChange();
                    return quantity;
                }
            }
            quantity = AddStackableItem(item, quantity, itemState);
            InformAboutChange();
            return quantity;
        }

        private int AddItemToFirstFreeSlot(ItemSO item, int quantity, List<ItemParameter> itemState = null, List<ModifierData> modifiersData = null)
        {
            InventoryItem newItem = new InventoryItem
            {
                item = item,
                quantity = quantity,
                itemState = new List<ItemParameter>(itemState ?? item.DefaultParametersList),
                modifiersData = new List<ModifierData>(modifiersData ?? item.DefaultModifiersData)
            };

            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].IsEmpty)
                {
                    inventoryItems[i] = newItem;
                    return quantity;
                }
            }
            return 0;
        }

        private bool IsInventoryFull()
            => !inventoryItems.Any(item => item.IsEmpty);

        private int AddStackableItem(ItemSO item, int quantity, List<ItemParameter> itemState = null, List<ModifierData> modifiersData = null)
        {
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].IsEmpty)
                    continue;
                if (inventoryItems[i].item.ID == item.ID)
                {
                    int availableSpace = item.MaxStackSize - inventoryItems[i].quantity;
                    if (quantity > availableSpace)
                    {
                        inventoryItems[i].quantity = item.MaxStackSize;
                        quantity -= availableSpace;
                    }
                    else
                    {
                        inventoryItems[i].quantity += quantity;
                        quantity = 0;
                        InformAboutChange();
                        return 0;
                    }
                }
            }
            while (quantity > 0 && !IsInventoryFull())
            {
                int newQuantity = Mathf.Clamp(quantity, 0, item.MaxStackSize);
                quantity -= newQuantity;
                AddItemToFirstFreeSlot(item, newQuantity, itemState, modifiersData);
            }

            InformAboutChange();
            return quantity;
        }

        public void RemoveDroppedItem(string itemID)
        {
            DroppedItem itemToRemove = DroppedItems.FirstOrDefault(di => di.itemID == itemID);
            if (itemToRemove != null)
            {
                DroppedItems.Remove(itemToRemove);
                InformAboutChange();
            }
        }

        public void RemoveItem(int itemIndex, int amount)
        {
            if (itemIndex < 0 || itemIndex >= inventoryItems.Count)
                return;

            if (inventoryItems[itemIndex].IsEmpty)
                return;

            inventoryItems[itemIndex].quantity -= amount;
            if (inventoryItems[itemIndex].quantity <= 0)
                inventoryItems[itemIndex] = InventoryItem.GetEmptyItem();

            InformAboutChange();
        }

        public void AddItem(InventoryItem item)
        {
            AddItem(item.item, item.quantity, item.itemState, item.modifiersData);
        }

        public Dictionary<int, InventoryItem> GetCurrentInventoryState()
        {
            Dictionary<int, InventoryItem> state = new Dictionary<int, InventoryItem>();
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (!inventoryItems[i].IsEmpty)
                    state[i] = inventoryItems[i];
            }
            return state;
        }

        public InventoryItem GetItemAt(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= inventoryItems.Count)
                return InventoryItem.GetEmptyItem();
            return inventoryItems[itemIndex];
        }

        public void SwapItems(int itemIndex1, int itemIndex2)
        {
            if (itemIndex1 < 0 || itemIndex1 >= inventoryItems.Count ||
                itemIndex2 < 0 || itemIndex2 >= inventoryItems.Count)
                return;

            InventoryItem temp = inventoryItems[itemIndex1];
            inventoryItems[itemIndex1] = inventoryItems[itemIndex2];
            inventoryItems[itemIndex2] = temp;
            InformAboutChange();
        }

        public void InformAboutChange()
        {
            OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
        }

        /// <summary>
        /// Возвращает все элементы инвентаря.
        /// </summary>
        public List<InventoryItem> GetAllItems()
            => inventoryItems;

        /// <summary>
        /// Загружает данные инвентаря из InventoryData.
        /// </summary>
        public void LoadData(InventoryData data)
        {
            Size = data.Size;
            inventoryItems = new List<InventoryItem>(data.InventoryItems);

            // Если размер инвентаря меньше, инициализируем дополнительные слоты
            while (inventoryItems.Count < Size)
            {
                inventoryItems.Add(InventoryItem.GetEmptyItem());
            }

            // Загружаем дропнутые предметы
            droppedItems = new List<DroppedItem>(data.DroppedItems);

            // Загружаем индекс последнего экипированного предмета
            LastEquippedItemIndex = data.LastEquippedItemIndex;

            InformAboutChange();
        }
    }

    [Serializable]
    public class InventoryItem
    {
        public int quantity;
        public ItemSO item;
        public List<ItemParameter> itemState;
        public List<ModifierData> modifiersData;
        public bool IsEmpty => item == null;

        public InventoryItem ChangeQuantity(int newQuantity)
        {
            return new InventoryItem
            {
                item = this.item,
                quantity = newQuantity,
                itemState = new List<ItemParameter>(this.itemState),
                modifiersData = new List<ModifierData>(this.modifiersData)
            };
        }

        public static InventoryItem GetEmptyItem()
            => new InventoryItem
            {
                item = null,
                quantity = 0,
                itemState = new List<ItemParameter>(),
                modifiersData = new List<ModifierData>()
            };
    }
}