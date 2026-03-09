using UnityEngine;
using System;

public class CooldownTag : MonoBehaviour, ITag
{
    public void Calling(string value)
    {
        float number = (float)Convert.ToDouble(value.Replace('.', ','));

        var dialogueWindow = GetComponent<DWindow>();
        try
        {
            dialogueWindow.SetCooldown(number);
        }
        catch (ArgumentException ex)
        {
            Debug.LogError(ex.Message);
        }
    }
}
