using UnityEngine;
using System;
using Ink.Runtime;

public class ButtonTrigger : MonoBehaviour
{
    [SerializeField] private TextAsset _inkJSON;

    public void ChangeField(int value)
    {
        Story story = new Story(_inkJSON.text);

        story.variablesState["nameField"] = value;
        Debug.Log(story.variablesState["nameField"]);
    }
}
