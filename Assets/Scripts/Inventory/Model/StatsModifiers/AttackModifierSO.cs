using UnityEngine;

[CreateAssetMenu(menuName = "Modifiers/AttackModifier")]
public class AttackModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float value)
    {
        Player player = character.GetComponent<Player>();
        if(player != null)
        {
            player.IncreaseAttack(value);
        }
    }

    public override void RemoveAffect(GameObject character, float value)
    {
        Player player = character.GetComponent<Player>();
        if(player != null)
        {
            player.DecreaseAttack(value);
        }
    }
}