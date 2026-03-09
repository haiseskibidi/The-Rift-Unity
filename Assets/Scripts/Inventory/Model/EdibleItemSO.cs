using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(menuName = "Inventory/EdibleItemSO")]
    public class EdibleItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        [SerializeField]
        private List<ModifierData> modifiersData = new List<ModifierData>();

        [SerializeField]
        private bool repairItem = false;

        public string ActionName => "Use";

        [field: SerializeField]
        public AudioClip actionSFX { get; private set; }

        public bool RepairItem
        {
            get => repairItem;
            set => repairItem = value;
        }

        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            if (character == null)
            {
                return false;
            }

            AgentWeapon agentWeapon = character.GetComponent<AgentWeapon>();
            if (repairItem && (agentWeapon == null || agentWeapon.CurrentWeapon == null))
            {
                return false;
            }

            foreach (ModifierData data in modifiersData)
            {
                data.statModifier.AffectCharacter(character, data.value);
            }
            return true;
        }
    }

    public interface IDestroyableItem
    {

    }

    public interface IItemAction
    {
        public string ActionName { get; }
        public AudioClip actionSFX { get; }
        bool PerformAction(GameObject character, List<ItemParameter> itemState);
    }
}