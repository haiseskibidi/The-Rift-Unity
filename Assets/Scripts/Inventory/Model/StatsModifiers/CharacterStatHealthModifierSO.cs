using UnityEngine;

public class CharacterStatHealthModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float value)
    {
        Player player = character.GetComponent<Player>();
        if (player != null)
            player.Heal(value);
    }

    public override void RemoveAffect(GameObject character, float value)
    {
        Player player = character.GetComponent<Player>();
        if (player != null)
            player.PlayerTakeDamage(value);
    }
}