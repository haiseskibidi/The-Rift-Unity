using UnityEngine;
using TMPro;
using System;
using Ink.Runtime;
using System.Collections.Generic;

public class DChoice : MonoBehaviour
{
    [SerializeField] private GameObject[] _choices;
    private TextMeshProUGUI[] _choicesText;

    public void Init()
    {
        _choicesText = new TextMeshProUGUI[_choices.Length];

        for (int index = 0; index < _choices.Length; index++)
        {
            _choicesText[index] = _choices[index].GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    public bool DisplayChoices(Story story)
    {
        Choice[] currentChoices = story.currentChoices.ToArray();
        
        if (currentChoices.Length > _choices.Length)
        {
            throw new ArgumentException("Ошибка, выборов в сценарии больше чем выборов в диалоге");
        }

        HideChoices();

        ushort index = 0;

        foreach (Choice choice in currentChoices)
        {
            _choices[index].SetActive(true);
            _choicesText[index++].text = choice.text;
        }

        return currentChoices.Length > 0;
    }

    public void HideChoices()
    {
        foreach (var button in _choices)
        {
            button.SetActive(false);
        }
    }
}
