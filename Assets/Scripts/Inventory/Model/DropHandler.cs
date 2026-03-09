using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    public class DropHandler
    {
        private List<DropItem> dropItems;
        private InventoryController inventoryController;

        public DropHandler(List<DropItem> dropItems, InventoryController inventoryController)
        {
            this.dropItems = dropItems;
            this.inventoryController = inventoryController;
        }

        /// <summary>
        /// Обрабатывает дроп при убийстве врага.
        /// </summary>
        public void HandleDrop()
        {
            foreach (var dropItem in dropItems)
            {
                float roll = Random.Range(0f, 100f);
                if (roll <= dropItem.dropChance)
                {
                    int quantity = dropItem.GetDroppedQuantity();
                    inventoryController.AddItem(dropItem.item, quantity);
                    Debug.Log($"Выпал предмет: {dropItem.item.name}, количество: {quantity}");
                }
            }
        }
    }
}