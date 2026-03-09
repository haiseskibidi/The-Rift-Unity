using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Inventory.Model
{
    public class AgentWeapon : MonoBehaviour
    {
        [SerializeField]
        private EquippableItemSO weapon;

        [SerializeField]
        private StatisticsUI statisticsUI;

        [SerializeField]
        private ItemParameterSO durabilityParameter; 

        [SerializeField]
        private InventorySO inventoryData;

        [SerializeField]
        private List<ItemParameter> itemCurrentParameters;

        [SerializeField]
        private List<ModifierData> modifiersData;

        public EquippableItemSO CurrentWeapon => weapon;
       
        private Dictionary<CharacterStatModifierSO, float> activeModifiers = new Dictionary<CharacterStatModifierSO, float>();

        public event Action<EquippableItemSO> OnWeaponParameterZero;

        /// <summary>
        /// Устанавливает оружие и применяет соответствующие модификаторы.
        /// </summary>
        public void SetWeapon(EquippableItemSO weaponItemSO, List<ItemParameter> itemState)
        {
            if (weapon != null)
            {
                // Удаляем текущие модификаторы
                foreach (var modifier in activeModifiers.Keys.ToList())
                {
                    RemoveModifier(modifier, activeModifiers[modifier]);
                }
                // activeModifiers.Clear(); // Удаляем эту строку
            }

            this.weapon = weaponItemSO;
            this.itemCurrentParameters = itemState;
            this.modifiersData = weaponItemSO.ModifiersData;

            // Применяем новые модификаторы с их значениями
            foreach (var data in weaponItemSO.ModifiersData)
            {
                AddModifier(data.statModifier, data.value);
            }

            if(statisticsUI != null)
            {
                statisticsUI.UpdateStatisticsUI();
            }
        }
        

        private void OnDestroy()
        {
            ClearModifiers();
        }

        private void OnDisable()
        {
            ClearModifiers();
        }

        public ItemParameter GetCurrentWeaponParameter(ItemParameterSO parameterSO)
        {
            return itemCurrentParameters.FirstOrDefault(p => p.itemParameter == parameterSO);
        }
        
        public void UpdateStatisticsUI()
        {
            if (statisticsUI != null)
            {
                statisticsUI.UpdateStatisticsUI();
            }
        }

        /// <summary>
        /// Добавляет модификатор с указанным значением.
        /// </summary>
        public void AddModifier(CharacterStatModifierSO modifier, float value)
        {
            if (activeModifiers.ContainsKey(modifier))
            {
                activeModifiers[modifier] += value;
            }
            else
            {
                activeModifiers.Add(modifier, value);
                modifier.AffectCharacter(gameObject, value);
                Debug.Log($"Модификатор {modifier.name} добавлен со значением {value}.");
            }
        }

        /// <summary>
        /// Удаляет модификатор с указанным значением.
        /// </summary>
        public void RemoveModifier(CharacterStatModifierSO modifier, float value)
        {
            if (activeModifiers.ContainsKey(modifier))
            {
                activeModifiers[modifier] -= value;
                Debug.Log($"Модификатор {modifier.name} уменьшен на {value}. Текущее значение: {activeModifiers[modifier]}.");

                if (activeModifiers[modifier] <= 0)
                {
                    activeModifiers.Remove(modifier);
                    modifier.RemoveAffect(gameObject, value);
                    Debug.Log($"Модификатор {modifier.name} полностью удалён.");
                }
                else
                {
                    modifier.RemoveAffect(gameObject, value);
                }

                if(statisticsUI != null)
                {
                    statisticsUI.UpdateStatisticsUI();
                }
            }
            else
            {
                Debug.LogWarning($"Модификатор {modifier.name} не найден в активных модификаторах.");
            }
        }

        /// <summary>
        /// Удаляет текущее оружие и соответствующие модификаторы.
        /// </summary>
        public void RemoveWeapon(EquippableItemSO weaponToRemove)
        {
            if (weapon == weaponToRemove)
            {
                foreach (var data in weapon.ModifiersData)
                {
                    RemoveModifier(data.statModifier, data.value);
                }
                // activeModifiers.Clear(); // Удаляем эту строку

                weapon = null;

                // Очищаем параметры оружия
                itemCurrentParameters = new List<ItemParameter>();
                modifiersData = new List<ModifierData>();

                if (statisticsUI != null)
                {
                    statisticsUI.UpdateStatisticsUI();
                }

                Debug.Log("Оружие успешно снято.");
            }
            else
            {
                Debug.LogWarning("Попытка снять не экипированное оружие.");
            }
        }

        public void DecreaseWeaponParameter(ItemParameterSO parameterSO, int amount)
        {
            // Поиск параметра оружия по заданному ItemParameterSO
            ItemParameter parameter = itemCurrentParameters
                .FirstOrDefault(p => p.itemParameter == parameterSO);
            
            if (parameter.itemParameter != null)
            {
                // Уменьшение значения параметра на заданное количество
                parameter.value -= amount;
                parameter.value = Mathf.Max(parameter.value, 0); // Обеспечение, что значение не станет отрицательным

                // Обновление списка параметров
                int index = itemCurrentParameters.FindIndex(p => p.itemParameter == parameterSO);
                if (index != -1)
                {
                    itemCurrentParameters[index] = parameter;
                }
                inventoryData?.InformAboutChange();

                // Проверка, достигло ли значение параметра нуля
                if (parameter.value <= 0)
                {
                    Debug.Log($"Параметр {parameterSO.ParameterName} достиг нуля. Оружие может быть разрушено или его эффекты могут быть отключены.");
                    OnWeaponParameterZero?.Invoke(this.weapon);
                }
            }
            else
            {
                Debug.LogWarning($"Параметр {parameterSO.ParameterName} не найден в текущих параметрах оружия.");
            }
        }

        public void IncreaseWeaponParameter(ItemParameterSO parameterSO, int amount)
        {
            ItemParameter parameter = itemCurrentParameters.FirstOrDefault(p => p.itemParameter == parameterSO);
            if (parameter.itemParameter != null)
            {
                parameter.value += amount;
                parameter.value = Mathf.Min(parameter.value, 100); // Ограничение значения параметра до 100

                int index = itemCurrentParameters.FindIndex(p => p.itemParameter == parameterSO);
                if (index != -1)
                {
                    itemCurrentParameters[index] = parameter;
                }
                inventoryData?.InformAboutChange();
            }
        }

        public float GetWeaponParameterValue(ItemParameterSO parameterSO)
        {
            ItemParameter parameter = itemCurrentParameters
                .FirstOrDefault(p => p.itemParameter == parameterSO);
                
            if (parameter.itemParameter != null)
            {
                return parameter.value;
            }

            Debug.LogWarning($"Параметр {parameterSO.ParameterName} не найден.");
            return 0f;
        }

        public void ClearModifiers()
        {
            // Удаляем все активные модификаторы
            foreach (var modifier in activeModifiers.Keys.ToList())
            {
                RemoveModifier(modifier, activeModifiers[modifier]);
            }
            activeModifiers.Clear();
        }
    }
}