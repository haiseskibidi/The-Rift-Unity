using UnityEngine;

public class SpeakerTag : MonoBehaviour, ITag
{
    public void Calling(string value)
    {
        var dialogueWindow = GetComponent<DWindow>();
        dialogueWindow.SetName(value);
    }
}
