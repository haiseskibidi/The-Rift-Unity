using UnityEngine;
using Inventory.Model;

public class WeaponParameterIncreaseModifierSO : CharacterStatModifierSO
{
    [SerializeField]
    private ItemParameterSO targetParameter;

    [SerializeField]
    private int increaseValue;

    /// <summary>
    /// Применяет увеличение параметра текущего экипированного оружия.
    /// </summary>
    public override void AffectCharacter(GameObject character, float value)
    {
        if (character == null)
        {
            Debug.LogWarning("Character GameObject is null.");
            return;
        }

        AgentWeapon agentWeapon = character.GetComponent<AgentWeapon>();
        if (agentWeapon != null && agentWeapon.CurrentWeapon != null)
        {
            agentWeapon.IncreaseWeaponParameter(targetParameter, increaseValue);
            Debug.Log($"Параметр {targetParameter.ParameterName} увеличен на {increaseValue}.");
        }
        else
        {
            Debug.LogWarning("AgentWeapon или текущее оружие не найдены на персонаже.");
        }
    }

    /// <summary>
    /// Удаляет увеличение параметра текущего экипированного оружия.
    /// </summary>
    public override void RemoveAffect(GameObject character, float value)
    {
        if (character == null)
        {
            Debug.LogWarning("Character GameObject is null.");
            return;
        }

        AgentWeapon agentWeapon = character.GetComponent<AgentWeapon>();
        if (agentWeapon != null && agentWeapon.CurrentWeapon != null)
        {
            agentWeapon.DecreaseWeaponParameter(targetParameter, increaseValue);
            Debug.Log($"Параметр {targetParameter.ParameterName} уменьшен на {increaseValue}.");
        }
        else
        {
            Debug.LogWarning("AgentWeapon или текущее оружие не найдены на персонаже.");
        }
    }
}