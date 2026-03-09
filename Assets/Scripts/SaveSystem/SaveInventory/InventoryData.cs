using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [Serializable]
    public class InventoryData
    {
        public int Size;
        public List<InventoryItem> InventoryItems;
        public List<DroppedItem> DroppedItems; 
        public int LastEquippedItemIndex;

        public InventoryData()
        {
            InventoryItems = new List<InventoryItem>();
            DroppedItems = new List<DroppedItem>();
            LastEquippedItemIndex = -1;
        }
    }

    [Serializable]
    public class DroppedItem
    {
        public string itemID;
        public string itemType; // Тип предмета для восстановления ItemSO
        public Vector3 position;
        public int quantity;

        public DroppedItem(string itemID, string itemType, Vector3 position, int quantity)
        {
            this.itemID = itemID;
            this.itemType = itemType;
            this.position = position;
            this.quantity = quantity;
        }
    }
}