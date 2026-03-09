using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonFX : MonoBehaviour
{
    public AudioClip buttonClick;
    public AudioClip buttonHover;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayButtonClick()
    {
        if (audioSource != null && buttonClick != null)
        {
            audioSource.PlayOneShot(buttonClick);
        }
    }

    public void PlayButtonHover()
    {
        if (audioSource != null && buttonHover != null)
        {
            audioSource.PlayOneShot(buttonHover);
        }
    }
}
