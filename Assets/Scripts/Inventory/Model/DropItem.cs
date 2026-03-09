using UnityEngine;

namespace Inventory.Model
{
    [System.Serializable]
    public class DropItem
    {
        public ItemSO item; // Предмет, который может выпасть
        [Range(0f, 100f)]
        public float dropChance = 50f; // Вероятность выпадения в процентах
        public int minQuantity = 1; // Минимальное количество
        public int maxQuantity = 1; // Максимальное количество

        /// <summary>
        /// Определяет количество выпавших предметов на основе распределения.
        /// </summary>
        public int GetDroppedQuantity()
        {
            if (minQuantity == maxQuantity)
                return minQuantity;
            
            // Равномерное распределение для количества (например, 1 или 2)
            return Random.Range(minQuantity, maxQuantity + 1);
        }
    }
}