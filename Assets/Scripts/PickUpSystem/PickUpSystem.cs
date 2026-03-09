using Inventory.Model;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory.Save;

public class PickUpSystem : MonoBehaviour
{
    [SerializeField]
    private InventorySO inventoryData;

    [SerializeField]
    private InventorySaveManager saveManager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Item item = collision.GetComponent<Item>();
        if (item != null && item.CanBePickedUp) 
        {
            int reminder = inventoryData.AddItem(item.InventoryItem, item.Quantity);
            if (reminder == 0)
            {
                item.DestroyItem();
                // Сохраняем, что предмет был подобран
                SaveManager.Instance.PickUpItem(item.ItemID); // Предполагается, что PickUpItem находится в InventorySaveManager

                // Удаляем DroppedItem из списка
                inventoryData.RemoveDroppedItem(item.ItemID);
                saveManager.SaveInventory(inventoryData); // Используем saveManager вместо SaveManager.Instance
            }
            else
            {
                item.Quantity = reminder;
            }
        }
    }
}