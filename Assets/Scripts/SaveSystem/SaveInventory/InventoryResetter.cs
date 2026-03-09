using UnityEngine;
using UnityEngine.UI;
using Inventory.Model;
using Inventory.Save;

namespace Inventory.UI
{
    public class InventoryResetter : MonoBehaviour
    {
        [SerializeField]
        private InventorySO inventoryData; // Ссылка на InventorySO

        [SerializeField]
        private InventorySaveManager saveManager; // Ссылка на InventorySaveManager

        [SerializeField]
        private Button resetButton; // Кнопка для сброса инвентаря

        private void Start()
        {
            if (resetButton != null)
            {
                resetButton.onClick.AddListener(OnResetButtonClicked);
            }
            else
            {
                Debug.LogError("Reset Button не назначена.");
            }
        }

        private void OnResetButtonClicked()
        {
            if (inventoryData != null && saveManager != null)
            {
                inventoryData.ResetToInitialItems(); // Сброс инвентаря
                saveManager.SaveInventory(inventoryData); // Сохранение состояния
            }
            else
            {
                Debug.LogError("InventorySO или InventorySaveManager не назначены.");
            }
        }
    }
}