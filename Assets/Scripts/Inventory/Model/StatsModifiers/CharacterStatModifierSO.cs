using UnityEngine;

public abstract class CharacterStatModifierSO : ScriptableObject
{
    public abstract void AffectCharacter(GameObject character, float value);
    public abstract void RemoveAffect(GameObject character, float value);
}