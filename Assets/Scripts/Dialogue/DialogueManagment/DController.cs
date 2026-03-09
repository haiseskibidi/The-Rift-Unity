using UnityEngine;
using Ink.Runtime;
using System;
using System.Collections;

[RequireComponent(typeof(DWindow), typeof(DTag))]
public class DController : MonoBehaviour
{
    public event Action OnDialogueEnded;

    private DWindow _dialogueWindow;
    private DTag _dialogueTag;

    public Story CurrentStory { get; private set; }
    private Coroutine _displayLineCoroutine;

    private void Awake()
    {
        _dialogueWindow = GetComponent<DWindow>();
        _dialogueTag = GetComponent<DTag>();

        _dialogueTag.Init();
        _dialogueWindow.Init();
    }

    private void Start()
    {
        _dialogueWindow.SetActive(false);
    }

    private void Update()
    {
        if (_dialogueWindow.IsStatusAnswer == true ||
            _dialogueWindow.IsPlaying == false ||
            _dialogueWindow.CanContinueToNextLine == false)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            ContinueStory();
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON)
    {
        CurrentStory = new Story(inkJSON.text);
        _dialogueWindow.SetActive(true);
        ContinueStory();
    }

    public IEnumerator ExitDialogueMode()
    {
        yield return new WaitForSeconds(_dialogueWindow.CoolDownNewLetter);
        _dialogueWindow.SetActive(false);
        _dialogueWindow.ClearText();
        UIManager.Instance.CloseCurrentUI();
        OnDialogueEnded?.Invoke();
    }

    // Новый метод для экстренного закрытия диалога
    public void EmergencyCloseDialogue()
    {
        if (_displayLineCoroutine != null)
        {
            StopCoroutine(_displayLineCoroutine);
            _displayLineCoroutine = null;
        }

        CurrentStory = null;
        _dialogueWindow.SetActive(false);
        _dialogueWindow.ClearText();
        UIManager.Instance.CloseCurrentUI();
        OnDialogueEnded?.Invoke();
    }

    private void ContinueStory()
    {
        if (CurrentStory.canContinue == false)
        {
            StartCoroutine(ExitDialogueMode());
            return;
        }

        if (_displayLineCoroutine != null)
        {
            StopCoroutine(_displayLineCoroutine);
        }

        _displayLineCoroutine = StartCoroutine(_dialogueWindow.DisplayLine(CurrentStory));

        try
        {
            _dialogueTag.HandleTags(CurrentStory.currentTags);
        }
        catch (ArgumentException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public void MakeChoice(int choiceIndex)
    {
        _dialogueWindow.MakeChoice();
        CurrentStory.ChooseChoiceIndex(choiceIndex);
        ContinueStory();
    }
}