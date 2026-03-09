using UnityEngine;
using System;

public class MethodTag : MonoBehaviour, ITag
{
    public void Calling(string value)
    {
        var dialogueMethods = GetComponent<DMethods>();
        var method = dialogueMethods.GetType().GetMethod(value);
        method.Invoke(dialogueMethods, null);
    }
}
