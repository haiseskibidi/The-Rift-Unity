using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(menuName = "Inventory/EquippableItemSO")]
    public class EquippableItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        public string ActionName => "Equip";

        [field: SerializeField]
        public AudioClip actionSFX { get; private set; }

        [SerializeField]
        private bool canDrop = true;

        public bool CanDrop => canDrop;

        [SerializeField]
        private List<ModifierData> modifiersData = new List<ModifierData>();

        public List<ModifierData> ModifiersData => modifiersData;

        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            AgentWeapon agentWeapon = character.GetComponent<AgentWeapon>();
            if (agentWeapon != null)
            {
                agentWeapon.SetWeapon(this, itemState == null ?
                    DefaultParametersList : itemState);
                return true;
            }
            return false;
        }

        public void RemoveEffect(GameObject character)
        {
            AgentWeapon agentWeapon = character.GetComponent<AgentWeapon>();
            if (agentWeapon != null)
            {
                foreach (var data in modifiersData)
                {
                    agentWeapon.RemoveModifier(data.statModifier, data.value);
                }

                agentWeapon.RemoveWeapon(this);
            }
        }
    }
}