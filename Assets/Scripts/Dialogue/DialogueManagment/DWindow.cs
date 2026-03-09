using System;
using System.Collections;
using UnityEngine;
using TMPro;
using Ink.Runtime;

[RequireComponent(typeof(DChoice))]
public class DWindow : MonoBehaviour
{
    [SerializeField] private GameObject _dialogueWindow;
    [SerializeField] private TextMeshProUGUI _displayText;
    [SerializeField] private TextMeshProUGUI _displayName;
    [SerializeField, Range(0f, 20f)] private float _cooldownNewLetter;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _letterSound;

    [SerializeField, Range(0f, 5f)] private float _soundCooldown = 0.3f; 
    private float _soundTimer = 0f;

    private DChoice _dialogueChoice;

    public bool IsStatusAnswer { get; private set; }
    public bool IsPlaying { get; private set; }
    public bool CanContinueToNextLine { get; private set; }

    public float CoolDownNewLetter
    {
        get => _cooldownNewLetter;
        private set
        {
            _cooldownNewLetter = CheckCooldown(value);
        }
    }

    private void Update()
    {
        if (_soundTimer > 0f)
        {
            _soundTimer -= Time.deltaTime;
        }
    }

    private float CheckCooldown(float value)
    {
        if (value < 0)
        {
            throw new ArgumentException("Значение задержки новой буквы не может быть меньше 0");
        }
        return value;
    }

    public void Init()
    {
        IsStatusAnswer = false;
        CanContinueToNextLine = false;

        _dialogueChoice = GetComponent<DChoice>();
        _dialogueChoice.Init();
    }

    public void SetActive(bool isActive)
    {
        IsPlaying = isActive;
        _dialogueWindow.SetActive(isActive);
    }

    public void SetText(string text)
    {
        _displayText.text = text;
    }

    public void Add(string text)
    {
        _displayText.text += text;
    }

    public void Add(char letter)
    {
        _displayText.text += letter;
        PlayLetterSoundWithCooldown();
    }

    public void ClearText()
    {
        _displayText.text = "";
        _displayName.text = "";
        _soundTimer = 0f; // Сброс таймера при очистке текста
    }

    public void SetName(string namePerson)
    {
        _displayName.text = namePerson;
    }

    public void SetCooldown(float cooldown)
    {
        CoolDownNewLetter = cooldown;
    }

    public void MakeChoice()
    {
        if (CanContinueToNextLine == false)
        {
            return;
        }
        
        IsStatusAnswer = false;
    }

    public IEnumerator DisplayLine(Story story)
    {
        string line = story.Continue();

        ClearText();

        _dialogueChoice.HideChoices();

        CanContinueToNextLine = false;
        bool isAddingRichText = false;

        yield return new WaitForSeconds(0.001f);

        foreach (char letter in line.ToCharArray())
        {
            if (Input.GetMouseButtonDown(0))
            {
                SetText(line);
                break;
            }

            isAddingRichText = letter == '<' || isAddingRichText;

            if (letter == '>')
            {
                isAddingRichText = false;
            }

            Add(letter);
            if (isAddingRichText == false)
            {
                yield return new WaitForSeconds(_cooldownNewLetter);
            }
        }

        CanContinueToNextLine = true;

        IsStatusAnswer = _dialogueChoice.DisplayChoices(story);
    }

    private void PlayLetterSoundWithCooldown()
    {
        _soundTimer -= Time.deltaTime;
        if (_soundTimer <= 0f)
        {
            PlayLetterSound();
            _soundTimer = _soundCooldown;
        }
    }

    // Метод для проигрывания звука
    private void PlayLetterSound()
    {
        if (_audioSource != null && _letterSound != null)
        {
            _audioSource.PlayOneShot(_letterSound);
        }
    }
}