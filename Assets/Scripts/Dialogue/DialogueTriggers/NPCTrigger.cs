using UnityEngine;
using System;

public class NPCTrigger : MonoBehaviour
{
    [SerializeField] private TextAsset _inkJSON;
    [SerializeField] private bool useAutoStart = false;
    [SerializeField] private Player player;
    [SerializeField] private bool onlyOneUse = false; 
    [SerializeField] private string triggerID; 

    private bool _isPlayerEnter;
    private DController _dialogueController;
    private DWindow _dialogueWindow;
    private PressFManager pressFManager;

    private void Start()
    {
        _isPlayerEnter = false;
        _dialogueController = FindObjectOfType<DController>();
        _dialogueWindow = FindObjectOfType<DWindow>();
        pressFManager = FindObjectOfType<PressFManager>();

        if (pressFManager == null)
        {
            Debug.LogError("PressFManager не найден в сцене.");
        }

        // Проверяем, был ли триггер уже использован
        if (onlyOneUse)
        {
            bool isUsed = GameManager.Instance.IsTriggerUsed(triggerID);
            if (isUsed)
            {
                // Отключаем триггер, если он уже использовался
                gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        // Обработка нажатия клавиши F для начала диалога
        if (Input.GetKeyDown(KeyCode.F) && _isPlayerEnter)
        {
            StartDialogue();
        }

        if (_dialogueWindow.IsPlaying || !_isPlayerEnter)
        {
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        GameObject obj = collider.gameObject;
        if (obj.GetComponent<Player>() != null)
        {
            _isPlayerEnter = true;
            if (useAutoStart)
            {
                StartDialogue();
                player.SetIdleAnimation();
                player.enabled = false;
            }
            else
            {
                if (pressFManager != null)
                {
                    pressFManager.ShowCanvas(); // Включаем PressFCanvas при подходе
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        GameObject obj = collider.gameObject;
        if (obj.GetComponent<Player>() != null)
        {
            _isPlayerEnter = false;
            
            // Проверяем, не уничтожен ли DController перед вызовом метода
            if (_dialogueController != null)
            {
                _dialogueController.EmergencyCloseDialogue();
            }
            else
            {
                Debug.LogWarning("DController уничтожен или не найден.");
            }

            if (pressFManager != null)
            {
                pressFManager.HideCanvas(); // Выключаем PressFCanvas при уходе
            }
        }
    }

    private void StartDialogue()
    {
        if (UIManager.Instance.RequestOpenUI(UIState.Dialogue))
        {
            Debug.Log($"Начало диалога с триггером ID: {triggerID}");
            // Подписываемся на событие завершения диалога
            if (_dialogueController != null)
            {
                _dialogueController.OnDialogueEnded += OnDialogueEnded;
            }

            _dialogueController.EnterDialogueMode(_inkJSON);
            
            // Устанавливаем анимацию игрока в Idle при начале диалога
            player.SetIdleAnimation();
            
            if (useAutoStart)
            {
                player.enabled = false;
            }
            if (pressFManager != null)
            {
                pressFManager.HideCanvas(); // Выключаем PressFCanvas после взаимодействия
            }

            // Теперь помечаем триггер как использованный и отключаем его после окончания диалога
            // Это делается в методе OnDialogueEnded
        }
        else
        {
            Debug.LogWarning($"Не удалось открыть UI для диалога с триггером ID: {triggerID}");
        }
    }

    private void OnDialogueEnded()
    {
        if (_dialogueController != null)
        {
            _dialogueController.OnDialogueEnded -= OnDialogueEnded;
        }

        if (useAutoStart)
        {
            player.enabled = true;
        }

        // Устанавливаем анимацию игрока в состояние Idle
        player.SetIdleAnimation();

        // Помечаем триггер как использованный и отключаем его, если установлен флаг OnlyOneUse
        if (onlyOneUse)
        {
            Debug.Log($"Диалог завершён. Помечаем триггер ID: {triggerID} как использованный.");
            GameManager.Instance.MarkTriggerAsUsed(triggerID);
            gameObject.SetActive(false); // Отключаем триггер после использования
            Debug.Log($"Триггер ID: {triggerID} деактивирован после использования.");
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от события, чтобы избежать ошибок
        if (_dialogueController != null)
        {
            _dialogueController.OnDialogueEnded -= OnDialogueEnded;
        }
    }
}